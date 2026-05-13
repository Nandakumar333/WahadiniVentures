# CI/CD Pipeline Documentation

## GitHub Actions Workflows

This project uses GitHub Actions for continuous integration and deployment.

### Workflows

#### 1. CI Pipeline (`ci.yml`)
**Triggers:** Push to `main`/`develop`, Pull Requests  
**Status:** ![CI Pipeline](https://github.com/YOUR_USERNAME/WahadiniCryptoQuest/actions/workflows/ci.yml/badge.svg)

**Jobs:**
- **Backend Build & Test**
  - Builds .NET 8 application
  - Runs all unit and integration tests
  - Generates code coverage reports
  - Uploads test results and coverage artifacts
  - Coverage threshold: Warning below 40%

- **Frontend Build & Test**
  - Builds React TypeScript application
  - Runs linting and type checking
  - Executes tests with coverage
  - Creates production build

- **Security Scan**
  - Trivy vulnerability scanning
  - Uploads results to GitHub Security
  - Fails on critical vulnerabilities

- **Code Quality Analysis**
  - Ready for SonarCloud integration
  - Code smell detection
  - Technical debt tracking

- **Docker Build**
  - Builds and pushes Docker images (main/develop only)
  - Uses layer caching for faster builds
  - Tags images with branch name and SHA

#### 2. Deployment Pipeline (`deploy.yml`)
**Triggers:** Push to `main`, Tags `v*.*.*`, Manual dispatch  
**Status:** ![Deploy](https://github.com/YOUR_USERNAME/WahadiniCryptoQuest/actions/workflows/deploy.yml/badge.svg)

**Environments:**
- **Staging:** Auto-deploys from `main` branch
  - URL: https://staging.wahadinicryptoquest.com
  - Runs smoke tests after deployment

- **Production:** Deploys from version tags
  - URL: https://wahadinicryptoquest.com
  - Requires manual approval
  - Automated rollback on failure

#### 3. Dependency Updates (`dependencies.yml`)
**Triggers:** Weekly (Mondays at 9:00 AM UTC), Manual dispatch  
**Status:** ![Dependencies](https://github.com/YOUR_USERNAME/WahadiniCryptoQuest/actions/workflows/dependencies.yml/badge.svg)

**Features:**
- Automatic NuGet package updates
- Automatic npm package updates
- Security vulnerability audits
- Auto-creates PRs for updates
- Runs tests before creating PRs

## Setup Instructions

### Required Secrets

Add these secrets in GitHub Settings → Secrets and variables → Actions:

#### Docker Hub (Optional)
```
DOCKER_USERNAME - Your Docker Hub username
DOCKER_PASSWORD - Your Docker Hub access token
```

#### SonarCloud (Optional)
```
SONAR_TOKEN - SonarCloud authentication token
```

#### Deployment (Optional)
```
STAGING_API_URL - Staging API endpoint
PRODUCTION_API_URL - Production API endpoint
```

### Branch Protection Rules

Recommended settings for `main` branch:
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
  - backend
  - frontend
  - security
- ✅ Require branches to be up to date before merging
- ✅ Include administrators

### Coverage Reports

Coverage reports are generated for each build and available as artifacts:
- **HTML Reports:** `backend-coverage-report` artifact
- **Coverage Badges:** `coverage-badges` artifact
- **Test Results:** `backend-test-results` artifact

View the latest coverage report:
1. Go to Actions tab
2. Select the latest successful workflow run
3. Download `backend-coverage-report` artifact
4. Open `index.html` in browser

## Local Testing

### Test CI Pipeline Locally

Using [act](https://github.com/nektos/act):

```bash
# Install act
choco install act-cli

# Run CI pipeline locally
act push

# Run specific job
act -j backend

# Use custom secrets
act --secret-file .secrets
```

### Build Docker Image Locally

```bash
# Backend
cd backend
docker build -t wahadinicryptoquest/api:local .
docker run -p 8080:8080 wahadinicryptoquest/api:local

# Test health endpoint
curl http://localhost:8080/health
```

## Monitoring

### Workflow Status

Check workflow status:
- Actions tab: https://github.com/YOUR_USERNAME/WahadiniCryptoQuest/actions
- Email notifications (configure in Settings)
- Slack integration (optional)

### Coverage Trends

Track coverage over time:
- View coverage badges in artifacts
- SonarCloud dashboard (if configured)
- GitHub Security tab for vulnerabilities

## Troubleshooting

### Common Issues

**Problem:** Tests failing in CI but passing locally  
**Solution:** Ensure `ASPNETCORE_ENVIRONMENT=Testing` is set

**Problem:** Docker build fails  
**Solution:** Check Dockerfile paths match project structure

**Problem:** Coverage report not generated  
**Solution:** Verify `dotnet-reportgenerator-globaltool` installation

**Problem:** Deployment fails  
**Solution:** Check secrets configuration and deployment scripts

### Debug Mode

Enable debug logging:
1. Go to Settings → Secrets and variables → Actions
2. Add repository variable: `ACTIONS_STEP_DEBUG = true`
3. Add repository variable: `ACTIONS_RUNNER_DEBUG = true`

## Best Practices

### Pull Requests
- ✅ Wait for all checks to pass before merging
- ✅ Review coverage changes in PR comments
- ✅ Fix security vulnerabilities before merging
- ✅ Keep PRs small and focused

### Releases
- ✅ Use semantic versioning (v1.2.3)
- ✅ Create release notes for each version
- ✅ Test in staging before production
- ✅ Monitor deployment health checks

### Dependencies
- ✅ Review auto-generated dependency PRs weekly
- ✅ Test thoroughly before merging
- ✅ Keep dependencies up to date
- ✅ Monitor security advisories

## Performance

### Build Times
- **Backend Build:** ~2-3 minutes
- **Frontend Build:** ~1-2 minutes
- **Security Scan:** ~30 seconds
- **Total CI Time:** ~5-7 minutes

### Optimization Tips
- ✅ Use caching for NuGet/npm packages
- ✅ Parallelize independent jobs
- ✅ Use Docker layer caching
- ✅ Skip redundant builds on docs-only changes

## Future Improvements

- [ ] Add E2E tests with Playwright
- [ ] Integrate performance testing (k6/NBomber)
- [ ] Add mobile app builds
- [ ] Implement blue-green deployments
- [ ] Add canary deployment strategy
- [ ] Integrate monitoring alerts
- [ ] Add automatic changelog generation
