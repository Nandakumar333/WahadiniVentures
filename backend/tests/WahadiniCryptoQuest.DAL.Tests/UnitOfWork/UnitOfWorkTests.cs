using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.UnitOfWork;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.UnitOfWork;

public class UnitOfWorkTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _unitOfWork = new DAL.UnitOfWork.UnitOfWork(_context);
    }

    [Fact]
    public async Task CompleteAsync_CommitsAllChangesInSingleTransaction()
    {
        // Arrange
        var user = User.Create(
            "unitofwork@example.com",
            "hashedPassword123",
            "Unit",
            "Work"
        );

        // Act
        await _unitOfWork.Users.AddAsync(user);
        var result = await _unitOfWork.CompleteAsync();

        // Assert
        result.Should().Be(1);
        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("unitofwork@example.com");
    }

    [Fact]
    public async Task CompleteAsync_RollsBackOnException()
    {
        // Arrange - Create a separate context that will be disposed to cause exception
        var separateOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var separateContext = new ApplicationDbContext(separateOptions);
        var separateUnitOfWork = new WahadiniCryptoQuest.DAL.UnitOfWork.UnitOfWork(separateContext);

        var user = User.Create(
            "rollback@example.com",
            "hashedPassword123",
            "Rollback",
            "Test"
        );

        await separateUnitOfWork.Users.AddAsync(user);
        
        // Dispose context to simulate a database failure
        separateContext.Dispose();
        
        Func<Task> act = async () => await separateUnitOfWork.CompleteAsync();

        // Assert - Should throw ObjectDisposedException
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public void Users_ProvidesAccessToUserRepository()
    {
        // Act
        var userRepository = _unitOfWork.Users;

        // Assert
        userRepository.Should().NotBeNull();
        userRepository.Should().BeAssignableTo<Core.Interfaces.Repositories.IUserRepository>();
    }

    [Fact]
    public void EmailVerificationTokens_ProvidesAccessToEmailVerificationTokenRepository()
    {
        // Act
        var tokenRepository = _unitOfWork.EmailVerificationTokens;

        // Assert
        tokenRepository.Should().NotBeNull();
        tokenRepository.Should().BeAssignableTo<Core.Interfaces.Repositories.IEmailVerificationTokenRepository>();
    }

    [Fact]
    public void RefreshTokens_ProvidesAccessToRefreshTokenRepository()
    {
        // Act
        var tokenRepository = _unitOfWork.RefreshTokens;

        // Assert
        tokenRepository.Should().NotBeNull();
        tokenRepository.Should().BeAssignableTo<Core.Interfaces.Repositories.IRefreshTokenRepository>();
    }

    [Fact]
    public void PasswordResetTokens_ProvidesAccessToPasswordResetTokenRepository()
    {
        // Act
        var tokenRepository = _unitOfWork.PasswordResetTokens;

        // Assert
        tokenRepository.Should().NotBeNull();
        tokenRepository.Should().BeAssignableTo<Core.Interfaces.Repositories.IPasswordResetTokenRepository>();
    }

    [Fact]
    public void Repositories_AreLazilyInitialized()
    {
        // Arrange
        var unitOfWork = new DAL.UnitOfWork.UnitOfWork(_context);

        // Act - Access same repository twice
        var userRepo1 = unitOfWork.Users;
        var userRepo2 = unitOfWork.Users;

        // Assert - Should return same instance (lazy initialization)
        userRepo1.Should().BeSameAs(userRepo2);
    }

    [Fact]
    public async Task CompleteAsync_ReturnsCountOfAffectedEntities()
    {
        // Arrange
        var user1 = User.Create(
            "count1@example.com",
            "hashedPassword123",
            "Count",
            "One"
        );

        var user2 = User.Create(
            "count2@example.com",
            "hashedPassword456",
            "Count",
            "Two"
        );

        // Act
        await _unitOfWork.Users.AddAsync(user1);
        await _unitOfWork.Users.AddAsync(user2);
        var result = await _unitOfWork.CompleteAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task MultipleRepositoryOperations_AreCommittedTogether()
    {
        // Arrange
        var user = User.Create(
            "multi@example.com",
            "hashedPassword123",
            "Multi",
            "Operation"
        );

        var emailToken = EmailVerificationToken.Create(user.Id, 24);

        // Act
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.EmailVerificationTokens.AddAsync(emailToken);
        var result = await _unitOfWork.CompleteAsync();

        // Assert
        result.Should().Be(2);
        var savedUser = await _context.Users.FindAsync(user.Id);
        var savedToken = await _context.EmailVerificationTokens.FindAsync(emailToken.Id);
        savedUser.Should().NotBeNull();
        savedToken.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_DisposesDbContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        var unitOfWork = new WahadiniCryptoQuest.DAL.UnitOfWork.UnitOfWork(context);

        // Act
        unitOfWork.Dispose();

        // Assert - Attempting to use disposed context should throw
        Action act = () => context.Users.ToList();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task CompleteAsync_WithNoChanges_ReturnsZero()
    {
        // Act
        var result = await _unitOfWork.CompleteAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CompleteAsync_WithUpdate_ReturnsCountOfUpdatedEntities()
    {
        // Arrange
        var user = User.Create(
            "update@example.com",
            "hashedPassword123",
            "Original",
            "Name"
        );

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act - Update the user using domain method
        user.UpdateProfile("Updated", "Name");
        await _unitOfWork.Users.UpdateAsync(user);
        var result = await _unitOfWork.CompleteAsync();

        // Assert
        result.Should().Be(1);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task CompleteAsync_WithDelete_ReturnsCountOfDeletedEntities()
    {
        // Arrange
        var user = User.Create(
            "delete@example.com",
            "hashedPassword123",
            "To",
            "Delete"
        );

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act - Delete the user
        await _unitOfWork.Users.DeleteAsync(user);
        var result = await _unitOfWork.CompleteAsync();

        // Assert
        result.Should().Be(1);
        var deletedUser = await _context.Users.FindAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_CanBeCalledMultipleTimes()
    {
        // Arrange
        var user1 = User.Create(
            "multiple1@example.com",
            "hashedPassword123",
            "First",
            "Call"
        );

        var user2 = User.Create(
            "multiple2@example.com",
            "hashedPassword456",
            "Second",
            "Call"
        );

        // Act - First call
        await _unitOfWork.Users.AddAsync(user1);
        var result1 = await _unitOfWork.CompleteAsync();

        // Act - Second call
        await _unitOfWork.Users.AddAsync(user2);
        var result2 = await _unitOfWork.CompleteAsync();

        // Assert
        result1.Should().Be(1);
        result2.Should().Be(1);
        var count = await _context.Users.CountAsync();
        count.Should().Be(2);
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _context?.Dispose();
    }
}
