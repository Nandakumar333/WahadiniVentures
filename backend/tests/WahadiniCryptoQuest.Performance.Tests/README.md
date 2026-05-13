# Performance Tests - WahadiniCryptoQuest

This project contains performance tests using **NBomber** - a modern load testing framework for .NET.

## Overview

Performance tests validate that the authentication endpoints can handle expected load levels and identify performance bottlenecks before production deployment.

## Test Framework

- **NBomber 5.10.3**: Modern load testing framework with powerful scenario modeling
- **NBomber.Http 5.10.3**: HTTP plugin for testing REST APIs
- **xUnit**: Test runner integration

## Test Scenarios

### 1. Register - Constant Load
**Test**: `Register_ConstantLoad_ShouldHandleExpectedThroughput`

- **Load Pattern**: 10 concurrent users for 30 seconds
- **Expected Performance**:
  - RPS > 5 requests/second
  - P95 latency < 1000ms
- **Validates**: User registration endpoint can handle steady traffic

### 2. Login - Ramp-Up Load
**Test**: `Login_RampUp_ShouldHandleIncreasingLoad`

- **Load Pattern**: 
  - 5 concurrent users for 10s
  - 10 concurrent users for 10s
  - 20 concurrent users for 10s
- **Expected Performance**:
  - RPS > 3 requests/second
  - P95 latency < 1500ms
- **Validates**: System scales gracefully under increasing load

### 3. Login - Spike Load
**Test**: `Login_SpikeLoad_ShouldRecoverGracefully`

- **Load Pattern**:
  - Normal: 5 concurrent users for 10s
  - Spike: 30 concurrent users for 10s
  - Recovery: 5 concurrent users for 10s
- **Expected Performance**:
  - Success rate > 95%
- **Validates**: System handles sudden traffic spikes and recovers

### 4. Token Refresh - Constant Load
**Test**: `TokenRefresh_ConstantLoad_ShouldHandleExpectedThroughput`

- **Load Pattern**: 15 concurrent users for 30 seconds
- **Expected Performance**:
  - RPS > 8 requests/second
  - P95 latency < 800ms
- **Validates**: Token refresh is fast and efficient

### 5. Mixed Workload - Realistic Scenario
**Test**: `MixedWorkload_RealisticScenario_ShouldHandleMultipleEndpoints`

- **Load Pattern** (concurrent):
  - Register: 3 users (20% of traffic)
  - Login: 8 users (50% of traffic)
  - Health Check: 5 users (30% of traffic)
- **Duration**: 30 seconds
- **Validates**: System handles realistic mixed traffic patterns

## Performance Metrics

NBomber collects comprehensive metrics:

- **Request Count**: Total successful and failed requests
- **RPS**: Requests per second (throughput)
- **Latency Statistics**:
  - Mean: Average response time
  - P50: Median response time
  - P75: 75th percentile
  - P95: 95th percentile (most requests faster than this)
  - P99: 99th percentile
  - Max: Slowest request

## Running Performance Tests

### Prerequisites

1. Ensure backend is built:
   ```bash
   dotnet build
   ```

2. Ensure database is available (uses InMemory for testing)

### Run All Performance Tests

```bash
cd backend/tests/WahadiniCryptoQuest.Performance.Tests
dotnet test
```

**Note**: Tests are marked with `Skip` attribute by default to prevent accidental long-running tests during regular CI/CD runs.

### Run Specific Test

```bash
# Remove [Fact(Skip = "...")] attribute from the test first
dotnet test --filter "FullyQualifiedName~Register_ConstantLoad"
```

### Run Without Skip (Override)

To run all performance tests, temporarily remove or comment out the `Skip` parameter:

```csharp
// Change this:
[Fact(Skip = "Performance test - run manually")]

// To this:
[Fact]
```

## Interpreting Results

### Good Performance Indicators

✅ **High RPS**: More requests per second = better throughput
✅ **Low P95 Latency**: 95% of requests complete quickly
✅ **High Success Rate**: > 95% requests succeed
✅ **Stable Recovery**: System returns to baseline after spikes

### Warning Signs

