# WahadiniCryptoQuest - Build and Compilation Prompt

## Context
You are an expert build engineer working on the WahadiniCryptoQuest compilation and build processes. The application consists of a .NET 9.0 backend with Clean Architecture and a React TypeScript frontend, requiring sophisticated build configurations for development, testing, and production environments.

## Build Architecture Overview

### Technology Stack
- **Backend**: .NET 9.0 Web API with MSBuild
- **Frontend**: React 18 with TypeScript and Create React App
- **Build Tools**: MSBuild, npm/yarn, Docker multi-stage builds
- **Package Management**: NuGet (.NET), npm (React)
- **Bundling**: Webpack (via Create React App), .NET publish profiles

### Build Environments
- **Development**: Hot reload, debugging symbols, source maps
- **Testing**: Test-optimized builds with coverage
- **Production**: Optimized, minified, compressed builds

## Backend Build Configuration

### 1. .NET Project Structure
```xml
<!-- WahadiniCryptoQuest.API.csproj - Main API project -->
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
    <DockerfileContext>..\..</DockerfileContext>
  </PropertyGroup>

  <!-- Development configuration -->
  <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <DebugType>full</DebugType>
    <DebugSymbols>true</DebugSymbols>
    <Optimize>false</Optimize>
  </PropertyGroup>

  <!-- Production configuration -->
  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <DefineConstants>TRACE</DefineConstants>
    <DebugType>pdbonly</DebugType>
    <DebugSymbols>false</DebugSymbols>
    <Optimize>true</Optimize>
    <PublishTrimmed>true</PublishTrimmed>
    <PublishSingleFile>false</PublishSingleFile>
    <PublishReadyToRun>true</PublishReadyToRun>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="7.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\WahadiniCryptoQuest.Core\WahadiniCryptoQuest.Core.csproj" />
    <ProjectReference Include="..\WahadiniCryptoQuest.Service\WahadiniCryptoQuest.Service.csproj" />
    <ProjectReference Include="..\WahadiniCryptoQuest.DAL\WahadiniCryptoQuest.DAL.csproj" />
  </ItemGroup>

  <!-- Copy configuration files -->
  <ItemGroup>
    <Content Include="appsettings*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

### 2. Directory.Build.props - Global Build Settings
```xml
<Project>

  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateAssemblyInfo>true</GenerateAssemblyInfo>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
    <InformationalVersion>1.0.0</InformationalVersion>
  </PropertyGroup>

  <!-- Global package versions -->
  <PropertyGroup>
    <MicrosoftNetCoreVersion>9.0.0</MicrosoftNetCoreVersion>
    <EntityFrameworkVersion>8.0.0</EntityFrameworkVersion>
    <AutoMapperVersion>12.0.1</AutoMapperVersion>
    <FluentValidationVersion>11.8.0</FluentValidationVersion>
    <SerilogVersion>7.0.0</SerilogVersion>
  </PropertyGroup>

  <!-- Common package references for all projects -->
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" Version="$(MicrosoftNetCoreVersion)" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="$(MicrosoftNetCoreVersion)" />
  </ItemGroup>

  <!-- Analyzer packages -->
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" PrivateAssets="all" />
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.12.0.78982" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

