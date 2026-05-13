using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WahadiniCryptoQuest.Core.Settings;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Seeders;

namespace WahadiniCryptoQuest.API.Tests;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
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
                // Disable rate limiting for tests - set very high limits
                new KeyValuePair<string, string?>("Performance:RateLimitPerMinute", "10000"),
                new KeyValuePair<string, string?>("Performance:RateLimitBurst", "5000")
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

            // Configure JWT settings for testing - must match between generation and validation
            var testJwtSettings = new JwtSettings
            {
                SecretKey = "ThisIsAVerySecureTestingSecretKeyThatIsAtLeast256BitsLong!123456789012345678901234567890",
                Issuer = "TestIssuer",  // Must match the in-memory configuration above
                Audience = "TestAudience",  // Must match the in-memory configuration above
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
            
            services.Configure<JwtSettings>(options =>
            {
                options.SecretKey = testJwtSettings.SecretKey;
                options.Issuer = testJwtSettings.Issuer;
                options.Audience = testJwtSettings.Audience;
                options.AccessTokenExpirationMinutes = testJwtSettings.AccessTokenExpirationMinutes;
                options.RefreshTokenExpirationDays = testJwtSettings.RefreshTokenExpirationDays;
                options.ValidateIssuer = testJwtSettings.ValidateIssuer;
                options.ValidateAudience = testJwtSettings.ValidateAudience;
                options.ValidateLifetime = testJwtSettings.ValidateLifetime;
                options.ValidateIssuerSigningKey = testJwtSettings.ValidateIssuerSigningKey;
                options.ClockSkew = testJwtSettings.ClockSkew;
            });

            // Override JWT Bearer options for testing
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(testJwtSettings.SecretKey));
                
                options.RequireHttpsMetadata = false; // Allow HTTP in tests
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = testJwtSettings.ValidateIssuer,
                    ValidateAudience = testJwtSettings.ValidateAudience,
                    ValidateLifetime = testJwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = testJwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = testJwtSettings.Issuer,
                    ValidAudience = testJwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = testJwtSettings.ClockSkew,
                    RoleClaimType = "role",
                    NameClaimType = "sub"
                };
            });
            
            // Seed RBAC data after services are configured
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seeder = new RBACDataSeeder(dbContext);
                seeder.SeedAsync().GetAwaiter().GetResult();
            }
        });

        builder.UseEnvironment("Testing");
    }
    
    protected override void ConfigureClient(System.Net.Http.HttpClient client)
    {
        base.ConfigureClient(client);
        // Disable client-side caching
        client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true
        };
    }
}
