using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class ReferralAttributionRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReferralAttributionRepository _repository;
    private readonly UserRepository _userRepository;

    public ReferralAttributionRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ReferralAttributionRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByInviteeUserIdAsync_ShouldReturnAttribution()
    {
        // Arrange
        var referrer = await CreateUserAsync("referrer@example.com");
        var invitee = await CreateUserAsync("invitee@example.com");
        
        var attribution = ReferralAttribution.Create(referrer.Id, invitee.Id);
        await _repository.AddAsync(attribution);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByInviteeUserIdAsync(invitee.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ReferrerId.Should().Be(referrer.Id);
        result.ReferredUserId.Should().Be(invitee.Id);
    }

    [Fact]
    public async Task GetReferralsByInviterAsync_ShouldReturnAllReferrals()
    {
        // Arrange
        var referrer = await CreateUserAsync("referrer@example.com");
        var invitee1 = await CreateUserAsync("invitee1@example.com");
        var invitee2 = await CreateUserAsync("invitee2@example.com");

        await _repository.AddAsync(ReferralAttribution.Create(referrer.Id, invitee1.Id));
        await _repository.AddAsync(ReferralAttribution.Create(referrer.Id, invitee2.Id));
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetReferralsByInviterAsync(referrer.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.ReferredUserId == invitee1.Id);
        result.Should().Contain(r => r.ReferredUserId == invitee2.Id);
    }

    [Fact]
    public async Task GetSuccessfulReferralsAsync_ShouldReturnOnlyClaimedReferrals()
    {
        // Arrange
        var referrer = await CreateUserAsync("referrer@example.com");
        var invitee1 = await CreateUserAsync("invitee1@example.com");
        var invitee2 = await CreateUserAsync("invitee2@example.com");

        var attribution1 = ReferralAttribution.Create(referrer.Id, invitee1.Id);
        attribution1.RewardClaimed = true;
        attribution1.RewardClaimedAt = DateTime.UtcNow;

        var attribution2 = ReferralAttribution.Create(referrer.Id, invitee2.Id);
        // attribution2 is not claimed

        await _repository.AddAsync(attribution1);
        await _repository.AddAsync(attribution2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSuccessfulReferralsAsync(referrer.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().ReferredUserId.Should().Be(invitee1.Id);
    }

    [Fact]
    public async Task GetSuccessfulReferralCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var referrer = await CreateUserAsync("referrer@example.com");
        var invitee1 = await CreateUserAsync("invitee1@example.com");
        var invitee2 = await CreateUserAsync("invitee2@example.com");

        var attribution1 = ReferralAttribution.Create(referrer.Id, invitee1.Id);
        attribution1.RewardClaimed = true;
        attribution1.RewardClaimedAt = DateTime.UtcNow;

        var attribution2 = ReferralAttribution.Create(referrer.Id, invitee2.Id);
        // attribution2 is not claimed

        await _repository.AddAsync(attribution1);
        await _repository.AddAsync(attribution2);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetSuccessfulReferralCountAsync(referrer.Id);

        // Assert
        count.Should().Be(1);
    }

    private async Task<User> CreateUserAsync(string email)
    {
        var user = User.Create(email, "password", "First", "Last");
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