### 3. Build Scripts for Backend
```bash
#!/bin/bash
# build-backend.sh - Backend build script

set -e

echo "Building Personal Finance Backend..."

# Configuration
CONFIGURATION=${1:-Release}
OUTPUT_DIR=${2:-./publish}
SOLUTION_FILE="WahadiniCryptoQuest.sln"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[BUILD]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to clean previous builds
clean_build() {
    print_status "Cleaning previous builds..."
    dotnet clean $SOLUTION_FILE --configuration $CONFIGURATION
    rm -rf $OUTPUT_DIR
    print_success "Clean completed"
}

# Function to restore packages
restore_packages() {
    print_status "Restoring NuGet packages..."
    dotnet restore $SOLUTION_FILE --verbosity minimal
    print_success "Package restoration completed"
}

# Function to build solution
build_solution() {
    print_status "Building solution in $CONFIGURATION mode..."
    dotnet build $SOLUTION_FILE \
        --configuration $CONFIGURATION \
        --no-restore \
        --verbosity minimal \
        --property:TreatWarningsAsErrors=true
    print_success "Build completed successfully"
}

# Function to run tests
run_tests() {
    print_status "Running unit tests..."
    dotnet test $SOLUTION_FILE \
        --configuration $CONFIGURATION \
        --no-build \
        --verbosity minimal \
        --logger "trx;LogFileName=test-results.trx" \
        --logger "html;LogFileName=test-results.html" \
        --collect:"XPlat Code Coverage"
    print_success "Tests completed"
}

# Function to publish application
publish_application() {
    print_status "Publishing application..."
    dotnet publish src/WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj \
        --configuration $CONFIGURATION \
        --output $OUTPUT_DIR \
        --no-build \
        --verbosity minimal \
        --self-contained false \
        --property:PublishReadyToRun=true \
        --property:PublishTrimmed=false
    print_success "Publishing completed"
}

# Function to generate documentation
generate_docs() {
    if command -v docfx &> /dev/null; then
        print_status "Generating API documentation..."
        docfx docfx.json
        print_success "Documentation generated"
    else
        print_status "DocFX not found, skipping documentation generation"
    fi
}

# Main build process
main() {
    echo "Personal Finance Backend Build Script"
    echo "Configuration: $CONFIGURATION"
    echo "Output Directory: $OUTPUT_DIR"
    echo "======================================="
    
    clean_build
    restore_packages
    build_solution
    
    if [ "$CONFIGURATION" == "Release" ]; then
        run_tests
        publish_application
        generate_docs
    fi
    
    print_success "Build process completed successfully!"
}

# Execute main function
main "$@"
```

