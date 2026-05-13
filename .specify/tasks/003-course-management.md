# Feature: Course & Lesson Management System

## /speckit.specify

### Feature Overview
Implement a complete course and lesson management system allowing administrators to create educational content, organize courses by categories, manage lessons with YouTube videos, and enable users to enroll and track their learning progress.

### Feature Scope
**Included:**
- Course CRUD operations (Create, Read, Update, Delete)
- Lesson management with YouTube video integration
- Course categorization and organization
- Difficulty level assignment (Beginner, Intermediate, Advanced)
- Premium content flagging
- User enrollment system
- Course and lesson visibility controls
- View count tracking
- Admin course editor interface
- Public course listing and discovery

**Excluded:**
- Video hosting (using YouTube only)
- Live streaming functionality
- Course recommendations (future enhancement)
- Course reviews/ratings (future enhancement)
- Course certificates (handled by subscription feature)

### User Stories
1. As an admin, I want to create new courses with multiple lessons so users can learn systematically
2. As an admin, I want to organize courses by categories (Airdrops, GameFi, DeFi, etc.)
3. As an admin, I want to add YouTube videos to lessons with descriptions
4. As an admin, I want to set difficulty levels and premium flags for courses
5. As a user, I want to browse available courses by category
6. As a user, I want to enroll in courses I'm interested in
7. As a user, I want to see my enrolled courses on my dashboard
8. As a premium user, I want access to premium courses
9. As a free user, I want to see preview information for premium courses
10. As a user, I want to track my progress through courses

### Technical Requirements
- Backend: .NET 8 C#, ASP.NET Core Web API
- Database: PostgreSQL with existing Course, Lesson, Category entities
- Frontend: React 18, TypeScript, Vite
- State Management: Zustand + React Query
- Validation: FluentValidation (backend), Zod (frontend)
- Authorization: JWT-based, role checks (Admin for creation)
- File handling: YouTube video IDs only (no file uploads for videos)
- Pagination: For course lists
- Rich text: Support markdown in course/lesson descriptions

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Backend Course Services (8 hours)
**Tasks:**
1. Create ICourseRepository and implementation
2. Create ILessonRepository and implementation  
3. Implement CourseService with business logic
4. Implement LessonService with business logic
5. Add validation rules for courses and lessons
6. Implement enrollment logic

**Deliverables:**
- CourseRepository with CRUD and filtering methods
- LessonRepository with CRUD methods
- CourseService with CreateCourse, UpdateCourse, DeleteCourse, EnrollUser
- LessonService with CreateLesson, UpdateLesson, DeleteLesson
- FluentValidation validators

#### Phase 2: Backend API Endpoints (6 hours)
**Tasks:**
1. Create CoursesController with REST endpoints
2. Create LessonsController with REST endpoints
3. Implement filtering and pagination
4. Add authorization attributes
5. Configure AutoMapper profiles
6. Add Swagger documentation

**Deliverables:**
- GET /api/courses (with filters)
- GET /api/courses/{id}
- POST /api/courses (admin only)
- PUT /api/courses/{id} (admin only)
- DELETE /api/courses/{id} (admin only)
- POST /api/courses/{id}/enroll
- GET /api/courses/my-courses
- Lesson endpoints under courses

#### Phase 3: Frontend Course Services (4 hours)
**Tasks:**
1. Create courseService.ts for API integration
2. Create lessonService.ts for API integration
3. Create course-related TypeScript interfaces
4. Set up React Query hooks
5. Create Zustand store for course state

**Deliverables:**
- courseService with all API methods
- lessonService with all API methods
- Course, Lesson, Enrollment types
- useCourses, useCourse, useEnrollment hooks
- courseStore.ts

#### Phase 4: Frontend Course Display (8 hours)
**Tasks:**
1. Create CoursesPage with grid layout
2. Create CourseCard component
3. Create CourseDetailPage
4. Create LessonList component
5. Implement course filters (category, difficulty, premium)
6. Add pagination
7. Style with TailwindCSS

**Deliverables:**
- CoursesPage.tsx (course listing)
- CourseCard.tsx (course preview card)
- CourseDetailPage.tsx (full course details)
- LessonList.tsx (lessons in course)
- CourseFilters.tsx (filter panel)

