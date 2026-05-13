# Developer Quickstart Guide

**WahadiniCryptoQuest Platform** - Course & Lesson Management System  
**Feature Branch**: `003-course-management`  
**Last Updated**: November 15, 2025

---

## Overview

This quickstart guide helps developers set up the WahadiniCryptoQuest Course & Lesson Management System locally, run tests, and contribute to the project. The system enables administrators to create and manage crypto education courses with YouTube video lessons, while users can browse, enroll, and track their learning progress.

**Prerequisites**:
- .NET 8 SDK
- Node.js 18+ (LTS recommended)
- PostgreSQL 15+
- Docker Desktop (for database container)
- Git

---

## Quick Setup (5 Minutes)

### 1. Clone Repository

```bash
git clone https://github.com/wahadinicryptoquest/platform.git
cd platform
git checkout 003-course-management
```

### 2. Start PostgreSQL Database

```bash
# Using Docker Compose (recommended)
docker-compose up -d postgres

# Verify database is running
docker ps | grep postgres
```

**Connection String**:
```
Host=localhost;Port=5432;Database=wahadini_dev;Username=postgres;Password=postgres
```

### 3. Setup Backend

```bash
cd backend

# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API

# Run backend
dotnet run --project src/WahadiniCryptoQuest.API
```

**Backend URL**: `https://localhost:5001`  
**Swagger Docs**: `https://localhost:5001/swagger`

### 4. Setup Frontend

```bash
cd ../frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

**Frontend URL**: `http://localhost:5173`

### 5. Verify Setup

Open browser and navigate to:
- Frontend: `http://localhost:5173`
- Swagger API Docs: `https://localhost:5001/swagger`

---

## Detailed Setup

### Environment Configuration

#### Backend Configuration

**File**: `backend/src/WahadiniCryptoQuest.API/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=wahadini_dev;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters-long",
    "Issuer": "WahadiniCryptoQuest",
    "Audience": "WahadiniCryptoQuest",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "https://localhost:5173"
    ]
  }
}
```

#### Frontend Configuration

**File**: `frontend/.env.development`

```env
VITE_API_BASE_URL=https://localhost:5001/api
VITE_APP_NAME=WahadiniCryptoQuest
VITE_ENABLE_ANALYTICS=false
```

### Database Migrations

#### Create New Migration

```bash
cd backend

dotnet ef migrations add MigrationName \
  --project src/WahadiniCryptoQuest.DAL \
  --startup-project src/WahadiniCryptoQuest.API
```

#### Apply Migrations

```bash
dotnet ef database update \
  --project src/WahadiniCryptoQuest.DAL \
  --startup-project src/WahadiniCryptoQuest.API
```

#### Rollback Migration

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName \
  --project src/WahadiniCryptoQuest.DAL \
  --startup-project src/WahadiniCryptoQuest.API

# Rollback all migrations
dotnet ef database update 0 \
  --project src/WahadiniCryptoQuest.DAL \
  --startup-project src/WahadiniCryptoQuest.API
```

### Seed Data

**Run Seed Script**:

```bash
cd backend

# Seed categories, users, sample courses
dotnet run --project src/WahadiniCryptoQuest.API -- seed-data
```

**Seeded Data Includes**:
- **5 Categories**: Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies
- **3 Users**: Admin, Premium User, Free User
- **10 Sample Courses**: Mix of free and premium across all categories
- **30 Lessons**: 3 lessons per course

**Test User Credentials**:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@wahadini.com | Admin@123 |
| Premium User | premium@wahadini.com | Premium@123 |
| Free User | free@wahadini.com | Free@123 |

---

## Running Tests

### Backend Tests

**Run All Tests**:
```bash
cd backend
dotnet test
```

**Run Specific Test Project**:
```bash
# Service layer unit tests
dotnet test tests/WahadiniCryptoQuest.Service.Tests

# API integration tests
dotnet test tests/WahadiniCryptoQuest.API.Tests

# Repository tests
dotnet test tests/WahadiniCryptoQuest.DAL.Tests
```

**Run Tests with Coverage**:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML coverage report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

**Coverage Targets**:
- Service Layer: ≥85%
- Repository Layer: ≥80%
- API Controllers: ≥75%

### Frontend Tests

**Run Unit Tests**:
```bash
cd frontend
npm test
```

**Run Tests with Coverage**:
```bash
npm run test:coverage
```

**Run Tests in Watch Mode**:
```bash
npm run test:watch
```

**Run E2E Tests** (Playwright):
```bash
# Install Playwright browsers (first time only)
npx playwright install