```powershell
# build-backend.ps1 - PowerShell build script for Windows
param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "./publish"
)

$ErrorActionPreference = "Stop"

Write-Host "Building Personal Finance Backend..." -ForegroundColor Green

$SolutionFile = "WahadiniCryptoQuest.sln"

function Write-Status($Message) {
    Write-Host "[BUILD] $Message" -ForegroundColor Yellow
}

function Write-Success($Message) {
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Error($Message) {
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

try {
    # Clean previous builds
    Write-Status "Cleaning previous builds..."
    dotnet clean $SolutionFile --configuration $Configuration
    if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
    Write-Success "Clean completed"

    # Restore packages
    Write-Status "Restoring NuGet packages..."
    dotnet restore $SolutionFile --verbosity minimal
    Write-Success "Package restoration completed"

    # Build solution
    Write-Status "Building solution in $Configuration mode..."
    dotnet build $SolutionFile `
        --configuration $Configuration `
        --no-restore `
        --verbosity minimal `
        --property:TreatWarningsAsErrors=true
    Write-Success "Build completed successfully"

    if ($Configuration -eq "Release") {
        # Run tests
        Write-Status "Running unit tests..."
        dotnet test $SolutionFile `
            --configuration $Configuration `
            --no-build `
            --verbosity minimal `
            --logger "trx;LogFileName=test-results.trx" `
            --collect:"XPlat Code Coverage"
        Write-Success "Tests completed"

        # Publish application
        Write-Status "Publishing application..."
        dotnet publish src/WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj `
            --configuration $Configuration `
            --output $OutputDir `
            --no-build `
            --verbosity minimal `
            --self-contained $false `
            --property:PublishReadyToRun=true
        Write-Success "Publishing completed"
    }

    Write-Success "Build process completed successfully!"
}
catch {
    Write-Error "Build failed: $_"
    exit 1
}
```

## Frontend Build Configuration

### 1. Package.json Build Scripts
```json
{
  "name": "personal-finance-frontend",
  "version": "1.0.0",
  "private": true,
  "scripts": {
    "start": "react-scripts start",
    "build": "react-scripts build",
    "build:dev": "REACT_APP_ENV=development npm run build",
    "build:staging": "REACT_APP_ENV=staging npm run build",
    "build:prod": "REACT_APP_ENV=production npm run build",
    "test": "react-scripts test",
    "test:coverage": "react-scripts test --coverage --watchAll=false",
    "test:ci": "CI=true react-scripts test --coverage --watchAll=false --verbose",
    "eject": "react-scripts eject",
    "lint": "eslint src --ext .ts,.tsx --max-warnings 0",
    "lint:fix": "eslint src --ext .ts,.tsx --fix",
    "format": "prettier --write \"src/**/*.{ts,tsx,js,jsx,json,css,md}\"",
    "format:check": "prettier --check \"src/**/*.{ts,tsx,js,jsx,json,css,md}\"",
    "type-check": "tsc --noEmit",
    "analyze": "npm run build && npx bundle-analyzer build/static/js/*.js",
    "prebuild": "npm run type-check && npm run lint",
    "postbuild": "npm run analyze:build",
    "analyze:build": "echo 'Build completed. Analyzing bundle...'",
    "clean": "rm -rf build node_modules/.cache",
    "deps:check": "npm audit && npm outdated",
    "deps:update": "npm update && npm audit fix"
  },
  "dependencies": {
    "@hookform/resolvers": "^5.2.2",
    "@radix-ui/react-checkbox": "^1.3.3",
    "@radix-ui/react-dialog": "^1.1.15",
    "@tanstack/react-query": "^5.90.2",
    "axios": "^1.12.2",
    "framer-motion": "^12.23.22",
    "react": "^19.1.1",
    "react-dom": "^19.1.1",
    "react-hook-form": "^7.63.0",
    "react-router-dom": "^7.9.3",
    "tailwindcss": "^3.4.17",
    "typescript": "^4.9.5",
    "zod": "^3.25.76",
    "zustand": "^5.0.8"
  },
  "devDependencies": {
    "@testing-library/jest-dom": "^6.8.0",
    "@testing-library/react": "^16.3.0",
    "@testing-library/user-event": "^13.5.0",
    "@types/jest": "^27.5.2",
    "@types/node": "^16.18.126",
    "@types/react": "^19.1.15",
    "@types/react-dom": "^19.1.9",
    "autoprefixer": "^10.4.21",
    "eslint": "^8.57.0",
    "eslint-config-react-app": "^7.0.1",
    "postcss": "^8.5.6",
    "prettier": "^3.6.2",
    "react-scripts": "5.0.1"
  },
  "browserslist": {
    "production": [
      ">0.2%",
      "not dead",
      "not op_mini all"
    ],
    "development": [
      "last 1 chrome version",
      "last 1 firefox version",
      "last 1 safari version"
    ]
  },
  "jest": {
    "collectCoverageFrom": [
      "src/**/*.{ts,tsx}",
      "!src/**/*.d.ts",
      "!src/index.tsx",
      "!src/reportWebVitals.ts",
      "!src/**/*.stories.{ts,tsx}",
      "!src/**/*.test.{ts,tsx}",
      "!src/**/*.spec.{ts,tsx}"
    ],
    "coverageThreshold": {
      "global": {
        "branches": 80,
        "functions": 80,
        "lines": 80,
        "statements": 80
      }
    },
    "coverageReporters": ["text", "lcov", "html"]
  }
}
```

### 2. TypeScript Configuration
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": [
      "dom",
      "dom.iterable",
      "esnext"
    ],
    "allowJs": true,
    "skipLibCheck": true,
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true,
    "strict": true,
    "forceConsistentCasingInFileNames": true,
    "noFallthroughCasesInSwitch": true,
    "module": "esnext",
    "moduleResolution": "node",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "declaration": false,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "exactOptionalPropertyTypes": true,
    "noImplicitReturns": true,
    "noImplicitOverride": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"],
      "@/components/*": ["src/components/*"],
      "@/features/*": ["src/features/*"],
      "@/hooks/*": ["src/hooks/*"],
      "@/lib/*": ["src/lib/*"],
      "@/services/*": ["src/services/*"],
      "@/types/*": ["src/types/*"],
      "@/utils/*": ["src/utils/*"],
      "@/store/*": ["src/store/*"]
    }
  },
  "include": [
    "src",
    "src/**/*"
  ],
  "exclude": [
    "node_modules",
    "build",
    "dist",
    "**/*.test.*",
    "**/*.spec.*"
  ]
}
```

