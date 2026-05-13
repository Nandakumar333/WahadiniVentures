# WahadiniCryptoQuest Platform - SpecKit Implementation Prompts

Based on the comprehensive crypto learning platform documentation, here are the SpecKit commands to transform the project into actionable implementation.

## Core SpecKit Commands

### 1. /speckit.constitution

```
Create a project constitution for WahadiniCryptoQuest, a gamified crypto education platform. Define the core principles, values, and constraints that will guide all development decisions.

**Context**: This is a crypto education platform with video-based learning, task verification system, reward points, premium subscriptions, and admin dashboard.

**Key Areas to Address**:
- User Experience Principles (learning-first, gamification balance)
- Technical Principles (scalability, security, maintainability)
- Business Principles (freemium model, fair reward distribution)
- Security & Privacy Standards (crypto industry standards)
- Content Quality Standards (educational value, accuracy)
- Platform Values (accessibility, fairness, transparency)

**Consider**:
- Free vs Premium content balance
- Point economy fairness
- Task verification integrity
- Educational effectiveness
- Community building
- Security best practices for crypto education
- Scalability requirements
- Mobile-first approach
```

### 2. /speckit.specify

```
Create a comprehensive baseline specification for WahadiniCryptoQuest platform based on the provided documentation.

**Platform Overview**: 
Gamified crypto education platform with YouTube video integration, multi-type task verification system, reward points economy, premium subscriptions with discount redemption, and comprehensive admin dashboard.

**Core Features to Specify**:

1. **Authentication & User Management**
   - JWT-based auth with refresh tokens
   - Role-based access (Free, Premium, Admin)
   - User profiles with progress tracking

2. **Course & Lesson System**
   - Category-based course organization
   - YouTube video integration with progress tracking
   - Resume functionality
   - Premium content gating

3. **Task Verification System**
   - 5 task types: Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification
   - Auto-approval vs manual review workflows
   - Admin review interface

4. **Reward & Gamification System**
   - Point earning mechanisms (lessons, tasks, streaks, referrals)
   - Discount code redemption system
   - Leaderboards and achievements
   - Point transaction ledger

5. **Subscription & Payment System**
   - Stripe integration (Free/Monthly/Yearly tiers)
   - Discount application from reward points
   - Subscription management

6. **Admin Dashboard**
   - Content management (courses, lessons, tasks)
   - User management and analytics
   - Task review system
   - Revenue and engagement analytics

**Technical Stack**:
- Frontend: React 18 + TypeScript + Vite + TailwindCSS
- Backend: .NET 8 Web API + Clean Architecture
- Database: PostgreSQL
- Authentication: JWT + ASP.NET Identity
- Payments: Stripe
- Video: YouTube API via react-player

**Success Criteria**:
- Support 1000+ concurrent users
- 99.9% uptime
- Mobile-responsive design
- Secure payment processing
- Real-time progress tracking
```

### 3. /speckit.plan

```
Create a detailed implementation plan for WahadiniCryptoQuest platform, breaking down the specification into phases and implementation streams.

**Planning Constraints**:
- Target: MVP in 8-12 weeks
- Team: 1-2 developers (full-stack capability)
- Budget: Free/low-cost services initially
- Quality: Production-ready, scalable architecture

**Phase Structure**:

**Phase 1: Foundation (Weeks 1-3)**
- Database schema and migrations
- Backend API structure with authentication
- Basic frontend setup with routing
- User registration/login flows

**Phase 2: Core Learning System (Weeks 4-6)**
- Course and lesson management
- YouTube video player integration
- Progress tracking system
- Basic task system (Quiz type first)

**Phase 3: Gamification & Rewards (Weeks 7-8)**
- Reward points system
- Task submission and review
- Basic admin dashboard
- Leaderboards

**Phase 4: Premium Features (Weeks 9-10)**
- Stripe payment integration
- Subscription management
- Premium content gating
- Discount redemption system

**Phase 5: Advanced Features & Polish (Weeks 11-12)**
- Advanced task types
- Comprehensive admin features
- Analytics dashboard
- Performance optimization

**Implementation Streams**:
1. Backend API Development
2. Frontend Component Development
3. Database Design & Migration
4. Integration & Testing
5. DevOps & Deployment

**Dependencies**:
- Stripe account setup
- YouTube API quotas
- Database hosting (PostgreSQL)
- Email service configuration
- Domain and SSL certificates

**Risk Mitigation**:
- Start with free tiers of all services
- Implement core learning loop first
- Progressive enhancement approach
- Comprehensive error handling
```