#### Phase 5: Admin Course Editor (10 hours)
**Tasks:**
1. Create AdminCoursesPage
2. Create CourseEditor component (create/edit form)
3. Create LessonEditor component
4. Implement drag-and-drop lesson reordering
5. Add rich text editor for descriptions
6. Implement YouTube video ID extraction
7. Add course preview functionality

**Deliverables:**
- AdminCoursesPage.tsx (admin course list)
- CourseEditor.tsx (comprehensive course form)
- LessonEditor.tsx (lesson form)
- DraggableLessonList.tsx
- Course preview modal

#### Phase 6: Enrollment & Access Control (4 hours)
**Tasks:**
1. Implement enrollment API calls
2. Create enrollment confirmation UI
3. Add premium content gates
4. Implement "My Courses" page
5. Add enrollment status indicators

**Deliverables:**
- Enrollment functionality
- PremiumCourseGate component
- MyCoursesPage.tsx
- Enrollment status badges

---

## /speckit.clarify

### Questions & Answers

**Q: Can multiple admins edit the same course simultaneously?**
A: For MVP, no conflict resolution. Last write wins. Add optimistic locking in future versions with version numbers.

**Q: How to handle deleted YouTube videos?**
A: Store video ID and handle 404 gracefully in frontend. Show "Video unavailable" message. Admin can update video ID.

**Q: Should course enrollment be automatic or require confirmation?**
A: Automatic for free courses. For premium, check subscription status first.

**Q: Can users unenroll from courses?**
A: Yes, but keep enrollment history for analytics. Add IsActive flag to UserCourseEnrollment.

**Q: Maximum number of lessons per course?**
A: No hard limit in MVP. Consider pagination for courses with 50+ lessons in future.

**Q: How to handle course drafts?**
A: Use IsPublished flag. Draft courses only visible to admins.

**Q: Can lesson order be changed after course is published?**
A: Yes, but may affect user progress tracking. Recalculate progress after reordering.

**Q: Should deleted courses remain in database?**
A: Soft delete (IsActive = false) to preserve user enrollment history and progress.

**Q: How to handle course prerequisites?**
A: Not in MVP. Add prerequisite courses feature in future version.

**Q: Rich text editor - which library?**
A: Use react-markdown for display, simple textarea for MVP. Consider TinyMCE or Quill for future.

---

## /speckit.analyze

### Technical Architecture

#### Backend Structure
```
WahadiniCryptoQuest.Application/
├── Interfaces/
│   ├── ICourseRepository.cs
│   ├── ILessonRepository.cs
│   ├── ICourseService.cs
│   └── ILessonService.cs
├── Services/
│   ├── CourseService.cs
│   └── LessonService.cs
├── DTOs/
│   ├── CourseDto.cs
│   ├── CreateCourseDto.cs
│   ├── UpdateCourseDto.cs
│   ├── LessonDto.cs
│   ├── CreateLessonDto.cs
│   └── EnrollmentDto.cs
└── Validators/
    ├── CreateCourseValidator.cs
    └── CreateLessonValidator.cs

WahadiniCryptoQuest.Infrastructure/
└── Repositories/
    ├── CourseRepository.cs
    └── LessonRepository.cs

WahadiniCryptoQuest.API/
└── Controllers/
    ├── CoursesController.cs
    └── LessonsController.cs
```

#### Frontend Structure
```
src/
├── services/
│   ├── courseService.ts
│   └── lessonService.ts
├── stores/
│   └── courseStore.ts
├── hooks/
│   ├── useCourses.ts
│   ├── useCourse.ts
│   └── useEnrollment.ts
├── components/
│   └── course/
│       ├── CourseCard.tsx
│       ├── CourseFilters.tsx
│       ├── LessonList.tsx
│       ├── LessonCard.tsx
│       ├── EnrollButton.tsx
│       └── CourseProgress.tsx
├── pages/
│   ├── CoursesPage.tsx
│   ├── CourseDetailPage.tsx
│   ├── MyCoursesPage.tsx
│   └── admin/
│       ├── AdminCoursesPage.tsx
│       ├── CourseEditor.tsx
│       └── LessonEditor.tsx
└── types/
    └── course.types.ts
```

