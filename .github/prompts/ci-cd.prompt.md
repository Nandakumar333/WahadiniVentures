# WahadiniCryptoQuest - CI/CD and Deployment Prompt

## Context
You are an expert DevOps engineer working on the WahadiniCryptoQuest CI/CD pipeline. The application uses Docker containers for deployment, includes automatic database migrations, and requires secure handling of financial data across development, staging, and production environments.

## Deployment Architecture Overview

### Container Strategy
- **Multi-stage Docker builds** for optimized production images
- **Docker Compose** for local development and orchestration
- **Health checks** for all services
- **Automatic database migrations** on container startup
- **Volume management** for persistent data

### Environment Strategy
- **Development**: Hot reload, database seeding, detailed logging
- **Staging**: Production-like with test data
- **Production**: SSL, resource limits, security hardening

## Docker Configuration

### 1. Backend Dockerfile
```dockerfile
# Multi-stage build for .NET API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install PostgreSQL client tools and curl for health checks
RUN apt-get update && \
    apt-get install -y postgresql-client curl && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install EF Core tools globally for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Install PostgreSQL client tools
RUN apt-get update && \
    apt-get install -y postgresql-client curl && \
    rm -rf /var/lib/apt/lists/*

# Copy project files and restore dependencies
COPY ["src/WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj", "WahadiniCryptoQuest.API/"]
COPY ["src/WahadiniCryptoQuest.Service/WahadiniCryptoQuest.Service.csproj", "WahadiniCryptoQuest.Service/"]
COPY ["src/WahadiniCryptoQuest.DAL/WahadiniCryptoQuest.DAL.csproj", "WahadiniCryptoQuest.DAL/"]
COPY ["src/WahadiniCryptoQuest.Core/WahadiniCryptoQuest.Core.csproj", "WahadiniCryptoQuest.Core/"]

RUN dotnet restore "WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj"

# Copy source code and build
COPY src/ .
WORKDIR "/src/WahadiniCryptoQuest.API"
RUN dotnet build "WahadiniCryptoQuest.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WahadiniCryptoQuest.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Copy migration scripts
COPY scripts/ /app/scripts/
RUN chmod +x /app/scripts/*.sh

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "WahadiniCryptoQuest.API.dll"]
```

### 2. Frontend Dockerfile
```dockerfile
# Multi-stage build for React app
FROM node:18-alpine as development

WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm ci --only=production

COPY . .

EXPOSE 3000

# Development command with hot reload
CMD ["npm", "start"]

FROM node:18-alpine as build

WORKDIR /app

# Install all dependencies (including dev dependencies)
COPY package*.json ./
RUN npm ci

# Copy source code and build
COPY . .
RUN npm run build

# Production stage with nginx
FROM nginx:alpine as production

# Copy built app to nginx
COPY --from=build /app/build /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Add security headers
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
    root /usr/share/nginx/html; \
    index index.html; \
    \
    # Security headers \
    add_header X-Frame-Options DENY; \
    add_header X-Content-Type-Options nosniff; \
    add_header X-XSS-Protection "1; mode=block"; \
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains"; \
    \
    # Gzip compression \
    gzip on; \
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript; \
    \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
    \
    location /api { \
        proxy_pass http://backend:80; \
        proxy_set_header Host $host; \
        proxy_set_header X-Real-IP $remote_addr; \
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for; \
        proxy_set_header X-Forwarded-Proto $scheme; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80

# Health check for nginx
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:80 || exit 1

CMD ["nginx", "-g", "daemon off;"]
```

