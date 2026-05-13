using WahadiniCryptoQuest.API.Extensions;
using WahadiniCryptoQuest.API.Middleware;
using WahadiniCryptoQuest.API.Logging;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Interfaces; // For ITaskSubmissionService, IFileStorageService
using WahadiniCryptoQuest.Core.Interfaces.Repositories; // For IUnitOfWork
using WahadiniCryptoQuest.Service.Services; // For TaskSubmissionService, FileStorageService
using WahadiniCryptoQuest.DAL.Repositories; // For UnitOfWork implementation
using Microsoft.EntityFrameworkCore;
using Serilog;
using WahadiniCryptoQuest.DAL.Services;

// Configure Serilog
LoggingExtensions.ConfigureSerilog();

try
{
    Log.Information("Starting WahadiniCryptoQuest API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the logging pipeline
    builder.AddSerilogLogging();

    // Configure services using extension methods
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddIdentityServices();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddCorsConfiguration(builder.Configuration); // T184: Pass configuration for CORS origins
    builder.Services.AddSwaggerConfiguration();
    builder.Services.AddMediatRConfiguration();
    builder.Services.AddAutoMapperProfiles();
    builder.Services.AddApplicationServices();

    // Add Response Caching for performance
    builder.Services.AddResponseCaching();

    // Add Response Compression for reduced bandwidth
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    });

    // Add Memory Cache for application-level caching
    builder.Services.AddMemoryCache();

    // Add Rate Limiting (T043-T044: Configure rate limit policies)
    builder.Services.AddRateLimitingPolicies();

    // Add HSTS configuration for production
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(ApplicationConstants.Hsts.MaxAgeDays);
    });

    // Add controllers
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // Add comprehensive health checks (Phase 9.3: Production reliability)
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<WahadiniCryptoQuest.DAL.Context.ApplicationDbContext>("database")
        .AddCheck<WahadiniCryptoQuest.API.HealthChecks.DatabaseHealthCheck>("database_detailed")
        .AddCheck<WahadiniCryptoQuest.API.HealthChecks.MemoryHealthCheck>("memory")
        .AddCheck<WahadiniCryptoQuest.API.HealthChecks.StripeHealthCheck>("stripe");

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ITaskSubmissionService, TaskSubmissionService>();
    builder.Services.AddScoped<IFileStorageService, FileStorageService>();

    // Repositories (Assuming ServiceExtensions.cs might already register some, but adding here to be sure for new ones)
    builder.Services.AddScoped<IUserTaskSubmissionRepository, UserTaskSubmissionRepository>();
    builder.Services.AddScoped<ILearningTaskRepository, LearningTaskRepository>();

    // Background Jobs
    builder.Services.AddScoped<WahadiniCryptoQuest.Service.BackgroundJobs.DeduplicationCleanupJob>();

    // Background Services
    builder.Services.AddHostedService<WahadiniCryptoQuest.API.BackgroundServices.FileCleanupService>();
    builder.Services.AddHostedService<WahadiniCryptoQuest.API.BackgroundServices.DeduplicationCleanupService>();

    var app = builder.Build();

    // Seed database with default data (development and testing)
    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<WahadiniCryptoQuest.DAL.Context.ApplicationDbContext>();

                // Skip seeding in test environment to avoid delays
                if (!app.Environment.IsEnvironment("Testing"))
                {
                    // Apply pending migrations
                    await context.Database.MigrateAsync();

                    var seeder = new WahadiniCryptoQuest.DAL.Seeders.DefaultDataSeeder(context);
                    await seeder.SeedAsync();
                    Log.Information("Database seeding completed successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "An error occurred while seeding the database - continuing anyway");
                // Don't rethrow - allow application to continue even if seeding fails
            }
        }
    }

    // Add global exception handler (must be first in pipeline)
    app.UseGlobalExceptionHandler();

    // Add correlation ID middleware for request tracking (Phase 9.4: Observability)
    app.UseCorrelationId();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "WahadiniCryptoQuest API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "WahadiniCryptoQuest API Documentation";
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
        });
    }

    // Enable HTTPS redirect and HSTS only outside Development
    // (avoids spurious warnings when running on HTTP-only in dev)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
        app.UseHsts();
    }

    // CORS must be early in the pipeline so preflight OPTIONS requests
    // are handled before rate limiting, caching, and auth middleware
    app.UseCors("AllowFrontend");

    // Enable Response Compression (before static files and controllers)
    app.UseResponseCompression();

    // Enable Response Caching
    app.UseResponseCaching();

    // Security headers
    app.Use(async (context, next) =>
    {
        // Prevent MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking attacks
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // Enable XSS protection (legacy browsers)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Control referrer information
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Content Security Policy - restrict resource loading
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'";

        // Restrict cross-domain policies
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Remove server header for security
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        await next();
    });

    // Rate Limiting - Prevents API overload and abuse (skip in test environment)
    if (!app.Environment.IsEnvironment("Testing"))
    {
        // Use ASP.NET Core rate limiter for endpoint-specific policies (T043-T044)
        app.UseRateLimiter();

        // Use custom token bucket middleware for global rate limiting
        app.UseRateLimiting();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    // Map Health Check endpoint
    app.MapHealthChecks("/health");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program { }