⚠️ **Low RPS**: < expected threshold may indicate bottlenecks
⚠️ **High P95 Latency**: > threshold may impact user experience
⚠️ **Low Success Rate**: < 95% indicates system overload
⚠️ **Slow Recovery**: System struggles after spike

## Performance Thresholds

Current baseline thresholds (adjust based on requirements):

| Endpoint | RPS Threshold | P95 Latency | Success Rate |
|----------|--------------|-------------|--------------|
| Register | > 5 req/s | < 1000ms | > 95% |
| Login | > 3 req/s | < 1500ms | > 95% |
| Token Refresh | > 8 req/s | < 800ms | > 95% |
| Mixed Load | Varies | Varies | > 95% |

## Optimization Tips

If tests fail to meet thresholds:

### Database Optimization
- Add indexes on frequently queried fields (email, username)
- Optimize queries (use projection, avoid N+1)
- Consider connection pooling

### API Optimization
- Enable response caching where appropriate
- Use asynchronous operations
- Optimize serialization/deserialization
- Implement pagination

### Infrastructure
- Scale horizontally (add instances)
- Use load balancer
- Implement CDN for static assets
- Consider caching layer (Redis)

## CI/CD Integration

Performance tests are excluded from regular CI/CD runs due to:
- Long execution time (30-60 seconds per test)
- Resource intensive
- Better suited for scheduled runs

### Recommended Approach

Run performance tests:
- ✅ Weekly scheduled job
- ✅ Before major releases
- ✅ After performance-related changes
- ✅ On dedicated performance testing environment

### GitHub Actions Example

```yaml
name: Performance Tests

on:
  schedule:
    - cron: '0 2 * * 0'  # Weekly on Sundays at 2 AM
  workflow_dispatch:

jobs:
  performance:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Run Performance Tests
        run: |
          cd backend/tests/WahadiniCryptoQuest.Performance.Tests
          # Remove Skip attributes
          sed -i 's/\[Fact(Skip = "Performance test - run manually")\]/[Fact]/g' *.cs
          dotnet test --logger "console;verbosity=detailed"
```

## Load Testing Strategy

### Development
- Small load (5-10 concurrent users)
- Short duration (10-30 seconds)
- Quick feedback on changes

### Staging
- Moderate load (20-50 concurrent users)
- Medium duration (1-2 minutes)
- Realistic traffic patterns

### Pre-Production
- Heavy load (100+ concurrent users)
- Extended duration (5-10 minutes)
- Stress testing and capacity planning

## Reports

NBomber generates detailed HTML reports (optional):

```csharp
var stats = NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFolder("performance-reports")
    .WithReportFormats(ReportFormat.Html, ReportFormat.Txt, ReportFormat.Csv)
    .Run();
```

Reports include:
- Timeline charts
- Latency distribution
- RPS graphs
- Failure analysis

## Troubleshooting

### Tests Timeout
- Reduce concurrent users
- Shorten test duration
- Check system resources

### High Failure Rate
- Check database connection
- Verify test data setup
- Review application logs

### Inconsistent Results
- Run multiple iterations
- Ensure clean state between runs
- Check for external factors (network, CPU)

## Best Practices

1. **Isolate Performance Tests**: Separate project prevents impact on unit tests
2. **Use Realistic Data**: Test with production-like data volumes
3. **Monitor Resources**: Track CPU, memory, database connections
4. **Baseline Performance**: Establish baseline before optimization
5. **Version Control**: Track performance metrics over time
6. **Clean State**: Reset database between test runs
7. **Warm-Up Period**: Allow system to stabilize before measurement

## Further Reading

- [NBomber Documentation](https://nbomber.com/docs/overview)
- [NBomber GitHub](https://github.com/PragmaticFlow/NBomber)
- [Performance Testing Best Practices](https://nbomber.com/docs/best-practices)

## Contributing

When adding new performance tests:

1. ✅ Follow naming convention: `{Endpoint}_{LoadPattern}_Should{Expected}`
2. ✅ Add `[Fact(Skip = "Performance test - run manually")]` attribute
3. ✅ Document expected thresholds in assertions
4. ✅ Log comprehensive metrics to test output
5. ✅ Update this README with new test scenarios
