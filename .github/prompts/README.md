# WahadiniCryptoQuest Prompts Directory

This directory contains all prompts for the WahadiniCryptoQuest project, including SpecKit command prompts and architecture/clean code standards.

## 📋 Table of Contents

- [Architecture & Standards](#architecture--standards)
- [SpecKit Commands](#speckit-commands)
- [Quick Start](#quick-start)
- [Integration](#integration)
- [Resources](#resources)

## 🏗️ Architecture & Standards

### Core Documentation

| File | Purpose | When to Use |
|------|---------|-------------|
| **[architecture.prompt.md](./architecture.prompt.md)** | Comprehensive architecture & clean code standards | Reference for all implementation work |
| **[backend.prompt.md](./backend.prompt.md)** | Backend-specific implementation patterns (.NET 8 C#) | Backend development (entities, services, controllers) |
| **[frontend.prompt.md](./frontend.prompt.md)** | Frontend-specific implementation patterns (React 18) | Frontend development (components, hooks, state) |
| **[database.prompt.md](./database.prompt.md)** | Database design, migrations, EF Core patterns | Database schema, migrations, queries |
| **[testing.prompt.md](./testing.prompt.md)** | Testing strategies (unit, integration, E2E) | Writing tests for all layers |
| **[security.prompt.md](./security.prompt.md)** | Security best practices (auth, validation, protection) | Implementing security features |
| **[ci-cd.prompt.md](./ci-cd.prompt.md)** | CI/CD pipeline configuration | Setting up automated workflows |
| **[deployment.prompt.md](./deployment.prompt.md)** | Build and deployment procedures | Deploying to production |
| **[ARCHITECTURE_QUICK_REFERENCE.md](./ARCHITECTURE_QUICK_REFERENCE.md)** | Quick reference guide with patterns and examples | Quick lookups during development |
| **[ARCHITECTURE_INTEGRATION.md](./ARCHITECTURE_INTEGRATION.md)** | How architecture is integrated with SpecKit | Understanding the system integration |
| **[BACKEND_FRONTEND_INTEGRATION.md](./BACKEND_FRONTEND_INTEGRATION.md)** | Backend and frontend prompt integration details | Understanding layer-specific patterns |

### What's Inside architecture.prompt.md

The architecture document contains:
- ✅ Complete technology stack (Backend: .NET 8, Frontend: React 18, DB: PostgreSQL)
- ✅ Clean Architecture layers (Domain, Application, Infrastructure, Presentation)
- ✅ Project structure for backend and frontend
- ✅ Naming conventions (C#, TypeScript)
- ✅ SOLID principles with examples
- ✅ Design patterns (Repository, Unit of Work, CQRS, Result)
- ✅ Code examples for all layers
- ✅ Error handling standards
- ✅ Security best practices
- ✅ Performance optimization
- ✅ Testing standards

### What's Inside backend.prompt.md

The backend document contains:
- ✅ .NET 8 C# specific patterns and examples
- ✅ Domain entity implementation with factory methods
- ✅ Repository and Unit of Work patterns
- ✅ Service layer with Result pattern
- ✅ API controllers with authorization
- ✅ Entity Framework Core configurations
- ✅ JWT authentication implementation
- ✅ External service integration (Stripe, YouTube)
- ✅ CQRS with MediatR
- ✅ Validation with FluentValidation
- ✅ Complete code examples for WahadiniCryptoQuest entities

### What's Inside frontend.prompt.md

The frontend document contains:
- ✅ React 18 + TypeScript specific patterns
- ✅ Component-based organization (not feature-based)
- ✅ Component patterns with hooks
- ✅ State management (Zustand + React Query)
- ✅ Form handling (React Hook Form + Zod)
- ✅ Routing with React Router
- ✅ UI component integration (shadcn/ui)
- ✅ API integration patterns
- ✅ Error boundaries
- ✅ Performance optimization techniques
- ✅ Complete code examples for crypto education features

### What's Inside database.prompt.md

The database document contains:
- ✅ PostgreSQL database design patterns
- ✅ Entity Framework Core 8.0 configurations
- ✅ Migration strategies and best practices
- ✅ Indexing and performance optimization
- ✅ Time-based partitioning for user activity
- ✅ Soft delete patterns
- ✅ Audit fields (CreatedAt, UpdatedAt)
- ✅ Query optimization techniques
- ✅ Transaction management
- ✅ Database seeding and test data

### What's Inside testing.prompt.md

The testing document contains:
- ✅ Unit testing strategies (xUnit for backend, Jest for frontend)
- ✅ Integration testing patterns
- ✅ E2E testing with proper tools
- ✅ Test organization and structure
- ✅ Mocking and test doubles
- ✅ Test data management
- ✅ Coverage requirements
- ✅ Testing best practices
- ✅ TDD approach when applicable
- ✅ Component testing (React Testing Library)

### What's Inside security.prompt.md

The security document contains:
- ✅ JWT authentication implementation
- ✅ Role-based authorization (Free, Premium, Admin)
- ✅ Input validation and sanitization
- ✅ SQL injection prevention
- ✅ XSS and CSRF protection
- ✅ Password hashing (BCrypt)
- ✅ Secure token storage
- ✅ HTTPS enforcement
- ✅ CORS configuration
- ✅ Rate limiting
- ✅ Security audit logging
- ✅ OWASP Top 10 compliance

### What's Inside ci-cd.prompt.md

The CI/CD document contains:
- ✅ GitHub Actions workflow configuration
- ✅ Automated testing pipeline
- ✅ Build and deployment automation
- ✅ Environment management (dev, staging, prod)
- ✅ Docker container builds
- ✅ Database migration automation
- ✅ Code quality checks (linting, formatting)
- ✅ Security scanning
- ✅ Deployment strategies (blue-green, canary)
- ✅ Rollback procedures

### What's Inside deployment.prompt.md

The deployment document contains:
- ✅ Build process for backend and frontend
- ✅ Docker containerization
- ✅ Environment configuration
- ✅ Production deployment steps
- ✅ Database migration in production
- ✅ Zero-downtime deployment
- ✅ Health checks and monitoring
- ✅ Logging configuration
- ✅ Performance monitoring setup
- ✅ Backup and recovery procedures

## 🔧 SpecKit Commands

### Workflow Commands

| Command | File | Description |
|---------|------|-------------|
| **/speckit.constitution** | [speckit.constitution.prompt.md](./speckit.constitution.prompt.md) | Create/update project constitution |
| **/speckit.specify** | [speckit.specify.prompt.md](./speckit.specify.prompt.md) | Create feature specification |
| **/speckit.plan** | [speckit.plan.prompt.md](./speckit.plan.prompt.md) | Generate implementation plan |
| **/speckit.tasks** | [speckit.tasks.prompt.md](./speckit.tasks.prompt.md) | Generate actionable tasks |
| **/speckit.implement** | [speckit.implement.prompt.md](./speckit.implement.prompt.md) | Execute implementation |

### Enhancement Commands

| Command | File | Description |
|---------|------|-------------|
| **/speckit.clarify** | [speckit.clarify.prompt.md](./speckit.clarify.prompt.md) | Clarify ambiguous requirements |
| **/speckit.analyze** | [speckit.analyze.prompt.md](./speckit.analyze.prompt.md) | Cross-artifact consistency analysis |
| **/speckit.checklist** | [speckit.checklist.prompt.md](./speckit.checklist.prompt.md) | Generate quality checklists |

## 🚀 Quick Start

### For New Features

1. **Specify** the feature:
   ```
   /speckit.specify "Add user authentication with JWT tokens"
   ```
   - Automatically considers domain entity patterns
   - References architecture standards

2. **Plan** the implementation:
   ```
   /speckit.plan
   ```
   - Applies Clean Architecture layers
   - Generates data models following domain patterns
   - Creates API contracts with proper authorization

3. **Generate tasks**:
   ```
   /speckit.tasks
   ```
   - Creates layered tasks (Domain → Application → Infrastructure → Presentation)
   - Follows project structure

4. **Implement** the feature:
   ```
   /speckit.implement
   ```
   - Follows architecture standards
   - Applies code conventions
   - Uses proper patterns

### For Quick Reference

Need a quick example? Check [ARCHITECTURE_QUICK_REFERENCE.md](./ARCHITECTURE_QUICK_REFERENCE.md) for:
- Project structure diagrams
- Naming convention summaries
- Common code patterns
- Technology stack reference
- Command cheat sheets

## 🔗 Integration

All SpecKit commands are integrated with architecture standards:

```
┌─────────────────────────────────────────────────────┐
│         architecture.prompt.md                      │
│  (Clean Architecture & Code Standards)              │
└─────────────────┬───────────────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
        ▼                   ▼
┌───────────────┐   ┌────────────────┐
│ SpecKit       │   │ SpecKit        │
│ Workflow      │   │ Enhancement    │
│ Commands      │   │ Commands       │
└───────────────┘   └────────────────┘
  │                   │
  ├─ specify          ├─ clarify
  ├─ plan             ├─ analyze
  ├─ tasks            └─ checklist
  └─ implement
```

### How It Works

1. **During Specification** (`/speckit.specify`):
   - Considers domain entity patterns
   - Thinks about Clean Architecture layers

2. **During Planning** (`/speckit.plan`):
   - Applies architectural patterns
   - Designs proper domain models
   - Creates RESTful API contracts

3. **During Task Generation** (`/speckit.tasks`):
   - Organizes tasks by layer (Domain → Application → Infrastructure → Presentation)
   - Creates setup tasks for project structure

4. **During Implementation** (`/speckit.implement`):
   - Follows naming conventions
   - Uses design patterns
   - Applies security best practices
   - Implements proper error handling

## 📚 Resources

### Primary Documents

1. **[architecture.prompt.md](./architecture.prompt.md)** - Your primary reference for all architecture decisions
2. **[ARCHITECTURE_QUICK_REFERENCE.md](./ARCHITECTURE_QUICK_REFERENCE.md)** - Quick lookups and patterns
3. **[ARCHITECTURE_INTEGRATION.md](./ARCHITECTURE_INTEGRATION.md)** - How everything connects

### Sample Prompts

Check `../../sample_prompt/` for additional prompt templates and examples.

### Project Structure

```
.github/prompts/
├── README.md                           ← You are here
├── architecture.prompt.md              ← Main architecture document
├── ARCHITECTURE_QUICK_REFERENCE.md     ← Quick reference
├── ARCHITECTURE_INTEGRATION.md         ← Integration details
├── speckit.constitution.prompt.md      ← Project principles
├── speckit.specify.prompt.md           ← Feature specification
├── speckit.plan.prompt.md              ← Implementation planning
├── speckit.tasks.prompt.md             ← Task generation
├── speckit.implement.prompt.md         ← Code implementation
├── speckit.clarify.prompt.md           ← Requirement clarification
├── speckit.analyze.prompt.md           ← Consistency analysis
└── speckit.checklist.prompt.md         ← Quality checklists
```

## 💡 Best Practices

### When Writing Code

1. **Always** reference [architecture.prompt.md](./architecture.prompt.md) for patterns
2. **Follow** Clean Architecture layers strictly
3. **Use** proper naming conventions (C#: PascalCase, TypeScript: camelCase)
4. **Apply** SOLID principles
5. **Implement** proper error handling (Result pattern)
6. **Write** tests for all layers
7. **Document** public APIs with XML comments

### When Using SpecKit

1. **Start** with a clear feature description in `/speckit.specify`
2. **Review** the generated spec before planning
3. **Check** the plan aligns with architecture standards
4. **Validate** tasks follow the proper layer order
5. **Monitor** implementation for code quality
6. **Test** each phase before moving to the next

### Code Review Checklist

Use the checklist from [architecture.prompt.md](./architecture.prompt.md):

**Backend**:
- ✅ Clean Architecture layers maintained
- ✅ Naming conventions followed
- ✅ Domain entities use factory methods
- ✅ Services use Result pattern
- ✅ Proper error handling
- ✅ Unit tests included

**Frontend**:
- ✅ Feature-based organization
- ✅ TypeScript typing complete
- ✅ Custom hooks for logic
- ✅ Error boundaries present
- ✅ Loading states handled
- ✅ Component tests included

## 🤝 Contributing

When updating architecture standards:

1. Update [architecture.prompt.md](./architecture.prompt.md)
2. Update [ARCHITECTURE_QUICK_REFERENCE.md](./ARCHITECTURE_QUICK_REFERENCE.md) if needed
3. Document changes in [ARCHITECTURE_INTEGRATION.md](./ARCHITECTURE_INTEGRATION.md)
4. Update affected SpecKit prompts if necessary
5. Test with actual feature implementation

## 📞 Support

- **Architecture Questions**: Review [architecture.prompt.md](./architecture.prompt.md)
- **Quick Patterns**: Check [ARCHITECTURE_QUICK_REFERENCE.md](./ARCHITECTURE_QUICK_REFERENCE.md)
- **Integration Issues**: See [ARCHITECTURE_INTEGRATION.md](./ARCHITECTURE_INTEGRATION.md)
- **SpecKit Usage**: Read individual command prompt files

## 🎯 Summary

This directory provides:
- ✅ Comprehensive architecture and clean code standards
- ✅ Integrated SpecKit commands that follow these standards
- ✅ Quick reference guides for daily use
- ✅ Clear integration documentation
- ✅ Consistent code quality across the project

All SpecKit commands automatically apply architecture standards, ensuring consistent, high-quality code generation throughout the WahadiniCryptoQuest project.

---

**Quick Tip**: Bookmark [ARCHITECTURE_QUICK_REFERENCE.md](./ARCHITECTURE_QUICK_REFERENCE.md) for daily development work!