#### API Endpoints

```
Public Endpoints:
GET    /api/courses
       Query: ?categoryId=guid&difficulty=Beginner&isPremium=false&search=text&page=1&pageSize=12
       Response: PaginatedResponse<CourseDto>

GET    /api/courses/{id}
       Response: CourseDetailDto (includes lessons)

Authenticated Endpoints:
POST   /api/courses/{id}/enroll
       Response: EnrollmentDto

GET    /api/courses/my-courses
       Response: List<EnrolledCourseDto>

Admin Endpoints:
POST   /api/courses
       Body: CreateCourseDto
       Response: CourseDto

PUT    /api/courses/{id}
       Body: UpdateCourseDto
       Response: CourseDto

DELETE /api/courses/{id}
       Response: 204 No Content

PUT    /api/courses/{id}/publish
       Response: CourseDto

POST   /api/courses/{id}/lessons
       Body: CreateLessonDto
       Response: LessonDto

PUT    /api/lessons/{id}
       Body: UpdateLessonDto
       Response: LessonDto

DELETE /api/lessons/{id}
       Response: 204 No Content

PUT    /api/lessons/{id}/reorder
       Body: { newOrderIndex: number }
       Response: 200 OK
```

#### Data Transfer Objects

```csharp
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public string CategoryName { get; set; }
    public string DifficultyLevel { get; set; }
    public int EstimatedDuration { get; set; }
    public bool IsPremium { get; set; }
    public int RewardPoints { get; set; }
    public int LessonCount { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CourseDetailDto : CourseDto
{
    public List<LessonDto> Lessons { get; set; }
    public bool IsEnrolled { get; set; }
    public decimal? UserProgress { get; set; }
}

public class CreateCourseDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; }
    
    [Required, MaxLength(2000)]
    public string Description { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    public string DifficultyLevel { get; set; }
    
    public int EstimatedDuration { get; set; }
    public bool IsPremium { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class LessonDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string YoutubeVideoId { get; set; }
    public int Duration { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPremium { get; set; }
    public int RewardPoints { get; set; }
}
```

#### State Management (Frontend)

```typescript
interface CourseStore {
  courses: Course[];
  selectedCourse: CourseDetail | null;
  enrolledCourses: EnrolledCourse[];
  filters: CourseFilters;
  setFilters: (filters: CourseFilters) => void;
  fetchCourses: () => Promise<void>;
  fetchCourse: (id: string) => Promise<void>;
  enrollInCourse: (courseId: string) => Promise<void>;
}
```

#### Security Measures
1. Role-based authorization for admin endpoints
2. Premium content access validation
3. Enrollment verification before allowing lesson access
4. Input validation on all DTOs
5. SQL injection prevention via EF Core parameterization
6. XSS prevention in markdown rendering
7. Rate limiting on course creation endpoints

---

## /speckit.checklist

### Implementation Checklist

#### Backend Setup
- [ ] Review existing Course and Lesson entities
- [ ] Create ICourseRepository interface
- [ ] Create ILessonRepository interface
- [ ] Implement CourseRepository
- [ ] Implement LessonRepository
- [ ] Create ICourseService interface
- [ ] Create ILessonService interface
- [ ] Implement CourseService
- [ ] Implement LessonService

#### DTOs & Validation
- [ ] Create CourseDto
- [ ] Create CourseDetailDto
- [ ] Create CreateCourseDto
- [ ] Create UpdateCourseDto
- [ ] Create LessonDto
- [ ] Create CreateLessonDto
- [ ] Create UpdateLessonDto
- [ ] Create EnrollmentDto
- [ ] Create CreateCourseValidator
- [ ] Create CreateLessonValidator
- [ ] Create UpdateCourseValidator