### 4. /speckit.tasks

```
Generate detailed, actionable tasks for implementing WahadiniCryptoQuest platform based on the implementation plan.

**Task Categories**:

**A. Backend Development Tasks**
- Set up .NET 8 solution structure (API, Domain, Application, Infrastructure)
- Implement Entity Framework models and DbContext
- Create repository pattern implementation
- Build JWT authentication system
- Develop course and lesson management APIs
- Implement task submission and review system
- Create reward points calculation engine
- Integrate Stripe payment webhooks
- Build admin analytics endpoints

**B. Frontend Development Tasks**
- Set up React + Vite + TypeScript project
- Create authentication store and components
- Build course browsing and enrollment UI
- Implement YouTube video player with progress tracking
- Create task submission forms (dynamic based on type)
- Build reward points and leaderboard components
- Develop admin dashboard with content management
- Implement subscription and payment flows

**C. Database Tasks**
- Design and create PostgreSQL schema
- Set up database migrations
- Create seed data for categories and initial content
- Implement database indexes for performance
- Set up backup and recovery procedures

**D. Integration Tasks**
- Connect frontend to backend APIs
- Implement YouTube video URL validation
- Set up Stripe webhook handling
- Configure email notification system
- Create file upload handling for screenshots

**E. Testing & Quality Assurance Tasks**
- Write unit tests for critical business logic
- Implement integration tests for API endpoints
- Create end-to-end tests for user journeys
- Perform security testing (authentication, authorization)
- Conduct performance testing with sample data

**F. DevOps & Deployment Tasks**
- Set up CI/CD pipeline
- Configure production environment variables
- Deploy to cloud hosting (Railway/Render/Vercel)
- Set up monitoring and logging
- Configure SSL and domain

**Task Prioritization**:
1. High Priority: Core learning functionality
2. Medium Priority: Gamification and rewards
3. Lower Priority: Advanced admin features and analytics

**Acceptance Criteria for Each Task**:
- Clear definition of done
- Testing requirements
- Performance benchmarks
- Security considerations
- Documentation needs
```

### 5. /speckit.implement

```
Execute the implementation of WahadiniCryptoQuest platform using the specified tasks and plan.

**Implementation Focus**:
Start with Phase 1 tasks and progress through the implementation plan systematically.

**Current Implementation Priority**:

**1. Backend Foundation**
- Create .NET 8 solution with Clean Architecture
- Implement database schema with Entity Framework
- Set up JWT authentication with ASP.NET Identity
- Create basic CRUD operations for Users, Courses, Lessons

**2. Frontend Foundation**
- Set up React project with TypeScript and Vite
- Implement authentication store and protected routes
- Create basic layout components (Header, Navigation, Footer)
- Build login and registration forms

**3. Core Learning Loop**
- Implement course browsing and enrollment
- Create lesson viewing with YouTube integration
- Add progress tracking and lesson completion
- Build basic task submission (Quiz type first)

**Implementation Guidelines**:
- Follow the prompts provided in the original documentation
- Use the specific GitHub Copilot Edits prompts (Prompts 1-12)
- Implement error handling and loading states
- Ensure responsive design from the start
- Add comprehensive logging
- Include input validation and sanitization

**Quality Gates**:
- Each feature must be tested before moving to next
- Database migrations must be reversible
- API endpoints must include proper error responses
- Frontend components must handle loading and error states
- Security measures must be implemented from day one

**Deployment Strategy**:
- Use free tiers initially (Supabase, Vercel, Railway)
- Implement environment-based configuration
- Set up automated deployments
- Configure monitoring and error tracking

**Success Metrics**:
- User can register, login, and browse courses
- Video playback works with progress saving
- Task submission and approval workflow functions
- Reward points are correctly calculated and displayed
- Admin can create and manage content
- Payment flow works end-to-end (test mode)
```

## Enhancement Commands (Optional)

### /speckit.clarify

