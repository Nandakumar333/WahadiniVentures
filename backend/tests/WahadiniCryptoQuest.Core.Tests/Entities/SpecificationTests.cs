using FluentAssertions;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Specifications;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

/// <summary>
/// Unit tests for Specification pattern implementation
/// Tests query building, filtering, ordering, paging, and composition logic
/// </summary>
public class SpecificationTests
{
    #region Test Specification Implementations

    /// <summary>
    /// Test specification for users created after a specific date
    /// </summary>
    private class UsersCreatedAfterSpecification : Specification<User>
    {
        public UsersCreatedAfterSpecification(DateTime date)
        {
            AddCriteria(u => u.CreatedAt > date);
        }
    }

    /// <summary>
    /// Test specification with paging
    /// </summary>
    private class PagedUsersSpecification : Specification<User>
    {
        public PagedUsersSpecification(int pageNumber, int pageSize)
        {
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
            ApplyOrderBy(u => u.CreatedAt);
        }
    }

    /// <summary>
    /// Test specification with ordering
    /// </summary>
    private class UsersOrderedByEmailSpecification : Specification<User>
    {
        public UsersOrderedByEmailSpecification(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(u => u.Email);
            else
                ApplyOrderBy(u => u.Email);
        }
    }

    #endregion

    #region Base Specification Tests