# Run E2E tests
npm run test:e2e

# Run E2E tests in UI mode
npm run test:e2e:ui
```

**Coverage Targets**:
- Components: ≥80%
- Hooks: ≥85%
- Services: ≥80%
- E2E: 5 critical flows automated

### Continuous Integration

**GitHub Actions** (`.github/workflows/test.yml`):
- Runs on every PR
- Backend unit tests (<5 min)
- Frontend unit tests (<3 min)
- Coverage reports uploaded to Codecov
- Fails if coverage drops below thresholds

---

## Project Structure

```
backend/
└── src/
    ├── WahadiniCryptoQuest.API/          # Presentation Layer
    │   ├── Controllers/                   # API endpoints
    │   ├── Validators/                    # FluentValidation
    │   ├── Middleware/                    # Rate limiting, error handling
    │   └── Program.cs                     # Application entry point
    │
    ├── WahadiniCryptoQuest.Core/         # Domain Layer
    │   ├── Entities/                      # Domain entities (Course, Lesson)
    │   ├── DTOs/                          # Data transfer objects
    │   ├── Interfaces/                    # Abstractions
    │   └── Enums/                         # Domain enums
    │
    ├── WahadiniCryptoQuest.Service/      # Application Layer
    │   ├── Course/                        # Course service & handlers
    │   ├── Lesson/                        # Lesson service & handlers
    │   └── Mappings/                      # AutoMapper profiles
    │
    └── WahadiniCryptoQuest.DAL/          # Infrastructure Layer
        ├── Repositories/                  # Data access
        ├── Context/                       # EF Core DbContext
        └── Migrations/                    # Database migrations

frontend/
└── src/
    ├── components/                        # React components
    │   ├── courses/                       # Course components
    │   ├── lessons/                       # Lesson components
    │   ├── admin/                         # Admin components
    │   └── ui/                            # Shared UI components
    │
    ├── hooks/                             # Custom React hooks
    │   ├── courses/                       # Course hooks
    │   └── lessons/                       # Lesson hooks
    │
    ├── services/                          # API services
    │   └── api/                           # HTTP client (Axios)
    │
    ├── store/                             # Zustand stores
    ├── types/                             # TypeScript types
    └── utils/                             # Utility functions
```

---

## Development Workflow

### 1. Create Feature Branch

```bash
git checkout -b feature/my-feature-name
```

**Branch Naming**:
- `feature/` - New features
- `bugfix/` - Bug fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates

### 2. Implement Changes

**Backend (Clean Architecture)**:
1. Define entity in `Core/Entities/`
2. Create DTOs in `Core/DTOs/`
3. Implement service in `Service/`
4. Add repository in `DAL/Repositories/`
5. Create controller in `API/Controllers/`
6. Add validators in `API/Validators/`
7. Write unit tests in `tests/`

**Frontend (Component-Based)**:
1. Define types in `types/`
2. Create service in `services/api/`
3. Build custom hook in `hooks/`
4. Implement component in `components/`
5. Add page in `pages/`
6. Write unit tests in `__tests__/`

### 3. Run Tests Locally

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && npm test
```

### 4. Commit Changes

**Commit Message Format**:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Example**:
```
feat(courses): add lesson reordering functionality

Implement drag-and-drop lesson reordering with @dnd-kit/sortable.
Admin can reorder lessons and changes persist to backend.

Closes #123
```

**Types**:
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation
- `style` - Code style (formatting)
- `refactor` - Code refactoring
- `test` - Adding tests
- `chore` - Build process, dependencies

### 5. Push and Create PR

```bash
git push origin feature/my-feature-name
```

**PR Template**:
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Backend unit tests passing
- [ ] Frontend unit tests passing
- [ ] E2E tests passing (if applicable)
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Tests added/updated
- [ ] All tests passing
```

---

## Common Tasks

### Add New API Endpoint

**1. Define DTO** (`Core/DTOs/`):
```csharp
public class CreateCourseDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
}
```

**2. Create Validator** (`API/Validators/`):
```csharp
public class CreateCourseValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