```
Identify and clarify ambiguous areas in the WahadiniCryptoQuest specification before implementation.

**Areas to Clarify**:

1. **Task Verification Workflows**
   - How should auto-approval thresholds be configurable?
   - What's the escalation process for disputed task reviews?
   - How to handle bulk task approvals efficiently?

2. **Reward Point Economy**
   - What prevents point inflation or gaming the system?
   - How are bonus calculations handled for edge cases?
   - What's the redemption rate limits to prevent abuse?

3. **Content Scaling Strategy**
   - How will course creation scale beyond manual admin input?
   - What's the content moderation process for user-generated elements?
   - How to handle multiple instructors or content creators?

4. **Premium Feature Boundaries**
   - Exactly which features require premium vs free access?
   - How is the trial experience structured?
   - What happens to progress when subscriptions expire?

5. **Technical Scalability Questions**
   - What are the expected concurrent user limits?
   - How should video streaming be optimized?
   - What's the plan for database partitioning as data grows?

Ask structured questions to de-risk these areas before detailed planning.
```

### /speckit.analyze

```
Perform cross-artifact consistency and alignment analysis for WahadiniCryptoQuest specification.

**Analysis Areas**:

1. **Specification-to-Plan Alignment**
   - Verify all specified features are included in implementation plan
   - Check that technical stack choices support all requirements
   - Ensure timeline is realistic for scope

2. **Architecture Consistency**
   - Validate that frontend and backend designs align
   - Check database schema supports all user stories
   - Verify API design matches frontend needs

3. **Security Alignment**
   - Ensure authentication flows are consistent across components
   - Verify payment security measures are comprehensive
   - Check that admin access controls are properly designed

4. **User Experience Coherence**
   - Validate that gamification elements enhance rather than distract
   - Check that premium upgrade flows are clear and compelling
   - Ensure mobile experience is consistently designed

5. **Business Logic Consistency**
   - Verify reward calculations are consistently defined
   - Check that subscription tiers have clear value propositions
   - Ensure task types support the learning objectives

Generate a report highlighting any inconsistencies or gaps that need resolution.
```

### /speckit.checklist

```
Generate comprehensive quality checklists for WahadiniCryptoQuest platform requirements.

**Requirements Completeness Checklist**:
- [ ] All user roles and permissions clearly defined
- [ ] Complete user journey flows documented
- [ ] All API endpoints specified with request/response formats
- [ ] Database relationships and constraints documented
- [ ] Error handling scenarios covered
- [ ] Performance requirements quantified
- [ ] Security requirements detailed
- [ ] Integration points clearly defined

**Clarity Checklist**:
- [ ] Technical terms consistently used throughout
- [ ] User interface behaviors explicitly described
- [ ] Business rules unambiguously stated
- [ ] Edge cases and error conditions covered
- [ ] Acceptance criteria measurable and testable
- [ ] Dependencies clearly identified
- [ ] Assumptions explicitly stated

**Consistency Checklist**:
- [ ] Naming conventions consistent across documents
- [ ] Data types and formats standardized
- [ ] User interface patterns consistent
- [ ] API design patterns consistent
- [ ] Security approaches uniform
- [ ] Error handling patterns consistent
- [ ] Documentation style uniform

**Implementation Readiness Checklist**:
- [ ] All technical decisions documented
- [ ] Development environment requirements specified
- [ ] Testing strategy defined
- [ ] Deployment process outlined
- [ ] Monitoring and maintenance plans included
- [ ] Team responsibilities clarified
- [ ] Timeline and milestones realistic

Use these checklists to validate specification quality before implementation begins.
```

## Usage Instructions

1. **Go to the project folder**: `cd WahadiniCryptoQuest`
2. **Start using slash commands with your AI agent**:
   - **2.1** `/speckit.constitution` - Establish project principles
   - **2.2** `/speckit.specify` - Create baseline specification
   - **2.3** `/speckit.plan` - Create implementation plan
   - **2.4** `/speckit.tasks` - Generate actionable tasks
   - **2.5** `/speckit.implement` - Execute implementation

### Enhancement Commands (Optional)
- **○** `/speckit.clarify` (optional) - Ask structured questions to de-risk ambiguous areas before planning (run before `/speckit.plan` if used)
- **○** `/speckit.analyze` (optional) - Cross-artifact consistency & alignment report (after `/speckit.tasks`, before `/speckit.implement`)
- **○** `/speckit.checklist` (optional) - Generate quality checklists to validate requirements completeness, clarity, and consistency (after `/speckit.plan`)

## Implementation Notes

These prompts will systematically transform your comprehensive crypto learning platform documentation into a well-structured, implementable project using SpecKit's methodology. Each command builds upon the previous one to ensure a cohesive development approach.

**Key Benefits**:
- Structured approach to complex platform development
- Clear phase-based implementation strategy
- Built-in quality assurance checkpoints
- Risk mitigation through systematic planning
- Actionable tasks with defined acceptance criteria