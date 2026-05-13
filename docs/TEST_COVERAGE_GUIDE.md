# Test Coverage Configuration

## Overview
This document describes the test coverage configuration and reporting for WahadiniCryptoQuest.

## Coverage Thresholds
- **Minimum Required**: 80% for all metrics (lines, branches, functions, statements)
- **Enforcement**: Automated in CI/CD pipeline
- **Target**: 85%+ for production code

## Backend Coverage (.NET)

### Tool: Coverlet
- **Version**: 6.0.x
- **Formats**: Cobertura, JSON, LCOV, OpenCover
- **Configuration**: `backend/tests/Directory.Build.props`

### Running Backend Coverage

```bash
# From backend directory
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# With threshold enforcement
dotnet test /p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=line,branch,method
```

### Coverage Reports Location
- **Path**: `backend/TestResults/`
- **Formats**:
  - `coverage.cobertura.xml` - For CI/CD integration
  - `coverage.json` - Machine-readable format
  - `coverage.opencover.xml` - For detailed analysis
  - HTML reports generated via ReportGenerator

### Exclusions
- `*.Tests` projects
- Migration files
- `Program.cs`, `Startup.cs`

## Frontend Coverage (React/TypeScript)

### Tool: Vitest with V8 Provider
- **Version**: 3.2.x
- **Formats**: Text, JSON, HTML, JSON Summary
- **Configuration**: `frontend/vitest.config.ts`

### Running Frontend Coverage

```bash
# From frontend directory
npm run test:coverage

# View HTML report
open coverage/index.html  # macOS
start coverage/index.html # Windows
```

### Coverage Reports Location
- **Path**: `frontend/coverage/`
- **Formats**:
  - `coverage-summary.json` - Quick stats
  - `coverage-final.json` - Detailed coverage data
  - `index.html` - Interactive HTML report

### Exclusions
- `node_modules/`
- `src/test/` setup files
- `*.d.ts` TypeScript definitions
- Test files (`*.test.ts`, `*.spec.ts`)

## CI/CD Integration

### GitHub Actions Workflow
Coverage is automatically collected and enforced in the CI/CD pipeline:

1. **Backend Coverage Job**
   - Runs all backend tests with Coverlet
   - Generates Cobertura report
   - Uploads to Codecov
   - Enforces 80% threshold

2. **Frontend Coverage Job**
   - Runs all frontend tests with Vitest
   - Generates JSON summary
   - Uploads to Codecov
   - Enforces 80% threshold via JSON parsing

### Codecov Integration
- **Dashboard**: https://codecov.io/gh/YOUR_ORG/WahadiniCryptoQuest
- **Badge**: Available in README.md
- **PR Comments**: Automatic coverage diff on pull requests

## Coverage Reports

### Viewing Reports Locally

**Backend (HTML with ReportGenerator)**:
```bash
# Install ReportGenerator globally
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"backend/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"backend/TestResults/html" \
  -reporttypes:Html

# Open report
start backend/TestResults/html/index.html
```

**Frontend (Built-in HTML)**:
```bash
npm run test:coverage
start frontend/coverage/index.html
```

### Interpreting Coverage Metrics

- **Lines**: Percentage of code lines executed
- **Branches**: Percentage of conditional paths tested
- **Functions**: Percentage of functions/methods called
- **Statements**: Percentage of statements executed

## Coverage Goals by Layer

### Backend
- **API Controllers**: 90%+ (critical user-facing endpoints)
- **Service Layer**: 85%+ (business logic)
- **Data Access**: 80%+ (repository patterns)
- **Core/Domain**: 95%+ (domain logic and validation)

### Frontend
- **Components**: 80%+ (UI components)
- **Hooks**: 90%+ (custom React hooks)
- **Services**: 85%+ (API clients, utilities)
- **Store**: 90%+ (state management)

## Improving Coverage

### Identify Gaps
```bash
# Backend - view uncovered lines
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
# Review lcov.info file

# Frontend - view coverage report
npm run test:coverage
# Open HTML report for visual analysis
```

### Priority Areas
1. **Critical Paths**: Authentication, authorization, payment processing
2. **Error Handling**: Exception scenarios, validation failures
3. **Edge Cases**: Boundary conditions, null handling
4. **Integration Points**: API calls, database operations

## Troubleshooting

### Backend Coverage Issues

**Issue**: Coverage not collected
```bash
# Verify Coverlet is installed
dotnet list package | findstr coverlet

# Run with verbose logging
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"
```

**Issue**: Threshold failures
- Review `TestResults/coverage.cobertura.xml`
- Identify uncovered lines
- Add targeted unit tests

### Frontend Coverage Issues

**Issue**: Coverage below threshold
```bash
# Run coverage with detailed output
npm run test:coverage -- --reporter=verbose

# View JSON summary
type coverage\coverage-summary.json
```

**Issue**: Files not being covered
- Check `vitest.config.ts` exclusions
- Ensure test files import the modules
- Verify coverage provider is 'v8'

## Best Practices

1. **Write Tests First**: TDD approach ensures coverage
2. **Test Public APIs**: Focus on public methods and exports
3. **Cover Edge Cases**: Boundary conditions, error paths
4. **Avoid Coverage Gaming**: Don't write tests just for metrics
5. **Review Coverage Reports**: Regularly check for gaps
6. **Update Thresholds**: Gradually increase as coverage improves

## Resources

- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [Vitest Coverage Guide](https://vitest.dev/guide/coverage.html)
- [Codecov Documentation](https://docs.codecov.com/)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