#### Controllers & Endpoints
- [ ] Create CoursesController
- [ ] Implement GET /api/courses endpoint with filters
- [ ] Implement GET /api/courses/{id} endpoint
- [ ] Implement POST /api/courses endpoint (admin)
- [ ] Implement PUT /api/courses/{id} endpoint (admin)
- [ ] Implement DELETE /api/courses/{id} endpoint (admin)
- [ ] Implement POST /api/courses/{id}/enroll endpoint
- [ ] Implement GET /api/courses/my-courses endpoint
- [ ] Create LessonsController
- [ ] Implement lesson CRUD endpoints
- [ ] Add authorization attributes
- [ ] Configure AutoMapper profiles

#### Frontend Services
- [ ] Create courseService.ts
- [ ] Implement getCourses with filters
- [ ] Implement getCourse by ID
- [ ] Implement createCourse (admin)
- [ ] Implement updateCourse (admin)
- [ ] Implement deleteCourse (admin)
- [ ] Implement enrollInCourse
- [ ] Implement getEnrolledCourses
- [ ] Create lessonService.ts
- [ ] Implement lesson CRUD methods

#### TypeScript Types
- [ ] Create Course interface
- [ ] Create CourseDetail interface
- [ ] Create Lesson interface
- [ ] Create Enrollment interface
- [ ] Create CourseFilters interface
- [ ] Create PaginatedCourses interface

#### State Management
- [ ] Create courseStore with Zustand
- [ ] Add courses state
- [ ] Add selectedCourse state
- [ ] Add enrolledCourses state
- [ ] Add filters state
- [ ] Implement fetchCourses action
- [ ] Implement fetchCourse action
- [ ] Implement enrollInCourse action
- [ ] Create useCourses hook
- [ ] Create useCourse hook
- [ ] Create useEnrollment hook

#### Course Display Components
- [ ] Create CoursesPage layout
- [ ] Create CourseCard component
- [ ] Add course thumbnail display
- [ ] Add difficulty badge
- [ ] Add premium badge
- [ ] Add reward points display
- [ ] Create CourseFilters component
- [ ] Add category filter dropdown
- [ ] Add difficulty filter
- [ ] Add premium toggle
- [ ] Add search input
- [ ] Implement pagination controls

#### Course Detail Page
- [ ] Create CourseDetailPage layout
- [ ] Display course header information
- [ ] Show course description (markdown)
- [ ] Display lesson list
- [ ] Add enrollment button
- [ ] Show enrollment status
- [ ] Display progress indicator
- [ ] Add navigation breadcrumbs
- [ ] Create LessonList component
- [ ] Create LessonCard component
- [ ] Add lesson duration display
- [ ] Add lesson completion status

#### Admin Course Management
- [ ] Create AdminCoursesPage
- [ ] Display courses table with actions
- [ ] Add "Create Course" button
- [ ] Create CourseEditor modal/page
- [ ] Add course basic info form
- [ ] Add category selection
- [ ] Add difficulty selection
- [ ] Add premium toggle
- [ ] Add thumbnail URL input
- [ ] Create LessonEditor component
- [ ] Add lesson title and description inputs
- [ ] Add YouTube video ID input
- [ ] Add video ID validation
- [ ] Add duration input
- [ ] Add reward points input
- [ ] Implement lesson reordering (drag-drop)
- [ ] Add lesson delete confirmation
- [ ] Add course preview button
- [ ] Implement course publish/unpublish

#### Enrollment System
- [ ] Create EnrollButton component
- [ ] Add enrollment confirmation
- [ ] Check premium status before enrollment
- [ ] Show upgrade prompt for premium courses
- [ ] Create MyCoursesPage
- [ ] Display enrolled courses grid
- [ ] Show progress for each course
- [ ] Add "Continue Learning" buttons
- [ ] Filter by completion status

#### Testing
- [ ] Unit tests for CourseService
- [ ] Unit tests for LessonService
- [ ] Integration tests for course endpoints
- [ ] Integration tests for enrollment
- [ ] Component tests for CourseCard
- [ ] Component tests for CourseFilters
- [ ] E2E test for course creation
- [ ] E2E test for enrollment flow

#### Documentation
- [ ] Document API endpoints in Swagger
- [ ] Add code comments for complex logic
- [ ] Create admin user guide for course creation
- [ ] Document YouTube video ID extraction