### 3. Frontend Build Scripts
```bash
#!/bin/bash
# build-frontend.sh - Frontend build script

set -e

echo "Building Personal Finance Frontend..."

# Configuration
ENVIRONMENT=${1:-production}
OUTPUT_DIR=${2:-build}
NODE_ENV=${ENVIRONMENT}

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[BUILD]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to check Node.js version
check_node_version() {
    print_status "Checking Node.js version..."
    NODE_VERSION=$(node --version)
    REQUIRED_VERSION="v18"
    
    if [[ "$NODE_VERSION" < "$REQUIRED_VERSION" ]]; then
        print_error "Node.js version $REQUIRED_VERSION or higher is required. Current: $NODE_VERSION"
        exit 1
    fi
    print_success "Node.js version check passed: $NODE_VERSION"
}

# Function to install dependencies
install_dependencies() {
    print_status "Installing dependencies..."
    
    # Check if package-lock.json exists for reproducible builds
    if [ -f "package-lock.json" ]; then
        npm ci --silent
    else
        npm install --silent
    fi
    
    print_success "Dependencies installed successfully"
}

# Function to run linting
run_linting() {
    print_status "Running ESLint..."
    npm run lint
    print_success "Linting passed"
}

# Function to run type checking
run_type_check() {
    print_status "Running TypeScript type checking..."
    npm run type-check
    print_success "Type checking passed"
}

# Function to run tests
run_tests() {
    print_status "Running tests with coverage..."
    npm run test:ci
    print_success "Tests passed"
}

# Function to build application
build_application() {
    print_status "Building application for $ENVIRONMENT..."
    
    # Set environment variables
    export NODE_ENV=$NODE_ENV
    export REACT_APP_ENV=$ENVIRONMENT
    export GENERATE_SOURCEMAP=false
    export INLINE_RUNTIME_CHUNK=false
    
    # Build based on environment
    case $ENVIRONMENT in
        development)
            export REACT_APP_API_URL="http://localhost:5000/api"
            ;;
        staging)
            export REACT_APP_API_URL="https://staging-api.WahadiniCryptoQuest.com/api"
            ;;
        production)
            export REACT_APP_API_URL="https://api.WahadiniCryptoQuest.com/api"
            ;;
    esac
    
    npm run build
    print_success "Build completed successfully"
}

# Function to analyze bundle
analyze_bundle() {
    if [ "$ENVIRONMENT" == "production" ]; then
        print_status "Analyzing bundle size..."
        
        # Check if bundle analyzer is available
        if command -v webpack-bundle-analyzer &> /dev/null; then
            npx webpack-bundle-analyzer build/static/js/*.js --report --mode static --report-filename bundle-report.html
            print_success "Bundle analysis completed"
        else
            print_status "Bundle analyzer not available, skipping analysis"
        fi
    fi
}

# Function to optimize build
optimize_build() {
    if [ "$ENVIRONMENT" == "production" ]; then
        print_status "Optimizing build artifacts..."
        
        # Compress assets if gzip is available
        if command -v gzip &> /dev/null; then
            find build -name "*.js" -o -name "*.css" -o -name "*.html" | while read file; do
                gzip -k -9 "$file"
            done
            print_success "Assets compressed"
        fi
        
        # Generate build manifest
        echo "{
  \"buildTime\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",
  \"environment\": \"$ENVIRONMENT\",
  \"version\": \"$(npm version --json | jq -r '.\"personal-finance-frontend\"')\"
}" > build/build-manifest.json
        
        print_success "Build optimization completed"
    fi
}

# Main build process
main() {
    echo "Personal Finance Frontend Build Script"
    echo "Environment: $ENVIRONMENT"
    echo "Output Directory: $OUTPUT_DIR"
    echo "======================================="
    
    check_node_version
    install_dependencies
    run_type_check
    run_linting
    
    if [ "$ENVIRONMENT" == "production" ] || [ "$ENVIRONMENT" == "staging" ]; then
        run_tests
    fi
    
    build_application
    optimize_build
    analyze_bundle
    
    print_success "Frontend build process completed successfully!"
}

# Execute main function
main "$@"
```

## Environment-Specific Build Configurations

### 1. Development Build Configuration
```bash
#!/bin/bash
# build-dev.sh - Development environment build

export NODE_ENV=development
export REACT_APP_ENV=development
export REACT_APP_API_URL=http://localhost:5000/api
export GENERATE_SOURCEMAP=true
export FAST_REFRESH=true

# Development-specific settings
export ESLINT_NO_DEV_ERRORS=false
export TSC_COMPILE_ON_ERROR=false
export DISABLE_ESLINT_PLUGIN=false

echo "Building for Development Environment..."

# Install dependencies
npm ci

# Type check in watch mode (background)
npm run type-check -- --watch &
TYPE_CHECK_PID=$!

# Start development server
npm start

# Cleanup on exit
trap "kill $TYPE_CHECK_PID 2>/dev/null" EXIT
```

