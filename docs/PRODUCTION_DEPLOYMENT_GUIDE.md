# Production Deployment Guide - Phase 9

## Security Hardening

### 1. Security Headers (✅ Implemented)
All security headers are configured in `Program.cs`:
- **X-Content-Type-Options**: `nosniff` - Prevents MIME type sniffing
- **X-Frame-Options**: `DENY` - Prevents clickjacking
- **X-XSS-Protection**: `1; mode=block` - XSS protection for legacy browsers
- **Referrer-Policy**: `strict-origin-when-cross-origin` - Controls referrer information
- **Content-Security-Policy**: Restricts resource loading
- **X-Permitted-Cross-Domain-Policies**: `none` - Blocks cross-domain policies
- **HSTS**: HTTP Strict Transport Security with preload

### 2. Rate Limiting (✅ Implemented)
Rate limiting policies configured in `RateLimitingExtensions.cs`:
- Progress updates: 5 requests per 5 seconds
- Balance/history: 100 requests per minute
- Leaderboard: 60 requests per minute (backend has 15-min cache)
- Achievements: 50 requests per minute
- General rewards: 100 requests per minute

### 3. Input Validation (✅ Implemented)
- FluentValidation on all commands/queries
- Phase 7 currency validation: ISO 4217 codes, 0-9999 range, 50% deviation warnings
- Backend validation prevents SQL injection via parameterized queries

## Performance Optimization

### 1. Database Indexes (✅ Implemented)
Migration: `20251216_AddPerformanceIndexes.cs`

**Composite Indexes:**
- `Users`: Email, IsActive
- `Subscriptions`: UserId+Status, Status+EndDate, StripeSubscriptionId
- `CurrencyPricings`: CurrencyCode+IsActive, IsActive+IsDeleted
- `DiscountCodes`: Code+IsActive, ExpiryDate
- `UserRewardTransactions`: UserId+CreatedAt (includes TransactionType, Amount)
- `UserProgress`: UserId+LessonId, CompletedAt
- `Courses`: IsPublished+CreatedAt
- `Lessons`: CourseId+OrderIndex
- `UserTaskSubmissions`: UserId+SubmittedAt, TaskId+UserId, Status

### 2. Caching Strategy (✅ Implemented)
- **Response Caching**: Enabled in middleware
- **Response Compression**: Gzip + Brotli compression for HTTPS
- **Memory Cache**: Application-level caching
- **Backend**: 15-minute cache on leaderboard endpoints

### 3. Database Optimization
**Run migration:**
```bash
cd backend/src/WahadiniCryptoQuest.DAL
dotnet ef migrations add AddPerformanceIndexes --startup-project ../WahadiniCryptoQuest.API
dotnet ef database update --startup-project ../WahadiniCryptoQuest.API
```

## Reliability & Health Checks

### 1. Health Check Endpoints (✅ Implemented)
`/health` - Comprehensive health checks:
- **Database**: Connectivity + responsiveness check with 5-second timeout
- **Memory**: Monitors allocation (warning at 1GB, critical at 2GB)
- **Stripe**: API key validation

**Health Check Response:**
```json
{
  "status": "Healthy|Degraded|Unhealthy",
  "totalDuration": "00:00:00.123",
  "entries": {
    "database": {
      "status": "Healthy",
      "data": {
        "database_status": "connected",
        "user_count": 1234,
        "response_time_ms": 45
      }
    },
    "memory": {
      "status": "Healthy",
      "data": {
        "allocated_mb": 512,
        "gen0_collections": 10,
        "heap_size_bytes": 536870912
      }
    },
    "stripe": {
      "status": "Healthy",
      "data": {
        "stripe_mode": "test",
        "api_key_configured": true
      }
    }
  }
}
```

### 2. Polly Resilience Policies (✅ Implemented)
Located in `Policies/ResiliencePolicies.cs`:

**Retry Policy:**
- Exponential backoff: 2^retry seconds
- Handles: HttpRequestException, TimeoutException, TaskCanceledException
- Default: 3 retries

**Circuit Breaker:**
- Opens after 5 consecutive failures
- Stays open for 30 seconds
- Prevents cascading failures

**Timeout Policy:**
- Default: 10 seconds
- Pessimistic strategy (cancels operation)

**Database Retry Policy:**
- 3 retries with 100ms * retry delay
- Handles: DbUpdateException, DbException, TimeoutException

**Usage Example:**
```csharp
var policy = ResiliencePolicies.GetCombinedPolicy(retryCount: 3, timeoutSeconds: 10);
var result = await policy.ExecuteAsync(async () =>
{
    // Call external service (e.g., Stripe API)
    return await stripeService.CreateCustomer(request);
});
```

### 3. Graceful Degradation
- Health checks return degraded status (not unhealthy) for non-critical issues
- Circuit breaker prevents hammering failed services
- Retry logic with exponential backoff reduces load

