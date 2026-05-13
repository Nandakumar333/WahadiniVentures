using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Subscription;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for creating Stripe checkout sessions
/// Validates user, currency, and tier before delegating to payment gateway
/// </summary>
public class CreateCheckoutSessionHandler : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionResponseDto>
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUserRepository _userRepository;
    private readonly ICurrencyPricingRepository _currencyPricingRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateCheckoutSessionHandler> _logger;

    public CreateCheckoutSessionHandler(
        IPaymentGateway paymentGateway,
        IUserRepository userRepository,
        ICurrencyPricingRepository currencyPricingRepository,
        ISubscriptionRepository subscriptionRepository,
        IMediator mediator,
        ILogger<CreateCheckoutSessionHandler> logger)
    {
        _paymentGateway = paymentGateway;
        _userRepository = userRepository;
        _currencyPricingRepository = currencyPricingRepository;
        _subscriptionRepository = subscriptionRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CheckoutSessionResponseDto> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for checkout session", request.UserId);
            throw new InvalidOperationException("User not found");
        }

        // Parse and validate tier
        if (!Enum.TryParse<SubscriptionTier>(request.Tier, true, out var tier) || tier == SubscriptionTier.Free)
        {
            _logger.LogWarning("Invalid tier {Tier} for checkout session", request.Tier);
            throw new ArgumentException($"Invalid subscription tier: {request.Tier}");
        }

        // Validate currency is supported and active
        var currencyPricing = await _currencyPricingRepository.GetByCurrencyCodeAsync(request.CurrencyCode, cancellationToken);
        if (currencyPricing == null || !currencyPricing.IsActive)
        {
            _logger.LogWarning("Currency {Currency} not supported or inactive", request.CurrencyCode);
            throw new ArgumentException($"Currency {request.CurrencyCode} is not supported");
        }

        // Check if user already has active subscription
        var existingSubscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(request.UserId, cancellationToken);
        if (existingSubscription != null && existingSubscription.HasPremiumAccess())
        {
            _logger.LogWarning("User {UserId} already has active subscription {SubscriptionId}", request.UserId, existingSubscription.Id);
            throw new InvalidOperationException("User already has an active subscription");
        }

        // Validate discount code if provided
        decimal? discountPercentage = null;
        string? validatedDiscountCode = null;
        if (!string.IsNullOrWhiteSpace(request.DiscountCode))
        {
            var validationResult = await _mediator.Send(new ValidateDiscountForCheckoutCommand
            {
                UserId = request.UserId,
                DiscountCode = request.DiscountCode,
                Tier = request.Tier
            }, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid discount code {Code} for user {UserId}: {Error}",
                    request.DiscountCode, request.UserId, validationResult.ErrorMessage);
                throw new ArgumentException(validationResult.ErrorMessage);
            }

            discountPercentage = validationResult.DiscountAmount;
            validatedDiscountCode = request.DiscountCode;
            _logger.LogInformation("Validated discount code {Code} for user {UserId} - {Amount}% off",
                request.DiscountCode, request.UserId, discountPercentage);
        }

        // Create checkout session via payment gateway
        var result = await _paymentGateway.CreateCheckoutSessionAsync(
            request.UserId,
            user.Email,
            tier,
            request.CurrencyCode,
            discountPercentage,
            validatedDiscountCode
        );

        _logger.LogInformation(
            "Created checkout session {SessionId} for user {UserId} - {Tier} {Currency}",
            result.SessionId, request.UserId, tier, request.CurrencyCode);

        return new CheckoutSessionResponseDto
        {
            SessionId = result.SessionId,
            CheckoutUrl = result.CheckoutUrl
        };
    }
}