### 2. Production Build Configuration
```bash
#!/bin/bash
# build-prod.sh - Production environment build

export NODE_ENV=production
export REACT_APP_ENV=production
export GENERATE_SOURCEMAP=false
export INLINE_RUNTIME_CHUNK=false

echo "Building for Production Environment..."

# Validate environment variables
if [ -z "$REACT_APP_API_URL" ]; then
    echo "Error: REACT_APP_API_URL must be set for production builds"
    exit 1
fi

# Clean previous builds
rm -rf build
rm -rf node_modules/.cache

# Install dependencies (production only)
npm ci --only=production --silent

# Reinstall dev dependencies for build
npm ci --silent

# Run full validation
npm run lint
npm run type-check
npm run test:ci

# Build application
npm run build

# Validate build output
if [ ! -d "build" ]; then
    echo "Error: Build directory not created"
    exit 1
fi

if [ ! -f "build/index.html" ]; then
    echo "Error: index.html not found in build output"
    exit 1
fi

echo "Production build completed successfully"
```

## Integrated Build Pipeline

### 1. Full Application Build Script
```bash
#!/bin/bash
# build-all.sh - Build entire application

set -e

ENVIRONMENT=${1:-production}
SKIP_TESTS=${2:-false}

echo "Building WahadiniCryptoQuestlication"
echo "Environment: $ENVIRONMENT"
echo "Skip Tests: $SKIP_TESTS"
echo "======================================"

# Build backend
echo "Building Backend..."
cd backend
./build-backend.sh $ENVIRONMENT
cd ..

# Build frontend
echo "Building Frontend..."
cd frontend
./build-frontend.sh $ENVIRONMENT
cd ..

# Run integration tests if not skipped
if [ "$SKIP_TESTS" != "true" ] && [ "$ENVIRONMENT" == "production" ]; then
    echo "Running Integration Tests..."
    # Add integration test commands here
fi

echo "Application build completed successfully!"
```

### 2. Docker Multi-stage Build Optimization
```dockerfile
# Multi-stage Dockerfile for frontend optimization
FROM node:18-alpine as dependencies

WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production && npm cache clean --force

FROM node:18-alpine as build-deps

WORKDIR /app
COPY package*.json ./
RUN npm ci && npm cache clean --force

FROM build-deps as build

WORKDIR /app
COPY . .
RUN npm run build

# Optimize and validate build
RUN npm run test:ci
RUN npm run lint
RUN npm run type-check

FROM nginx:alpine as production

# Copy built application
COPY --from=build /app/build /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Add build information
COPY --from=build /app/build/build-manifest.json /usr/share/nginx/html/

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
```

## Build Optimization Strategies

### 1. MSBuild Optimization
```xml
<!-- MSBuild optimization properties -->
<PropertyGroup>
  <!-- Parallel build -->
  <MultiProcessorCompilation>true</MultiProcessorCompilation>
  <UseSharedCompilation>true</UseSharedCompilation>
  
  <!-- Incremental build -->
  <UseIncrementalCompilation>true</UseIncrementalCompilation>
  
  <!-- Output optimization -->
  <ProduceReferenceAssembly>false</ProduceReferenceAssembly>
  <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
  
  <!-- Package optimization -->
  <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  <RestoreLockedMode Condition="'$(CI)' == 'true'">true</RestoreLockedMode>
</PropertyGroup>
```

### 2. Webpack Bundle Optimization
```javascript
// craco.config.js - Override Create React App webpack config
const path = require('path');

module.exports = {
  webpack: {
    configure: (webpackConfig, { env }) => {
      if (env === 'production') {
        // Optimize bundle splitting
        webpackConfig.optimization.splitChunks = {
          chunks: 'all',
          cacheGroups: {
            vendor: {
              test: /[\\/]node_modules[\\/]/,
              name: 'vendors',
              chunks: 'all',
            },
            common: {
              name: 'common',
              minChunks: 2,
              chunks: 'all',
              enforce: true,
            },
          },
        };

        // Add bundle analyzer in CI
        if (process.env.ANALYZE_BUNDLE) {
          const { BundleAnalyzerPlugin } = require('webpack-bundle-analyzer');
          webpackConfig.plugins.push(
            new BundleAnalyzerPlugin({
              analyzerMode: 'static',
              reportFilename: 'bundle-report.html',
              openAnalyzer: false,
            })
          );
        }
      }

      return webpackConfig;
    },
  },
  devServer: {
    configure: (devServerConfig) => {
      devServerConfig.historyApiFallback = true;
      return devServerConfig;
    },
  },
};
```