## Observability & Monitoring

### 1. Structured Logging (✅ Implemented)
Serilog configuration with:
- **Correlation IDs**: Track requests across services via `X-Correlation-ID` header
- **Log Enrichment**: Environment, thread, correlation ID
- **Log Levels**: Configurable via `appsettings.json`

**Correlation ID Middleware:**
- Automatically generates/propagates correlation IDs
- Added to all log entries via `CorrelationIdEnricher`
- Included in response headers for client tracking

### 2. Monitoring Setup (To Configure)

**Application Insights (Azure):**
```bash
# Install NuGet package
dotnet add package Microsoft.ApplicationInsights.AspNetCore

# Add to Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

**Environment Variables:**
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...
```

**Metrics to Monitor:**
- Request duration (P50, P95, P99)
- Error rate (5xx errors)
- Health check status
- Memory usage trend
- Database query performance
- Rate limit rejections (429 errors)

### 3. Alerting Configuration (Recommended)

**Critical Alerts:**
- Health check fails for >5 minutes
- Error rate >1% for >10 minutes
- Memory usage >90% for >5 minutes
- Database connection failures
- Stripe API failures

**Warning Alerts:**
- Memory usage >70%
- Request duration P95 >1s
- Rate limit rejections >100/min
- Circuit breaker opens

## Deployment Checklist

### Pre-Deployment
- [ ] Run database migration: `AddPerformanceIndexes`
- [ ] Configure environment variables (connection strings, Stripe keys)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Enable HTTPS certificate
- [ ] Configure CORS origins
- [ ] Set up Application Insights (optional)

### Database Migration
```bash
# Production migration command
dotnet ef database update --project WahadiniCryptoQuest.DAL --startup-project WahadiniCryptoQuest.API --configuration Release
```

### Environment Variables (Production)
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=...
Jwt__Secret=<generate-secure-secret>
Jwt__Issuer=https://api.wahadinicryptoquest.com
Jwt__Audience=https://wahadinicryptoquest.com
Stripe__SecretKey=sk_live_...
Stripe__WebhookSecret=whsec_...
Stripe__PublishableKey=pk_live_...
AllowedOrigins__0=https://wahadinicryptoquest.com
```

### Health Check Monitoring
```bash
# Test health endpoint
curl https://api.wahadinicryptoquest.com/health

# Uptime monitoring services
# - Pingdom
# - UptimeRobot
# - Azure Monitor (if on Azure)
```

### Performance Validation
```bash
# Load testing with Apache Bench
ab -n 1000 -c 10 https://api.wahadinicryptoquest.com/api/subscriptions/pricing

# Expected improvements from indexes:
# - User queries: 50-70% faster
# - Subscription status checks: 60-80% faster
# - Leaderboard queries: 40-60% faster
# - Reward transaction history: 70-85% faster
```

## Security Recommendations

### 1. Secrets Management
- **Azure Key Vault** (Azure)
- **AWS Secrets Manager** (AWS)
- **HashiCorp Vault** (On-premise)

### 2. Network Security
- Configure firewall rules (allow only 80/443)
- Use VPN/private network for database
- Enable DDoS protection

### 3. Database Security
- Use connection pooling
- Enable SSL for database connections
- Restrict database user permissions (principle of least privilege)
- Regular automated backups

### 4. API Security
- Implement API versioning
- Use API gateway for additional protection (optional)
- Monitor for suspicious patterns
- Regular security audits

## Monitoring Dashboard

### Key Metrics
1. **Availability**: Health check uptime %
2. **Performance**: Request duration P95
3. **Reliability**: Error rate %
4. **Capacity**: Memory/CPU usage
5. **Business**: Active subscriptions, revenue

### Log Queries (Application Insights)
```kusto
// Correlation ID tracking
traces
| where customDimensions.CorrelationId == "abc123"
| order by timestamp desc

// Error rate by endpoint
requests
| where resultCode >= 500
| summarize ErrorCount=count() by name
| order by ErrorCount desc

// Slow queries (>1s)
dependencies
| where duration > 1000
| order by duration desc
```

## Rollback Plan

If issues occur after deployment:
1. Revert to previous Docker image/deployment
2. Rollback database migration if needed:
   ```bash
   dotnet ef migrations remove --project WahadiniCryptoQuest.DAL
   ```
3. Monitor health checks and error logs
4. Notify users via status page

## Phase 9 Summary

✅ **Security**: Headers, HSTS, rate limiting, input validation
✅ **Performance**: Database indexes, caching, compression
✅ **Reliability**: Health checks, Polly policies, circuit breaker, graceful degradation
✅ **Observability**: Correlation IDs, structured logging, health endpoint

**Production Ready**: Application is hardened and ready for production deployment with comprehensive monitoring and resilience features.
