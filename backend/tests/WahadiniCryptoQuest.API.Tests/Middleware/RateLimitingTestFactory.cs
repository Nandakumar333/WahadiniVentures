using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.API.Tests.Middleware;

/// <summary>
/// Custom WebApplicationFactory for rate limiting tests
/// Uses "Development" environment instead of "Testing" to enable rate limiting middleware
/// </summary>
public class RateLimitingTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"TestDatabase_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "Host=localhost;Port=5432;Database=test_db;Username=test;Password=test"),
                new KeyValuePair<string, string?>("JwtSettings:SecretKey", "ThisIsAVerySecureTestingSecretKeyThatIsAtLeast256BitsLong!123456789012345678901234567890"),
                new KeyValuePair<string, string?>("JwtSettings:Issuer", "TestIssuer"),
                new KeyValuePair<string, string?>("JwtSettings:Audience", "TestAudience"),
                new KeyValuePair<string, string?>("JwtSettings:AccessTokenExpirationMinutes", "60"),
                new KeyValuePair<string, string?>("JwtSettings:RefreshTokenExpirationDays", "7"),
                new KeyValuePair<string, string?>("EmailSettings:SmtpServer", "localhost"),
                new KeyValuePair<string, string?>("EmailSettings:SmtpPort", "587"),
                new KeyValuePair<string, string?>("EmailSettings:EnableSsl", "false"),
                new KeyValuePair<string, string?>("EmailSettings:Username", "test@test.com"),
                new KeyValuePair<string, string?>("EmailSettings:Password", "testpassword"),
                new KeyValuePair<string, string?>("EmailSettings:FromEmail", "test@test.com"),
                new KeyValuePair<string, string?>("EmailSettings:FromName", "Test Application"),
                // Set very low rate limits for testing (allows reliable testing of 429 responses)
                new KeyValuePair<string, string?>("Performance:RateLimitPerMinute", "3"),
                new KeyValuePair<string, string?>("Performance:RateLimitBurst", "2")
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });
        });

        // Use Development environment instead of Testing to enable rate limiting
        builder.UseEnvironment("Development");
    }
}