---

## /speckit.tasks

### Task Breakdown (Estimated 40 hours)

#### Task 1: Repository Layer (4 hours)
**Description:** Create repository interfaces and implementations for courses and lessons
**Subtasks:**
1. Create ICourseRepository with methods (GetAll, GetById, GetByCategory, Add, Update, Delete, GetEnrolled)
2. Implement CourseRepository with EF Core queries
3. Add filtering methods (by category, difficulty, isPremium, search)
4. Implement pagination support
5. Create ILessonRepository with CRUD methods
6. Implement LessonRepository
7. Add method to get lessons by course ID
8. Test repository methods with unit tests

#### Task 2: Service Layer (5 hours)
**Description:** Implement business logic for course and lesson management
**Subtasks:**
1. Create ICourseService interface
2. Implement CourseService.CreateCourseAsync
3. Implement CourseService.UpdateCourseAsync
4. Implement CourseService.DeleteCourseAsync (soft delete)
5. Implement CourseService.GetCoursesAsync with filters
6. Implement CourseService.GetCourseDetailAsync (check user enrollment)
7. Implement CourseService.EnrollUserAsync
8. Implement CourseService.GetUserCoursesAsync
9. Add validation logic
10. Create ILessonService interface
11. Implement LessonService CRUD methods
12. Add YouTube video ID validation

#### Task 3: DTOs & Validation (3 hours)
**Description:** Create data transfer objects and FluentValidation rules
**Subtasks:**
1. Create all DTO classes (Course, Lesson, Enrollment)
2. Create CreateCourseValidator with rules
3. Create UpdateCourseValidator
4. Create CreateLessonValidator
5. Validate YouTube video ID format
6. Test validation rules
7. Configure AutoMapper profiles

#### Task 4: API Controllers (4 hours)
**Description:** Create REST API endpoints for courses and lessons
**Subtasks:**
1. Create CoursesController with dependency injection
2. Implement GET /api/courses with filtering
3. Implement GET /api/courses/{id}
4. Implement POST /api/courses (admin only)
5. Implement PUT /api/courses/{id} (admin only)
6. Implement DELETE /api/courses/{id} (admin only)
7. Implement POST /api/courses/{id}/enroll
8. Implement GET /api/courses/my-courses
9. Create LessonsController
10. Implement lesson CRUD endpoints
11. Add Swagger documentation
12. Test endpoints with Postman

#### Task 5: Frontend Services (3 hours)
**Description:** Create API integration layer for frontend
**Subtasks:**
1. Create courseService.ts with all API methods
2. Implement error handling
3. Create lessonService.ts
4. Create TypeScript interfaces
5. Set up React Query hooks
6. Create useCourses custom hook
7. Create useCourse custom hook

#### Task 6: Course Store (2 hours)
**Description:** Set up Zustand state management for courses
**Subtasks:**
1. Create courseStore.ts
2. Define state interface
3. Implement fetchCourses action
4. Implement fetchCourse action
5. Implement enrollInCourse action
6. Add filters state management
7. Test store actions

#### Task 7: Course Display Components (6 hours)
**Description:** Build course listing and display components
**Subtasks:**
1. Create CoursesPage layout
2. Implement course grid with CourseCard
3. Add loading skeletons
4. Create CourseCard component
5. Add thumbnail, title, description
6. Add badges (difficulty, premium)
7. Add reward points display
8. Style with TailwindCSS
9. Create CourseFilters component
10. Implement filter logic
11. Add pagination component
12. Make responsive for mobile

#### Task 8: Course Detail Page (5 hours)
**Description:** Build detailed course view with lessons
**Subtasks:**
1. Create CourseDetailPage
2. Display course header with metadata
3. Render markdown description
4. Create LessonList component
5. Create LessonCard component
6. Add enrollment button
7. Show enrollment status
8. Display progress indicator
9. Add breadcrumb navigation
10. Make responsive