### 3. Docker Compose Configuration
```yaml
# docker-compose.yml - Base configuration
version: '3.8'

services:
  database:
    image: postgres:15-alpine
    container_name: personal-finance-db
    environment:
      POSTGRES_DB: ${DB_NAME:-WahadiniCryptoQuest}
      POSTGRES_USER: ${DB_USER:-postgres}
      POSTGRES_PASSWORD: ${DB_PASSWORD:-password}
    ports:
      - "${DB_PORT:-5432}:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/init:/docker-entrypoint-initdb.d
    networks:
      - personal-finance-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER:-postgres}"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
    restart: unless-stopped

  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    container_name: personal-finance-api
    environment:
      - ASPNETCORE_ENVIRONMENT=${ENVIRONMENT:-Development}
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=database;Port=5432;Database=${DB_NAME:-WahadiniCryptoQuest};User Id=${DB_USER:-postgres};Password=${DB_PASSWORD:-password};Include Error Detail=true
      - JwtSettings__SecretKey=${JWT_SECRET}
      - JwtSettings__Issuer=${JWT_ISSUER:-WahadiniCryptoQuestAPI}
      - JwtSettings__Audience=${JWT_AUDIENCE:-WahadiniCryptoQuestApp}
      - SEED_DATABASE=${SEED_DATABASE:-true}
    ports:
      - "${API_PORT:-5000}:80"
    depends_on:
      database:
        condition: service_healthy
    networks:
      - personal-finance-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:80/health || exit 1"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
      target: ${BUILD_TARGET:-development}
    container_name: personal-finance-web
    environment:
      - REACT_APP_API_URL=${REACT_APP_API_URL:-http://localhost:5000/api}
      - NODE_ENV=${NODE_ENV:-development}
    ports:
      - "${WEB_PORT:-3000}:${FRONTEND_PORT:-3000}"
    volumes:
      - ./frontend/src:/app/src
      - ./frontend/public:/app/public
      - /app/node_modules
    networks:
      - personal-finance-network
    depends_on:
      backend:
        condition: service_healthy
    restart: unless-stopped

volumes:
  postgres_data:
    driver: local
    labels:
      - "com.docker.compose.project=WahadiniCryptoQuestapp"

networks:
  personal-finance-network:
    driver: bridge
    labels:
      - "com.docker.compose.project=WahadiniCryptoQuestapp"
```

## Database Migration Strategy

### 1. Automatic Migration Script
```bash
#!/bin/bash
# migrate.sh - Automatic database migration script

set -e

# Configuration
MAX_RETRIES=${MAX_RETRIES:-30}
RETRY_INTERVAL=${RETRY_INTERVAL:-5}
CONNECTION_STRING=${CONNECTION_STRING:-"Server=database;Port=5432;Database=WahadiniCryptoQuest;User Id=postgres;Password=password"}

echo "Starting database migration process..."

# Function to wait for database
wait_for_database() {
    local retries=0
    
    echo "Waiting for database to be ready..."
    while [ $retries -lt $MAX_RETRIES ]; do
        if pg_isready -h database -p 5432 -U postgres; then
            echo "Database is ready!"
            return 0
        fi
        
        echo "Database not ready, waiting ${RETRY_INTERVAL}s (attempt $((retries + 1))/$MAX_RETRIES)..."
        sleep $RETRY_INTERVAL
        retries=$((retries + 1))
    done
    
    echo "Database failed to become ready after $MAX_RETRIES attempts"
    return 1
}

# Function to run migrations
run_migrations() {
    echo "Checking for pending migrations..."
    
    # Check if there are pending migrations
    if dotnet ef database update --connection "$CONNECTION_STRING" --dry-run --project /app/src/WahadiniCryptoQuest.DAL; then
        echo "Running database migrations..."
        dotnet ef database update --connection "$CONNECTION_STRING" --project /app/src/WahadiniCryptoQuest.DAL
        echo "Database migrations completed successfully!"
    else
        echo "No pending migrations found."
    fi
}

# Function to seed database if enabled
seed_database() {
    if [ "${SEED_DATABASE:-false}" = "true" ]; then
        echo "Database seeding enabled. Running seed process..."
        # Add your seeding logic here
        echo "Database seeding completed!"
    else
        echo "Database seeding disabled."
    fi
}

# Main execution
wait_for_database
run_migrations
seed_database

echo "Migration process completed successfully!"
```

