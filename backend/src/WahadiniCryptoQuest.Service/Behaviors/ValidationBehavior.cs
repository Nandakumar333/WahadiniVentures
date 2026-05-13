using FluentValidation;
using MediatR;

namespace WahadiniCryptoQuest.Service.Behaviors;

/// <summary>
/// MediatR pipeline behavior for validating commands using FluentValidation
/// Runs before the command handler and throws ValidationException if validation fails
/// </summary>
/// <typeparam name="TRequest">The command/query type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only validate if validators are available
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Throw validation exception if there are failures
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // Proceed to next behavior/handler
        return await next();
    }
}