## Build Monitoring and Validation

### 1. Build Health Checks
```bash
#!/bin/bash
# validate-build.sh - Validate build outputs

set -e

echo "Validating Build Outputs..."

# Backend validation
validate_backend() {
    echo "Validating backend build..."
    
    # Check if API assembly exists
    if [ ! -f "backend/publish/WahadiniCryptoQuest.API.dll" ]; then
        echo "Error: API assembly not found"
        exit 1
    fi
    
    # Check if all dependencies are present
    if [ ! -f "backend/publish/WahadiniCryptoQuest.API.deps.json" ]; then
        echo "Error: Dependencies file not found"
        exit 1
    fi
    
    echo "Backend build validation passed"
}

# Frontend validation
validate_frontend() {
    echo "Validating frontend build..."
    
    # Check if build directory exists
    if [ ! -d "frontend/build" ]; then
        echo "Error: Frontend build directory not found"
        exit 1
    fi
    
    # Check if main files exist
    if [ ! -f "frontend/build/index.html" ]; then
        echo "Error: index.html not found"
        exit 1
    fi
    
    # Check if JavaScript bundles exist
    if ! ls frontend/build/static/js/*.js 1> /dev/null 2>&1; then
        echo "Error: JavaScript bundles not found"
        exit 1
    fi
    
    # Check if CSS bundles exist
    if ! ls frontend/build/static/css/*.css 1> /dev/null 2>&1; then
        echo "Error: CSS bundles not found"
        exit 1
    fi
    
    echo "Frontend build validation passed"
}

# Performance validation
validate_performance() {
    echo "Validating build performance..."
    
    # Check bundle sizes
    JS_SIZE=$(find frontend/build/static/js -name "*.js" -exec wc -c {} + | tail -1 | awk '{print $1}')
    CSS_SIZE=$(find frontend/build/static/css -name "*.css" -exec wc -c {} + | tail -1 | awk '{print $1}')
    
    # Convert to MB for readability
    JS_SIZE_MB=$((JS_SIZE / 1024 / 1024))
    CSS_SIZE_MB=$((CSS_SIZE / 1024 / 1024))
    
    echo "JavaScript bundle size: ${JS_SIZE_MB}MB"
    echo "CSS bundle size: ${CSS_SIZE_MB}MB"
    
    # Warn if bundles are too large
    if [ $JS_SIZE_MB -gt 5 ]; then
        echo "Warning: JavaScript bundle is larger than 5MB"
    fi
    
    if [ $CSS_SIZE_MB -gt 1 ]; then
        echo "Warning: CSS bundle is larger than 1MB"
    fi
    
    echo "Performance validation completed"
}

# Main validation
main() {
    validate_backend
    validate_frontend
    validate_performance
    
    echo "All build validations passed successfully!"
}

main "$@"
```

## Best Practices

### 1. Build Performance
- Use incremental compilation where possible
- Implement proper caching strategies
- Optimize dependency resolution
- Use parallel build processes
- Monitor build times and optimize bottlenecks

### 2. Build Security
- Validate all dependencies for vulnerabilities
- Use lock files for reproducible builds
- Implement build artifact signing
- Scan build outputs for security issues
- Use minimal base images in containers

### 3. Build Reliability
- Implement comprehensive build validation
- Use deterministic build processes
- Version all build artifacts
- Maintain build logs and metrics
- Implement rollback strategies

### 4. Development Experience
- Provide fast development builds
- Implement hot reload for development
- Use source maps for debugging
- Provide clear error messages
- Automate common build tasks

## Instructions

When implementing build and compilation for the WahadiniCryptoQuest:

1. **Clean Builds**: Always start with clean builds for production
2. **Dependency Management**: Use lock files and validate dependencies
3. **Environment Configuration**: Use environment-specific build settings
4. **Testing Integration**: Include testing in the build pipeline
5. **Performance Monitoring**: Monitor and optimize build performance
6. **Validation**: Validate all build outputs before deployment
7. **Documentation**: Document all build processes and requirements
8. **Automation**: Automate repetitive build tasks
9. **Security**: Include security scanning in build processes
10. **Monitoring**: Track build metrics and success rates

Focus on creating reliable, fast, and secure build processes that support the development workflow and ensure high-quality deployments.
