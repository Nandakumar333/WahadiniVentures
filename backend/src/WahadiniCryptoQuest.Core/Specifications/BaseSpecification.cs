using System.Linq.Expressions;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Specifications;

/// <summary>
/// Base specification class for building complex queries
/// Implements the Specification pattern for query logic encapsulation
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class Specification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}

/// <summary>
/// Example specification for fetching users with confirmed emails
/// </summary>
public class ConfirmedUsersSpecification : Specification<User>
{
    public ConfirmedUsersSpecification()
    {
        AddCriteria(u => u.EmailConfirmed);
        ApplyOrderBy(u => u.CreatedAt);
    }
}

/// <summary>
/// Example specification for fetching users by email
/// </summary>
public class UserByEmailSpecification : Specification<User>
{
    public UserByEmailSpecification(string email)
    {
        AddCriteria(u => u.Email.ToLower() == email.ToLower());
    }
}