### 2. Init Container for Migrations
```yaml
# Migration init container in docker-compose
services:
  migrate:
    build:
      context: ./backend
      dockerfile: Dockerfile
      target: build
    container_name: personal-finance-migrate
    command: ["/app/scripts/migrate.sh"]
    environment:
      - CONNECTION_STRING=Server=database;Port=5432;Database=${DB_NAME:-WahadiniCryptoQuest};User Id=${DB_USER:-postgres};Password=${DB_PASSWORD:-password}
      - SEED_DATABASE=${SEED_DATABASE:-true}
    depends_on:
      database:
        condition: service_healthy
    networks:
      - personal-finance-network
    restart: "no"

  backend:
    # ... backend configuration
    depends_on:
      migrate:
        condition: service_completed_successfully
      database:
        condition: service_healthy
```

## CI/CD Pipeline Configuration

### 1. GitHub Actions Workflow
```yaml
# .github/workflows/ci-cd.yml
name: WahadiniCryptoQuest CI/CD

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: personal-finance-app

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15-alpine
        env:
          POSTGRES_PASSWORD: password
          POSTGRES_DB: WahadiniCryptoQuest_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
        cache-dependency-path: frontend/package-lock.json
    
    - name: Restore backend dependencies
      run: dotnet restore backend/WahadiniCryptoQuest.sln
    
    - name: Build backend
      run: dotnet build backend/WahadiniCryptoQuest.sln --no-restore
    
    - name: Run backend tests
      run: dotnet test backend/WahadiniCryptoQuest.sln --no-build --verbosity normal
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost;Port=5432;Database=WahadiniCryptoQuest_test;User Id=postgres;Password=password"
    
    - name: Install frontend dependencies
      run: npm ci
      working-directory: frontend
    
    - name: Run frontend tests
      run: npm test -- --coverage --watchAll=false
      working-directory: frontend
    
    - name: Build frontend
      run: npm run build
      working-directory: frontend

  security-scan:
    runs-on: ubuntu-latest
    needs: test
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'fs'
        scan-ref: '.'
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload Trivy scan results to GitHub Security tab
      uses: github/codeql-action/upload-sarif@v2
      if: always()
      with:
        sarif_file: 'trivy-results.sarif'

  build-and-push:
    runs-on: ubuntu-latest
    needs: [test, security-scan]
    if: github.ref == 'refs/heads/main'
    
    permissions:
      contents: read
      packages: write
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
    
    - name: Extract metadata for backend
      id: meta-backend
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ github.repository }}/backend
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}
    
    - name: Build and push backend image
      uses: docker/build-push-action@v5
      with:
        context: ./backend
        push: true
        tags: ${{ steps.meta-backend.outputs.tags }}
        labels: ${{ steps.meta-backend.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
    
    - name: Extract metadata for frontend
      id: meta-frontend
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ github.repository }}/frontend
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}
    
    - name: Build and push frontend image
      uses: docker/build-push-action@v5
      with:
        context: ./frontend
        target: production
        push: true
        tags: ${{ steps.meta-frontend.outputs.tags }}
        labels: ${{ steps.meta-frontend.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max

  deploy-staging:
    runs-on: ubuntu-latest
    needs: build-and-push
    if: github.ref == 'refs/heads/develop'
    environment: staging
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to staging
      run: |
        echo "Deploying to staging environment..."
        # Add staging deployment commands here
        # This could involve SSH, kubectl, docker compose, etc.

  deploy-production:
    runs-on: ubuntu-latest
    needs: build-and-push
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to production
      run: |
        echo "Deploying to production environment..."
        # Add production deployment commands here
```