**3. Implement Service Method** (`Service/Course/CourseService.cs`):
```csharp
public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
{
    var course = new Course
    {
        Title = dto.Title,
        Description = dto.Description,
        CategoryId = dto.CategoryId
    };

    await _courseRepository.AddAsync(course);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<CourseDto>(course);
}
```

**4. Add Controller Action** (`API/Controllers/CoursesController.cs`):
```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CreateCourseDto dto)
{
    var course = await _courseService.CreateCourseAsync(dto);
    return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
}
```

**5. Write Tests**:
```csharp
[Fact]
public async Task CreateCourse_ValidDto_ReturnsCreatedCourse()
{
    // Arrange
    var dto = new CreateCourseDto { Title = "Test Course", ... };

    // Act
    var result = await _courseService.CreateCourseAsync(dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test Course", result.Title);
}
```

### Add New React Component

**1. Create Component** (`components/courses/CourseCard.tsx`):
```tsx
interface CourseCardProps {
  course: Course;
  onEnroll?: (courseId: string) => void;
}

export const CourseCard: React.FC<CourseCardProps> = ({ course, onEnroll }) => {
  return (
    <div className="bg-white rounded-lg shadow p-4">
      <h3 className="text-xl font-bold">{course.title}</h3>
      <p className="text-gray-600">{course.description}</p>
      <button onClick={() => onEnroll?.(course.id)}>
        Enroll
      </button>
    </div>
  );
};
```

**2. Create Hook** (`hooks/courses/useCourse.ts`):
```typescript
export function useCourse(courseId: string) {
  return useQuery({
    queryKey: ['course', courseId],
    queryFn: () => courseService.getCourse(courseId),
  });
}
```

**3. Write Tests** (`components/courses/__tests__/CourseCard.test.tsx`):
```tsx
describe('CourseCard', () => {
  it('renders course title and description', () => {
    const course = { id: '1', title: 'Test Course', description: 'Test Description' };
    render(<CourseCard course={course} />);

    expect(screen.getByText('Test Course')).toBeInTheDocument();
    expect(screen.getByText('Test Description')).toBeInTheDocument();
  });

  it('calls onEnroll when enroll button clicked', () => {
    const handleEnroll = vi.fn();
    const course = { id: '1', title: 'Test Course' };
    render(<CourseCard course={course} onEnroll={handleEnroll} />);

    fireEvent.click(screen.getByText('Enroll'));
    expect(handleEnroll).toHaveBeenCalledWith('1');
  });
});
```

---

## Debugging

### Backend Debugging (Visual Studio Code)

**Launch Configuration** (`.vscode/launch.json`):
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/backend/src/WahadiniCryptoQuest.API/bin/Debug/net8.0/WahadiniCryptoQuest.API.dll",
      "args": [],
      "cwd": "${workspaceFolder}/backend/src/WahadiniCryptoQuest.API",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

**Set Breakpoints**: Click left margin in code editor  
**Start Debugging**: Press `F5`

### Frontend Debugging (Browser DevTools)

**Chrome DevTools**:
1. Open DevTools (`F12`)
2. Navigate to **Sources** tab
3. Find file in file tree (`webpack://` → `src/`)
4. Set breakpoints by clicking line number
5. Refresh page to hit breakpoints

**React DevTools Extension**:
- Install from Chrome Web Store
- Inspect component props and state
- View component hierarchy

### Database Debugging

**View Database Tables**:
```bash
# Using psql
docker exec -it wahadini_postgres psql -U postgres -d wahadini_dev

# List tables
\dt

# View courses
SELECT * FROM "Courses";

# View lessons
SELECT * FROM "Lessons" WHERE "CourseId" = 'uuid-here';
```

**pgAdmin** (GUI tool):
1. Install pgAdmin
2. Connect to `localhost:5432`
3. Browse database visually

---

## Troubleshooting

### Backend Won't Start

**Error**: "Unable to connect to database"

**Solution**:
1. Verify PostgreSQL container is running: `docker ps`
2. Check connection string in `appsettings.Development.json`
3. Test connection: `docker exec -it wahadini_postgres psql -U postgres -d wahadini_dev`

**Error**: "Port 5001 already in use"

**Solution**:
```bash
# Find process using port
netstat -ano | findstr :5001

# Kill process (Windows)
taskkill /PID <process_id> /F

# Kill process (Linux/Mac)
kill -9 <process_id>
```

### Frontend Won't Start

**Error**: "Module not found"

**Solution**:
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

**Error**: "CORS policy blocked"