#### Task 9: Admin Course Editor (8 hours)
**Description:** Build comprehensive course creation/editing interface
**Subtasks:**
1. Create AdminCoursesPage with table
2. Create CourseEditor component
3. Build course basic info form (title, description, category)
4. Add difficulty selector
5. Add premium toggle
6. Add thumbnail URL input with preview
7. Create LessonEditor component
8. Build lesson form (title, description, video ID)
9. Add YouTube video ID extraction logic
10. Implement drag-and-drop lesson reordering
11. Add lesson delete with confirmation
12. Implement course preview
13. Add save draft / publish functionality
14. Handle form validation errors

#### Task 10: Enrollment System (3 hours)
**Description:** Implement course enrollment functionality
**Subtasks:**
1. Create EnrollButton component
2. Add premium status check
3. Show upgrade prompt for premium courses
4. Implement enrollment API call
5. Update UI after enrollment
6. Create MyCoursesPage
7. Display enrolled courses with progress
8. Add filter by completion status

#### Task 11: Testing (5 hours)
**Description:** Comprehensive testing of course management
**Subtasks:**
1. Write unit tests for CourseService (80% coverage)
2. Write unit tests for LessonService
3. Write integration tests for course endpoints
4. Write integration tests for enrollment
5. Component tests for CourseCard
6. Component tests for CourseFilters
7. E2E test for creating a course
8. E2E test for enrollment flow

#### Task 12: Polish & Documentation (2 hours)
**Description:** Final touches and documentation
**Subtasks:**
1. Add loading states throughout
2. Add error handling and user feedback
3. Optimize performance (lazy loading, memoization)
4. Write API documentation
5. Create admin guide for course creation
6. Add code comments

---

## /speckit.implement

### Implementation Code

#### CourseRepository

**File:** `WahadiniCryptoQuest.Infrastructure/Repositories/CourseRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Application.Interfaces;
using WahadiniCryptoQuest.Domain.Entities;
using WahadiniCryptoQuest.Infrastructure.Data;

namespace WahadiniCryptoQuest.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;
    
    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<(List<Course> Courses, int TotalCount)> GetCoursesAsync(
        Guid? categoryId,
        string? difficulty,
        bool? isPremium,
        string? searchQuery,
        int page,
        int pageSize)
    {
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Lessons)
            .Where(c => c.IsPublished)
            .AsQueryable();
        
        // Apply filters
        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }
        
        if (!string.IsNullOrEmpty(difficulty))
        {
            query = query.Where(c => c.DifficultyLevel.ToString() == difficulty);
        }
        
        if (isPremium.HasValue)
        {
            query = query.Where(c => c.IsPremium == isPremium.Value);
        }
        
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(c => 
                c.Title.Contains(searchQuery) || 
                c.Description.Contains(searchQuery));
        }
        
        var totalCount = await query.CountAsync();
        
        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (courses, totalCount);
    }
    
    public async Task<Course?> GetByIdAsync(Guid id, bool includeDetails = false)
    {
        var query = _context.Courses.AsQueryable();
        
        if (includeDetails)
        {
            query = query
                .Include(c => c.Category)
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Enrollments);
        }
        
        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<List<Course>> GetUserEnrolledCoursesAsync(Guid userId)
    {
        return await _context.UserCourseEnrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Where(e => e.UserId == userId)
            .Select(e => e.Course)
            .ToListAsync();
    }
    
    public async Task<Course> AddAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }
    
    public async Task UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var course = await GetByIdAsync(id);
        if (course != null)
        {
            // Soft delete
            course.IsPublished = false;
            await UpdateAsync(course);
        }
    }
    
    public async Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId)
    {
        return await _context.UserCourseEnrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
    }
}
```

#### CourseService

**File:** `WahadiniCryptoQuest.Application/Services/CourseService.cs`

