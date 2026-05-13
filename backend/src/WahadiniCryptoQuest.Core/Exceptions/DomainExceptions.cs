namespace WahadiniCryptoQuest.Core.Exceptions;

/// <summary>
/// Base exception for all domain-specific exceptions
/// Follows Clean Architecture pattern by keeping domain logic separate from infrastructure concerns
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found in the domain
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when an entity already exists (duplicate)
/// </summary>
public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityName, string identifier)
        : base($"{entityName} with identifier '{identifier}' already exists.")
    {
    }
}

/// <summary>
/// Exception thrown when a user attempts to access premium content without a premium subscription
/// </summary>
public class PremiumAccessDeniedException : DomainException
{
    public PremiumAccessDeniedException(string message) : base(message)
    {
    }

    public PremiumAccessDeniedException()
        : base("This content requires a premium subscription. Please upgrade your account to access premium features.")
    {
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}