### 2. Environment-Specific Configurations
```yaml
# docker-compose.dev.yml - Development overrides
version: '3.8'

services:
  database:
    environment:
      POSTGRES_DB: WahadiniCryptoQuest_dev
      POSTGRES_USER: dev_user
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5433:5432"
    volumes:
      - postgres_dev_data:/var/lib/postgresql/data

  backend:
    build:
      target: development
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - SEED_DATABASE=true
      - EnableSwagger=true
    ports:
      - "5001:80"
    volumes:
      - ./backend/src:/app/src
      - /app/obj
      - /app/bin

  frontend:
    build:
      target: development
    environment:
      - REACT_APP_API_URL=http://localhost:5001/api
      - NODE_ENV=development
      - FAST_REFRESH=true
    ports:
      - "3001:3000"

volumes:
  postgres_dev_data:
    driver: local
```

```yaml
# docker-compose.prod.yml - Production overrides
version: '3.8'

services:
  database:
    environment:
      POSTGRES_DB: ${DB_NAME}
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_prod_data:/var/lib/postgresql/data
    restart: always
    command: >
      postgres -c log_statement=all
               -c log_destination=stderr
               -c log_min_duration_statement=1000
               -c shared_preload_libraries=pg_stat_statements
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 4G
        reservations:
          cpus: '0.5'
          memory: 1G

  backend:
    build:
      target: production
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
      - JwtSettings__SecretKey=${JWT_SECRET}
      - SEED_DATABASE=false
    ports:
      - "80:80"
    restart: always
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 512M

  frontend:
    build:
      target: production
    environment:
      - REACT_APP_API_URL=${REACT_APP_API_URL}
      - NODE_ENV=production
    ports:
      - "443:80"
    restart: always
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.25'
          memory: 256M

  nginx:
    image: nginx:alpine
    ports:
      - "443:443"
      - "80:80"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    depends_on:
      - frontend
      - backend
    restart: always

volumes:
  postgres_prod_data:
    driver: local
```

## Container Management Scripts

### 1. Docker Management Script
```bash
#!/bin/bash
# docker-manage.sh - Comprehensive container management

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

check_docker() {
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    print_success "Docker is running"
}

# Environment detection
detect_environment() {
    local env=${ENVIRONMENT:-development}
    case $env in
        development|dev)
            export COMPOSE_FILE="docker-compose.yml:docker-compose.dev.yml"
            ;;
        staging)
            export COMPOSE_FILE="docker-compose.yml:docker-compose.staging.yml"
            ;;
        production|prod)
            export COMPOSE_FILE="docker-compose.yml:docker-compose.prod.yml"
            ;;
        *)
            export COMPOSE_FILE="docker-compose.yml"
            ;;
    esac
    print_status "Environment: $env (Using: $COMPOSE_FILE)"
}

# Service health check
check_health() {
    print_status "Checking service health..."
    
    echo "=== Service Status ==="
    docker-compose ps
    
    echo -e "\n=== Database Health ==="
    if docker-compose exec -T database pg_isready -U postgres; then
        print_success "Database is healthy"
    else
        print_warning "Database is not ready"
    fi
    
    echo -e "\n=== Backend Health ==="
    if curl -f http://localhost:${API_PORT:-5000}/health; then
        print_success "Backend is healthy"
    else
        print_warning "Backend health check failed"
    fi
    
    echo -e "\n=== Frontend Health ==="
    if curl -f http://localhost:${WEB_PORT:-3000}; then
        print_success "Frontend is healthy"
    else
        print_warning "Frontend health check failed"
    fi
}

# Backup database
backup_database() {
    local backup_name="backup_$(date +%Y%m%d_%H%M%S).sql"
    print_status "Creating database backup: $backup_name"
    
    docker-compose exec -T database pg_dump -U postgres WahadiniCryptoQuest > "./backups/$backup_name"
    print_success "Database backup created: ./backups/$backup_name"
}

# Clean up old images and volumes
cleanup() {
    print_status "Cleaning up Docker resources..."
    
    # Remove stopped containers
    docker container prune -f
    
    # Remove unused images
    docker image prune -f
    
    # Remove unused volumes (be careful!)
    docker volume prune -f
    
    # Remove unused networks
    docker network prune -f
    
    print_success "Cleanup completed"
}

case "${1:-help}" in
    "up")
        check_docker
        detect_environment
        print_status "Starting services..."
        docker-compose up -d
        sleep 10
        check_health
        ;;
    "down")
        check_docker
        print_status "Stopping services..."
        docker-compose down --remove-orphans
        ;;
    "restart")
        check_docker
        detect_environment
        print_status "Restarting services..."
        docker-compose down --remove-orphans
        docker-compose up -d
        sleep 10
        check_health
        ;;
    "rebuild")
        check_docker
        detect_environment
        print_status "Rebuilding and starting services..."
        docker-compose down --remove-orphans
        docker-compose build --no-cache
        docker-compose up -d
        sleep 10
        check_health
        ;;
    "logs")
        docker-compose logs -f "${2:-}"
        ;;
    "health")
        check_health
        ;;
    "backup")
        backup_database
        ;;
    "cleanup")
        cleanup
        ;;
    "reset")
        check_docker
        print_warning "This will remove ALL containers, volumes, and images. Are you sure? (y/N)"
        read -r response
        if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
            docker-compose down --volumes --rmi all --remove-orphans
            cleanup
            detect_environment
            docker-compose build --no-cache
            docker-compose up -d
            sleep 10
            check_health
        else
            print_status "Reset cancelled"
        fi
        ;;
    *)
        echo "Docker Management Script for WahadiniCryptoQuest"
        echo ""
        echo "Usage: $0 [command] [options]"
        echo ""
        echo "Commands:"
        echo "  up             - Start services"
        echo "  down           - Stop services"
        echo "  restart        - Restart services"
        echo "  rebuild        - Rebuild and start services"
        echo "  logs [service] - Show logs"
        echo "  health         - Check service health"
        echo "  backup         - Backup database"
        echo "  cleanup        - Clean up Docker resources"
        echo "  reset          - Complete reset"
        echo ""
        echo "Environment Variables:"
        echo "  ENVIRONMENT    - Set to 'development', 'staging', or 'production'"
        echo "  API_PORT       - Backend API port (default: 5000)"
        echo "  WEB_PORT       - Frontend port (default: 3000)"
        echo "  DB_PORT        - Database port (default: 5432)"
        ;;
esac
```