```csharp
using AutoMapper;
using WahadiniCryptoQuest.Application.DTOs;
using WahadiniCryptoQuest.Application.Interfaces;
using WahadiniCryptoQuest.Domain.Entities;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    
    public CourseService(
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    public async Task<(List<CourseDto> Courses, int TotalCount)> GetCoursesAsync(
        Guid? categoryId,
        string? difficulty,
        bool? isPremium,
        string? searchQuery,
        int page,
        int pageSize)
    {
        var (courses, totalCount) = await _courseRepository.GetCoursesAsync(
            categoryId, difficulty, isPremium, searchQuery, page, pageSize);
        
        var courseDtos = _mapper.Map<List<CourseDto>>(courses);
        return (courseDtos, totalCount);
    }
    
    public async Task<CourseDetailDto?> GetCourseDetailAsync(Guid courseId, Guid? userId = null)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, includeDetails: true);
        
        if (course == null)
        {
            return null;
        }
        
        var courseDto = _mapper.Map<CourseDetailDto>(course);
        
        if (userId.HasValue)
        {
            courseDto.IsEnrolled = await _courseRepository.IsUserEnrolledAsync(userId.Value, courseId);
            // Calculate user progress if enrolled
            if (courseDto.IsEnrolled)
            {
                // Progress calculation logic here
                courseDto.UserProgress = await CalculateUserProgressAsync(userId.Value, courseId);
            }
        }
        
        return courseDto;
    }
    
    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto, Guid createdByUserId)
    {
        var course = _mapper.Map<Course>(dto);
        course.CreatedByUserId = createdByUserId;
        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        course.IsPublished = false; // Default to draft
        
        var createdCourse = await _courseRepository.AddAsync(course);
        return _mapper.Map<CourseDto>(createdCourse);
    }
    
    public async Task<CourseDto?> UpdateCourseAsync(Guid courseId, UpdateCourseDto dto)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        
        if (course == null)
        {
            return null;
        }
        
        _mapper.Map(dto, course);
        await _courseRepository.UpdateAsync(course);
        
        return _mapper.Map<CourseDto>(course);
    }
    
    public async Task<bool> DeleteCourseAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        
        if (course == null)
        {
            return false;
        }
        
        await _courseRepository.DeleteAsync(courseId);
        return true;
    }
    
    public async Task<EnrollmentDto?> EnrollUserAsync(Guid userId, Guid courseId)
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }
        
        // Check if course exists
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return null;
        }
        
        // Check if course is premium and user has access
        if (course.IsPremium && user.SubscriptionTier == "Free")
        {
            throw new UnauthorizedAccessException("Premium subscription required");
        }
        
        // Check if already enrolled
        if (await _courseRepository.IsUserEnrolledAsync(userId, courseId))
        {
            throw new InvalidOperationException("User already enrolled in this course");
        }
        
        // Create enrollment
        var enrollment = new UserCourseEnrollment
        {
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };
        
        // Save enrollment (implement in repository)
        // await _courseRepository.AddEnrollmentAsync(enrollment);
        
        return _mapper.Map<EnrollmentDto>(enrollment);
    }
    
    public async Task<List<EnrolledCourseDto>> GetUserCoursesAsync(Guid userId)
    {
        var courses = await _courseRepository.GetUserEnrolledCoursesAsync(userId);
        return _mapper.Map<List<EnrolledCourseDto>>(courses);
    }
    
    private async Task<decimal> CalculateUserProgressAsync(Guid userId, Guid courseId)
    {
        // Calculate progress based on completed lessons
        // Implementation depends on UserProgress entity
        return 0; // Placeholder
    }
}
```

#### CoursesController

**File:** `WahadiniCryptoQuest.API/Controllers/CoursesController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WahadiniCryptoQuest.Application.DTOs;
using WahadiniCryptoQuest.Application.Interfaces;

namespace WahadiniCryptoQuest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;
    
    public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? difficulty,
        [FromQuery] bool? isPremium,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var (courses, totalCount) = await _courseService.GetCoursesAsync(
            categoryId, difficulty, isPremium, search, page, pageSize);
        
        return Ok(new
        {
            data = courses,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(Guid id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid? userGuid = userId != null ? Guid.Parse(userId) : null;
        
        var course = await _courseService.GetCourseDetailAsync(id, userGuid);
        
        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }
        
        return Ok(course);
    }
    
    [Authorize(Policy = "RequireAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var course = await _courseService.CreateCourseAsync(dto, userId);
        
        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
    }
    
    [Authorize(Policy = "RequireAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var course = await _courseService.UpdateCourseAsync(id, dto);
        
        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }
        
        return Ok(course);
    }
    
    [Authorize(Policy = "RequireAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        var success = await _courseService.DeleteCourseAsync(id);
        
        if (!success)
        {
            return NotFound(new { message = "Course not found" });
        }
        
        return NoContent();
    }
    
    [Authorize]
    [HttpPost("{id}/enroll")]
    public async Task<IActionResult> EnrollInCourse(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        
        try
        {
            var enrollment = await _courseService.EnrollUserAsync(userId, id);
            
            if (enrollment == null)
            {
                return NotFound(new { message = "Course not found" });
            }
            
            return Ok(enrollment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize]
    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var courses = await _courseService.GetUserCoursesAsync(userId);
        
        return Ok(courses);
    }
}
```

