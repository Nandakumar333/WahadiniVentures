using Serilog.Core;
using Serilog.Events;

namespace WahadiniCryptoQuest.API.Logging;

/// <summary>
/// Serilog enricher to add correlation IDs to all log entries
/// Phase 9.4: Observability - request tracking across distributed systems
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        // Get correlation ID from header or generate new one
        var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                          ?? httpContext.TraceIdentifier;

        var correlationIdProperty = propertyFactory.CreateProperty(
            CorrelationIdPropertyName,
            correlationId);

        logEvent.AddPropertyIfAbsent(correlationIdProperty);

        // Also add to response headers for client tracking
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.Headers["X-Correlation-ID"] = correlationId;
        }
    }
}

/// <summary>
/// Middleware to ensure correlation IDs are set for all requests
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or generate correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                          ?? Guid.NewGuid().ToString();

        // Store in HttpContext items for easy access
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>
/// Extension methods for correlation ID middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items["CorrelationId"] as string;
    }
}