## Production Deployment

### 1. Production Environment Setup
```bash
#!/bin/bash
# production-deploy.sh - Production deployment script

set -e

# Configuration
BACKUP_DIR="/backups"
LOG_DIR="/var/log/WahadiniCryptoQuest"
SSL_DIR="/etc/ssl/WahadiniCryptoQuest"

# Create required directories
mkdir -p $BACKUP_DIR $LOG_DIR $SSL_DIR

# Function to validate environment variables
validate_environment() {
    local required_vars=(
        "DB_PASSWORD"
        "JWT_SECRET"
        "CONNECTION_STRING"
        "REACT_APP_API_URL"
    )
    
    for var in "${required_vars[@]}"; do
        if [[ -z "${!var}" ]]; then
            echo "Error: $var is not set"
            exit 1
        fi
    done
    
    echo "All required environment variables are set"
}

# Function to setup SSL certificates
setup_ssl() {
    if [[ ! -f "$SSL_DIR/cert.pem" ]] || [[ ! -f "$SSL_DIR/key.pem" ]]; then
        echo "SSL certificates not found. Setting up Let's Encrypt..."
        
        # Install certbot if not present
        if ! command -v certbot &> /dev/null; then
            sudo apt-get update
            sudo apt-get install -y certbot python3-certbot-nginx
        fi
        
        # Generate certificates
        sudo certbot certonly --standalone -d $DOMAIN_NAME
        
        # Copy certificates
        sudo cp /etc/letsencrypt/live/$DOMAIN_NAME/fullchain.pem $SSL_DIR/cert.pem
        sudo cp /etc/letsencrypt/live/$DOMAIN_NAME/privkey.pem $SSL_DIR/key.pem
        
        echo "SSL certificates setup completed"
    else
        echo "SSL certificates already exist"
    fi
}

# Function to backup before deployment
backup_before_deploy() {
    echo "Creating backup before deployment..."
    
    local backup_name="pre_deploy_$(date +%Y%m%d_%H%M%S)"
    
    # Backup database
    docker-compose exec -T database pg_dump -U postgres WahadiniCryptoQuest > "$BACKUP_DIR/${backup_name}.sql"
    
    # Backup volumes
    docker run --rm -v postgres_prod_data:/data -v $BACKUP_DIR:/backup alpine tar czf /backup/${backup_name}_volumes.tar.gz -C /data .
    
    echo "Backup completed: $backup_name"
}

# Function to deploy application
deploy_application() {
    echo "Deploying WahadiniCryptoQuest to production..."
    
    # Set environment
    export ENVIRONMENT=production
    export COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml
    
    # Pull latest images
    docker-compose pull
    
    # Start services
    docker-compose up -d
    
    # Wait for services to be healthy
    echo "Waiting for services to become healthy..."
    sleep 30
    
    # Health check
    if ./docker-manage.sh health; then
        echo "Deployment successful!"
    else
        echo "Deployment failed - rolling back..."
        rollback_deployment
        exit 1
    fi
}

# Function to rollback deployment
rollback_deployment() {
    echo "Rolling back deployment..."
    
    # Stop current services
    docker-compose down
    
    # Restore previous backup (implement based on your backup strategy)
    # This is a simplified example
    echo "Rollback completed"
}

# Main deployment process
main() {
    echo "Starting production deployment process..."
    
    validate_environment
    setup_ssl
    backup_before_deploy
    deploy_application
    
    echo "Production deployment completed successfully!"
}

# Execute main function
main "$@"
```