    [Fact]
    public void Specification_Criteria_CanBeSet()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-30);

        // Act
        var spec = new UsersCreatedAfterSpecification(date);

        // Assert
        spec.Criteria.Should().NotBeNull();
        spec.Criteria!.Body.Should().NotBeNull();
    }

    [Fact]
    public void Specification_OrderBy_CanBeSet()
    {
        // Arrange & Act
        var spec = new UsersOrderedByEmailSpecification();

        // Assert
        spec.OrderBy.Should().NotBeNull();
        spec.OrderByDescending.Should().BeNull();
    }

    [Fact]
    public void Specification_OrderByDescending_CanBeSet()
    {
        // Arrange & Act
        var spec = new UsersOrderedByEmailSpecification(descending: true);

        // Assert
        spec.OrderByDescending.Should().NotBeNull();
        spec.OrderBy.Should().BeNull();
    }

    [Fact]
    public void Specification_Paging_CanBeApplied()
    {
        // Arrange
        const int pageNumber = 2;
        const int pageSize = 10;

        // Act
        var spec = new PagedUsersSpecification(pageNumber, pageSize);

        // Assert
        spec.IsPagingEnabled.Should().BeTrue();
        spec.Skip.Should().Be(10); // (2-1) * 10
        spec.Take.Should().Be(10);
    }

    [Fact]
    public void Specification_Paging_CalculatesSkipCorrectly()
    {
        // Arrange & Act
        var page1 = new PagedUsersSpecification(1, 20);
        var page2 = new PagedUsersSpecification(2, 20);
        var page3 = new PagedUsersSpecification(3, 20);

        // Assert
        page1.Skip.Should().Be(0);  // (1-1) * 20
        page2.Skip.Should().Be(20); // (2-1) * 20
        page3.Skip.Should().Be(40); // (3-1) * 20
        
        page1.Take.Should().Be(20);
        page2.Take.Should().Be(20);
        page3.Take.Should().Be(20);
    }

    [Fact]
    public void Specification_DefaultState_IsCorrect()
    {
        // Arrange & Act
        var spec = new UsersCreatedAfterSpecification(DateTime.UtcNow);

        // Assert
        spec.IsPagingEnabled.Should().BeFalse();
        spec.Skip.Should().Be(0);
        spec.Take.Should().Be(0);
        spec.Includes.Should().BeEmpty();
        spec.IncludeStrings.Should().BeEmpty();
        spec.OrderBy.Should().BeNull();
        spec.OrderByDescending.Should().BeNull();
    }

    #endregion

    #region ConfirmedUsersSpecification Tests

    [Fact]
    public void ConfirmedUsersSpecification_HasCorrectCriteria()
    {
        // Arrange & Act
        var spec = new ConfirmedUsersSpecification();

        // Assert
        spec.Criteria.Should().NotBeNull();
        spec.Criteria!.Body.Should().NotBeNull();
    }

    [Fact]
    public void ConfirmedUsersSpecification_FiltersConfirmedUsersOnly()
    {
        // Arrange
        var spec = new ConfirmedUsersSpecification();
        
        var confirmedUser = User.Create("confirmed@example.com", "hashedPassword", "Test", "User");
        confirmedUser.ConfirmEmail();
        
        var unconfirmedUser = User.Create("unconfirmed@example.com", "hashedPassword", "Test", "User");
        
        var users = new List<User> { confirmedUser, unconfirmedUser };

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var filteredUsers = users.Where(compiledCriteria).ToList();

        // Assert
        filteredUsers.Should().HaveCount(1);
        filteredUsers[0].Email.Should().Be("confirmed@example.com");
        filteredUsers[0].EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ConfirmedUsersSpecification_HasOrderByCreatedAt()
    {
        // Arrange & Act
        var spec = new ConfirmedUsersSpecification();

        // Assert
        spec.OrderBy.Should().NotBeNull();
    }

    [Fact]
    public void ConfirmedUsersSpecification_DoesNotFilterUnconfirmedUsers()
    {
        // Arrange
        var spec = new ConfirmedUsersSpecification();
        var unconfirmedUser = User.Create("test@example.com", "hashedPassword", "Test", "User");

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var result = compiledCriteria(unconfirmedUser);

        // Assert
        result.Should().BeFalse(); // Unconfirmed user should not match
    }

    [Fact]
    public void ConfirmedUsersSpecification_FiltersMultipleConfirmedUsers()
    {
        // Arrange
        var spec = new ConfirmedUsersSpecification();
        
        var user1 = User.Create("user1@example.com", "hashedPassword", "Test", "User");
        user1.ConfirmEmail();
        
        var user2 = User.Create("user2@example.com", "hashedPassword", "Test", "User");
        user2.ConfirmEmail();
        
        var user3 = User.Create("user3@example.com", "hashedPassword", "Test", "User");
        // user3 not confirmed
        
        var users = new List<User> { user1, user2, user3 };

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var filteredUsers = users.Where(compiledCriteria).ToList();

        // Assert
        filteredUsers.Should().HaveCount(2);
        filteredUsers.Should().OnlyContain(u => u.EmailConfirmed);
    }

    #endregion

    #region UserByEmailSpecification Tests

    [Fact]
    public void UserByEmailSpecification_HasCorrectCriteria()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        var spec = new UserByEmailSpecification(email);

        // Assert
        spec.Criteria.Should().NotBeNull();
    }

    [Fact]
    public void UserByEmailSpecification_FiltersUserByEmail()
    {
        // Arrange
        const string searchEmail = "test@example.com";
        var spec = new UserByEmailSpecification(searchEmail);
        
        var matchingUser = User.Create("test@example.com", "hashedPassword", "Test", "User");
        var otherUser = User.Create("other@example.com", "hashedPassword", "Test", "User");
        
        var users = new List<User> { matchingUser, otherUser };

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var filteredUsers = users.Where(compiledCriteria).ToList();

        // Assert
        filteredUsers.Should().HaveCount(1);
        filteredUsers[0].Email.Should().Be("test@example.com");
    }

    [Fact]
    public void UserByEmailSpecification_IsCaseInsensitive()
    {
        // Arrange
        const string searchEmail = "TEST@EXAMPLE.COM";
        var spec = new UserByEmailSpecification(searchEmail);
        
        var user = User.Create("test@example.com", "hashedPassword", "Test", "User");

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var result = compiledCriteria(user);

        // Assert
        result.Should().BeTrue(); // Should match regardless of case
    }

    [Fact]
    public void UserByEmailSpecification_DoesNotMatchDifferentEmail()
    {
        // Arrange
        const string searchEmail = "search@example.com";
        var spec = new UserByEmailSpecification(searchEmail);
        
        var user = User.Create("different@example.com", "hashedPassword", "Test", "User");

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var result = compiledCriteria(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserByEmailSpecification_HandlesEmptyEmailGracefully()
    {
        // Arrange
        var spec = new UserByEmailSpecification(string.Empty);
        var user = User.Create("test@example.com", "hashedPassword", "Test", "User");

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var result = compiledCriteria(user);

        // Assert
        result.Should().BeFalse(); // Empty string should not match any email
    }

    [Fact]
    public void UserByEmailSpecification_MatchesExactEmailOnly()
    {
        // Arrange
        const string searchEmail = "test@example.com";
        var spec = new UserByEmailSpecification(searchEmail);
        
        var exactMatch = User.Create("test@example.com", "hashedPassword", "Test", "User");
        var partialMatch = User.Create("test@example.com.au", "hashedPassword", "Test", "User");
        
        var users = new List<User> { exactMatch, partialMatch };

        // Act
        var compiledCriteria = spec.Criteria!.Compile();
        var filteredUsers = users.Where(compiledCriteria).ToList();

        // Assert
        filteredUsers.Should().HaveCount(1);
        filteredUsers[0].Email.Should().Be("test@example.com");
    }

    #endregion

    #region Real-World Usage Scenarios

    [Fact]
    public void Specification_CanBeUsedForDashboardQueries()
    {
        // Arrange - Simulate dashboard query: confirmed users, ordered by creation date
        var confirmedSpec = new ConfirmedUsersSpecification();
        
        var users = new List<User>();
        for (int i = 0; i < 5; i++)
        {
            var user = User.Create($"user{i}@example.com", "hashedPassword", "Test", "User");
            if (i % 2 == 0) user.ConfirmEmail(); // Confirm every other user
            users.Add(user);
        }

        // Act
        var compiledCriteria = confirmedSpec.Criteria!.Compile();
        var filteredUsers = users.Where(compiledCriteria).ToList();

        // Assert
        filteredUsers.Should().HaveCount(3); // 0, 2, 4 (every other user)
        filteredUsers.Should().OnlyContain(u => u.EmailConfirmed);
    }

    [Fact]
    public void Specification_CanBeUsedForSearchFunctionality()
    {
        // Arrange
        var searchEmail = "john@example.com";
        var searchSpec = new UserByEmailSpecification(searchEmail);
        
        var users = new List<User>
        {
            User.Create("john@example.com", "hashedPassword", "John", "Doe"),
            User.Create("jane@example.com", "hashedPassword", "Jane", "Doe"),
            User.Create("bob@example.com", "hashedPassword", "Bob", "Smith")
        };

        // Act
        var compiledCriteria = searchSpec.Criteria!.Compile();
        var result = users.SingleOrDefault(compiledCriteria);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Specification_PagingWorksCorrectlyWithLargeDataset()
    {
        // Arrange
        var users = new List<User>();
        for (int i = 0; i < 100; i++)
        {
            users.Add(User.Create($"user{i}@example.com", "hashedPassword", "Test", "User"));
        }

        var page1Spec = new PagedUsersSpecification(1, 10);
        var page5Spec = new PagedUsersSpecification(5, 10);

        // Act & Assert
        page1Spec.Skip.Should().Be(0);
        page1Spec.Take.Should().Be(10);
        
        page5Spec.Skip.Should().Be(40); // (5-1) * 10
        page5Spec.Take.Should().Be(10);
    }

    #endregion
}