#### Frontend courseService

**File:** `frontend/src/services/courseService.ts`

```typescript
import api from './api';
import { Course, CourseDetail, CreateCourseDto, PaginatedResponse } from '../types/course.types';

export const courseService = {
  async getCourses(filters: {
    categoryId?: string;
    difficulty?: string;
    isPremium?: boolean;
    search?: string;
    page?: number;
    pageSize?: number;
  }): Promise<PaginatedResponse<Course>> {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined) {
        params.append(key, value.toString());
      }
    });
    
    const response = await api.get(`/courses?${params.toString()}`);
    return response.data;
  },
  
  async getCourse(id: string): Promise<CourseDetail> {
    const response = await api.get(`/courses/${id}`);
    return response.data;
  },
  
  async createCourse(data: CreateCourseDto): Promise<Course> {
    const response = await api.post('/courses', data);
    return response.data;
  },
  
  async updateCourse(id: string, data: Partial<CreateCourseDto>): Promise<Course> {
    const response = await api.put(`/courses/${id}`, data);
    return response.data;
  },
  
  async deleteCourse(id: string): Promise<void> {
    await api.delete(`/courses/${id}`);
  },
  
  async enrollInCourse(courseId: string): Promise<void> {
    await api.post(`/courses/${courseId}/enroll`);
  },
  
  async getMyCourses(): Promise<Course[]> {
    const response = await api.get('/courses/my-courses');
    return response.data;
  },
};
```

#### Frontend CoursesPage

**File:** `frontend/src/pages/CoursesPage.tsx`

```typescript
import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { courseService } from '../services/courseService';
import CourseCard from '../components/course/CourseCard';
import CourseFilters from '../components/course/CourseFilters';
import { Loader2 } from 'lucide-react';

export const CoursesPage: React.FC = () => {
  const [filters, setFilters] = useState({
    categoryId: undefined,
    difficulty: undefined,
    isPremium: undefined,
    search: '',
    page: 1,
    pageSize: 12,
  });
  
  const { data, isLoading, error } = useQuery({
    queryKey: ['courses', filters],
    queryFn: () => courseService.getCourses(filters),
  });
  
  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-primary-600" />
      </div>
    );
  }
  
  if (error) {
    return (
      <div className="text-center text-red-600 p-8">
        Error loading courses. Please try again.
      </div>
    );
  }
  
  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <h1 className="text-4xl font-bold mb-8">Explore Courses</h1>
      
      <CourseFilters filters={filters} onChange={setFilters} />
      
      {data?.data.length === 0 ? (
        <div className="text-center text-gray-500 py-12">
          No courses found matching your criteria.
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
            {data?.data.map((course) => (
              <CourseCard key={course.id} course={course} />
            ))}
          </div>
          
          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex justify-center gap-2">
              {Array.from({ length: data.totalPages }, (_, i) => i + 1).map((page) => (
                <button
                  key={page}
                  onClick={() => setFilters((prev) => ({ ...prev, page }))}
                  className={`px-4 py-2 rounded ${
                    filters.page === page
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                  }`}
                >
                  {page}
                </button>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default CoursesPage;
```

### Notes
- Implement AutoMapper profiles for DTO mappings
- Add comprehensive error handling
- Implement caching for frequently accessed courses
- Add course analytics tracking
- Consider implementing course recommendations in future
- Test YouTube video ID validation thoroughly
- Optimize database queries with proper indexes