**Solution**:
1. Verify `CorsSettings` in `appsettings.Development.json` includes `http://localhost:5173`
2. Restart backend after config changes
3. Clear browser cache

### Tests Failing

**Error**: "Database connection failed in tests"

**Solution**:
- Integration tests use TestContainers (requires Docker running)
- Start Docker Desktop before running tests
- Verify Docker daemon is running: `docker info`

**Error**: "Timeout waiting for video element"

**Solution** (E2E tests):
- Increase timeout in Playwright config
- Check if YouTube video ID is valid
- Verify internet connection (E2E tests load real videos)

---

## Performance Tips

### Backend

1. **Enable Response Caching**:
```csharp
[HttpGet]
[ResponseCache(Duration = 300)] // Cache for 5 minutes
public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
{
    // ...
}
```

2. **Use AsNoTracking for Read-Only Queries**:
```csharp
return await _context.Courses
    .AsNoTracking()
    .Where(c => c.IsPublished)
    .ToListAsync();
```

3. **Optimize Database Queries with Indexes**:
```csharp
modelBuilder.Entity<Course>()
    .HasIndex(c => new { c.CategoryId, c.IsPublished, c.DifficultyLevel });
```

### Frontend

1. **Code Splitting with React.lazy**:
```tsx
const AdminDashboard = lazy(() => import('./pages/admin/AdminDashboard'));
```

2. **Memoize Expensive Computations**:
```tsx
const filteredCourses = useMemo(() => {
  return courses.filter(c => c.category === selectedCategory);
}, [courses, selectedCategory]);
```

3. **Optimize React Query Cache**:
```tsx
queryClient.setDefaultOptions({
  queries: {
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
  },
});
```

---

## Contributing Guidelines

### Code Style

**Backend (C#)**:
- Follow Microsoft C# coding conventions
- Use PascalCase for public members
- Use camelCase for private fields (prefix with `_`)
- Add XML documentation comments for public APIs
- Run `dotnet format` before committing

**Frontend (TypeScript/React)**:
- Follow Airbnb React/JSX Style Guide
- Use PascalCase for components
- Use camelCase for functions and variables
- Use 2-space indentation
- Run `npm run lint` before committing

### Pull Request Guidelines

1. **Keep PRs Small**: Ideally <500 lines changed
2. **Write Clear Descriptions**: Explain what, why, and how
3. **Include Tests**: All new code must have tests
4. **Update Documentation**: README, code comments, API docs
5. **Link Issues**: Reference issue numbers in PR description
6. **Request Reviews**: Tag at least 2 reviewers
7. **Address Feedback**: Respond to all review comments

### Code Review Checklist

**Reviewer Responsibilities**:
- [ ] Code follows project conventions
- [ ] Logic is clear and maintainable
- [ ] Tests cover new functionality
- [ ] Documentation is updated
- [ ] No security vulnerabilities introduced
- [ ] Performance implications considered
- [ ] No breaking changes (or properly documented)

---

## Additional Resources

**Documentation**:
- [Admin Course Management Guide](../docs/admin-course-management-guide.md)
- [YouTube Integration Guide](../docs/youtube-integration.md)
- [API Documentation](https://localhost:5001/swagger)

**External Resources**:
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [React Documentation](https://react.dev/)
- [TailwindCSS Documentation](https://tailwindcss.com/docs)

**Community**:
- GitHub Discussions: `https://github.com/wahadinicryptoquest/platform/discussions`
- Slack: `#dev-course-management`
- Email: dev@wahadinicryptoquest.com

---

## Getting Help

**Stuck? Try these steps**:

1. **Search Documentation**: Check this guide, admin guide, and YouTube integration docs
2. **Search GitHub Issues**: Someone may have encountered the same problem
3. **Ask in Slack**: `#dev-course-management` channel for quick questions
4. **Create GitHub Issue**: For bugs or feature requests
5. **Stack Overflow**: Tag with `wahadinicryptoquest` for community help

**When Reporting Issues**:
- Include error messages (full stack trace)
- Describe steps to reproduce
- Mention environment (OS, .NET version, Node version)
- Attach relevant code snippets
- Share screenshots if applicable

---

## Changelog

### Version 1.0 (November 2025)
- Initial developer quickstart guide
- Setup instructions for backend and frontend
- Testing guidelines documented
- Development workflow defined
- Troubleshooting section added

---

**Happy Coding! 🚀**
