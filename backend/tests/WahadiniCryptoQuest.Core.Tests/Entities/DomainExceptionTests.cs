using FluentAssertions;
using WahadiniCryptoQuest.Core.Exceptions;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

/// <summary>
/// Unit tests for custom exception classes
/// Verifies exception creation, messages, inheritance, and behavior
/// </summary>
public class DomainExceptionTests
{
    #region EntityNotFoundException Tests

    [Fact]
    public void EntityNotFoundException_CanBeCreatedWithEntityNameAndKey()
    {
        // Arrange
        const string entityName = "User";
        const string entityKey = "123e4567-e89b-12d3-a456-426614174000";

        // Act
        var exception = new EntityNotFoundException(entityName, entityKey);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain(entityKey);
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public void EntityNotFoundException_MessageIsPreservedCorrectly()
    {
        // Arrange
        const string entityName = "Course";
        const string entityKey = "course-123";

        // Act
        var exception = new EntityNotFoundException(entityName, entityKey);

        // Assert
        exception.Message.Should().Be($"{entityName} with key '{entityKey}' was not found.");
    }

    [Fact]
    public void EntityNotFoundException_InheritsFromDomainException()
    {
        // Arrange & Act
        var exception = new EntityNotFoundException("Entity", "id");

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void EntityNotFoundException_WorksWithDifferentKeyTypes()
    {
        // Arrange & Act
        var stringKeyException = new EntityNotFoundException("User", "user-123");
        var guidKeyException = new EntityNotFoundException("User", Guid.NewGuid());
        var intKeyException = new EntityNotFoundException("Task", 42);

        // Assert
        stringKeyException.Message.Should().Contain("user-123");
        guidKeyException.Message.Should().MatchRegex(@"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}");
        intKeyException.Message.Should().Contain("42");
    }

    [Fact]
    public void EntityNotFoundException_CanBeThrown()
    {
        // Arrange
        Action throwAction = () => throw new EntityNotFoundException("Entity", "id");

        // Act & Assert
        throwAction.Should().Throw<EntityNotFoundException>()
            .And.Message.Should().Contain("Entity")
            .And.Contain("id")
            .And.Contain("not found");
    }

    [Fact]
    public void EntityNotFoundException_CanBeCaught()
    {
        // Arrange
        EntityNotFoundException? caughtException = null;

        // Act
        try
        {
            throw new EntityNotFoundException("TestEntity", "test-id");
        }
        catch (EntityNotFoundException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.Message.Should().Contain("TestEntity");
    }

    #endregion

    #region BusinessRuleValidationException Tests

    [Fact]
    public void BusinessRuleValidationException_CanBeCreatedWithMessage()
    {
        // Arrange
        const string message = "Account is locked due to too many failed login attempts";

        // Act
        var exception = new BusinessRuleValidationException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void BusinessRuleValidationException_InheritsFromDomainException()
    {
        // Arrange & Act
        var exception = new BusinessRuleValidationException("Test message");

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void BusinessRuleValidationException_MessageIsPreservedCorrectly()
    {
        // Arrange
        const string expectedMessage = "Subscription tier upgrade required for this feature";

        // Act
        var exception = new BusinessRuleValidationException(expectedMessage);

        // Assert
        exception.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void BusinessRuleValidationException_CanBeThrown()
    {
        // Arrange
        Action throwAction = () => throw new BusinessRuleValidationException("Business rule violated");

        // Act & Assert
        throwAction.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Business rule violated");
    }

    [Fact]
    public void BusinessRuleValidationException_CanBeCaughtAsDomainException()
    {
        // Arrange
        DomainException? caughtException = null;

        // Act
        try
        {
            throw new BusinessRuleValidationException("Test business rule violation");
        }
        catch (DomainException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<BusinessRuleValidationException>();
    }

    #endregion

    #region DuplicateEntityException Tests

    [Fact]
    public void DuplicateEntityException_CanBeCreatedWithEntityNameAndIdentifier()
    {
        // Arrange
        const string entityName = "User";
        const string identifier = "test@example.com";

        // Act
        var exception = new DuplicateEntityException(entityName, identifier);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain(identifier);
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public void DuplicateEntityException_MessageIsPreservedCorrectly()
    {
        // Arrange
        const string entityName = "Role";
        const string identifier = "Premium";

        // Act
        var exception = new DuplicateEntityException(entityName, identifier);

        // Assert
        exception.Message.Should().Be($"{entityName} with identifier '{identifier}' already exists.");
    }

    [Fact]
    public void DuplicateEntityException_InheritsFromDomainException()
    {
        // Arrange & Act
        var exception = new DuplicateEntityException("Entity", "identifier");

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void DuplicateEntityException_CanBeThrown()
    {
        // Arrange
        Action throwAction = () => throw new DuplicateEntityException("User", "duplicate@example.com");

        // Act & Assert
        throwAction.Should().Throw<DuplicateEntityException>()
            .And.Message.Should().Contain("User")
            .And.Contain("duplicate@example.com")
            .And.Contain("already exists");
    }

    [Fact]
    public void DuplicateEntityException_CanBeCaughtAsDomainException()
    {
        // Arrange
        DomainException? caughtException = null;

        // Act
        try
        {
            throw new DuplicateEntityException("TestEntity", "test-identifier");
        }
        catch (DomainException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<DuplicateEntityException>();
    }

    #endregion

    #region DomainException Base Class Tests

    [Fact]
    public void DomainException_CanBeCaughtAsGenericException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new EntityNotFoundException("Entity", "id");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void DomainException_AllSubclassesInheritCorrectly()
    {
        // Arrange & Act
        var entityNotFound = new EntityNotFoundException("Entity", "id");
        var businessRule = new BusinessRuleValidationException("Rule violated");
        var duplicate = new DuplicateEntityException("Entity", "id");

        // Assert - All should be assignable to DomainException
        entityNotFound.Should().BeAssignableTo<DomainException>();
        businessRule.Should().BeAssignableTo<DomainException>();
        duplicate.Should().BeAssignableTo<DomainException>();

        // Assert - All should be assignable to Exception
        entityNotFound.Should().BeAssignableTo<Exception>();
        businessRule.Should().BeAssignableTo<Exception>();
        duplicate.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region Exception Serialization Tests

    [Fact]
    public void EntityNotFoundException_MessageCanBeSerialized()
    {
        // Arrange
        var exception = new EntityNotFoundException("User", "user-123");

        // Act
        var act = () => System.Text.Json.JsonSerializer.Serialize(new { error = exception.Message });

        // Assert
        act.Should().NotThrow();
        var json = act.Invoke();
        json.Should().Contain("User");
        json.Should().Contain("user-123");
    }

    [Fact]
    public void BusinessRuleValidationException_MessageCanBeSerialized()
    {
        // Arrange
        var exception = new BusinessRuleValidationException("Account locked");

        // Act
        var act = () => System.Text.Json.JsonSerializer.Serialize(new { error = exception.Message });

        // Assert
        act.Should().NotThrow();
        var json = act.Invoke();
        json.Should().Contain("Account locked");
    }

    [Fact]
    public void DuplicateEntityException_MessageCanBeSerialized()
    {
        // Arrange
        var exception = new DuplicateEntityException("User", "test@example.com");

        // Act
        var act = () => System.Text.Json.JsonSerializer.Serialize(new { error = exception.Message });

        // Assert
        act.Should().NotThrow();
        var json = act.Invoke();
        json.Should().Contain("User");
        json.Should().Contain("test@example.com");
    }

    #endregion

    #region Real-World Usage Scenarios

    [Fact]
    public void ExceptionHandling_WorksInRepositoryPattern()
    {
        // Arrange
        Func<string> findUser = () =>
        {
            // Simulate repository lookup failure
            throw new EntityNotFoundException("User", "non-existent-id");
        };

        // Act & Assert
        findUser.Should().Throw<EntityNotFoundException>()
            .WithMessage("*User*non-existent-id*not found*");
    }

    [Fact]
    public void ExceptionHandling_WorksInBusinessLogic()
    {
        // Arrange
        Action validateBusinessRule = () =>
        {
            // Simulate business rule validation
            var accountLocked = true;
            if (accountLocked)
            {
                throw new BusinessRuleValidationException("Account is locked due to failed login attempts");
            }
        };

        // Act & Assert
        validateBusinessRule.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("*locked*");
    }

    [Fact]
    public void ExceptionHandling_WorksInDuplicateDetection()
    {
        // Arrange
        Action checkDuplicate = () =>
        {
            // Simulate duplicate detection
            var emailExists = true;
            if (emailExists)
            {
                throw new DuplicateEntityException("User", "duplicate@example.com");
            }
        };

        // Act & Assert
        checkDuplicate.Should().Throw<DuplicateEntityException>()
            .WithMessage("*already exists*");
    }

    #endregion
}