### 2. Monitoring and Logging
```yaml
# docker-compose.monitoring.yml - Add monitoring services
version: '3.8'

services:
  prometheus:
    image: prom/prometheus
    container_name: prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
    networks:
      - personal-finance-network

  grafana:
    image: grafana/grafana
    container_name: grafana
    ports:
      - "3001:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD}
    volumes:
      - grafana_data:/var/lib/grafana
      - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources
    networks:
      - personal-finance-network

  loki:
    image: grafana/loki
    container_name: loki
    ports:
      - "3100:3100"
    volumes:
      - ./monitoring/loki.yml:/etc/loki/local-config.yaml
      - loki_data:/loki
    command: -config.file=/etc/loki/local-config.yaml
    networks:
      - personal-finance-network

volumes:
  prometheus_data:
  grafana_data:
  loki_data:
```

## Best Practices

### 1. Security Hardening
- Use non-root users in containers
- Scan images for vulnerabilities
- Use multi-stage builds to reduce image size
- Implement proper secret management
- Enable Docker Content Trust

### 2. Performance Optimization
- Use Docker layer caching
- Optimize Dockerfile for build speed
- Implement health checks for all services
- Use appropriate resource limits
- Monitor container performance

### 3. Reliability
- Implement proper restart policies
- Use dependency checks between services
- Implement graceful shutdown handling
- Regular backup automation
- Disaster recovery procedures

### 4. Development Workflow
- Use consistent naming conventions
- Implement proper logging
- Use environment-specific configurations
- Automate testing in CI/CD
- Version control all configuration files

## Instructions

When implementing CI/CD for the WahadiniCryptoQuest:

1. **Container Security**: Always scan images for vulnerabilities and use minimal base images
2. **Environment Parity**: Ensure development, staging, and production environments are as similar as possible
3. **Database Migrations**: Implement safe, reversible database migration strategies
4. **Health Checks**: Add comprehensive health checks for all services
5. **Monitoring**: Implement proper logging and monitoring for production deployments
6. **Backup Strategy**: Automate regular backups and test restore procedures
7. **Secret Management**: Use proper secret management for sensitive configuration
8. **Rolling Deployments**: Implement zero-downtime deployment strategies
9. **Rollback Plan**: Always have a tested rollback procedure ready
10. **Documentation**: Keep deployment documentation updated and accessible

Focus on reliability and security given the financial nature of the application data.
