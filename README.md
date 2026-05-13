# 🎓 WahadiniCryptoQuest

> A gamified cryptocurrency education platform with task-based learning, reward points, and premium subscription tiers.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![PostgreSQL 15](https://img.shields.io/badge/PostgreSQL-15-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![Vite](https://img.shields.io/badge/Vite-7-646CFF?logo=vite)](https://vitejs.dev/)
[![TailwindCSS](https://img.shields.io/badge/TailwindCSS-3.4-06B6D4?logo=tailwindcss)](https://tailwindcss.com/)
[![Stripe](https://img.shields.io/badge/Stripe-Payments-635BFF?logo=stripe)](https://stripe.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
- [Environment Variables](#-environment-variables)
- [API Reference](#-api-reference)
- [Database Schema](#-database-schema)
- [Frontend Pages](#-frontend-pages)
- [Testing](#-testing)
- [CI/CD](#-cicd)
- [Docker Deployment](#-docker-deployment)
- [Documentation](#-documentation)
- [Contributing](#-contributing)
- [License](#-license)

---

## Overview

**WahadiniCryptoQuest** is a full-stack web application that teaches cryptocurrency concepts through video-based courses (YouTube embedded), interactive tasks, and a reward-based gamification system. Users earn points by completing lessons and tasks, redeem points for discount codes, and optionally upgrade to premium subscriptions via Stripe for access to exclusive content.

### Key Highlights

- **Multi-category learning paths** — Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies, and more
- **YouTube video lessons** with automatic progress tracking and resume functionality
- **5 task types** — Quiz, ExternalLink, WalletVerification, Screenshot, TextSubmission
- **Points & rewards economy** — earn, redeem, and compete on leaderboards
- **Multi-currency Stripe subscriptions** — USD, EUR, GBP, INR, JPY
- **Role-based access control** — Free users, Premium subscribers, Admins (SuperAdmin, ContentCreator, Moderator)
- **Comprehensive admin dashboard** — user management, course editor, task review, analytics, discount management

---

## ✨ Features

### For Learners
| Feature | Description |
|---------|-------------|
| 🎥 **Video Courses** | YouTube-embedded lessons with auto-save progress every 10s and resume from last position |
| ✅ **Interactive Tasks** | Quiz, screenshot upload, text submission, external link verification, wallet verification |
| 🏆 **Reward Points** | Earn points for completing lessons (80% threshold), tasks, courses (20% bonus), daily streaks, and referrals |
| 💎 **Premium Access** | Unlock premium courses and unlimited task submissions with Monthly or Yearly plans |
| 🎯 **Achievements & Badges** | Unlock milestones like First Steps, Task Master, Course Conqueror, Knowledge Seeker |
| 🔥 **Daily Streaks** | Consecutive login bonuses with milestone rewards at 5, 10, 30, and 100 days |
| 📊 **Progress Dashboard** | Track enrolled courses, completed tasks, reward points, and recent activity |
| 🏅 **Leaderboard** | Compete with other learners — filter by all-time, monthly, or weekly rankings |
| 🎟️ **Discount Codes** | Redeem earned points for subscription discount codes (10%–30% off) |

### For Admins
| Feature | Description |
|---------|-------------|
| 📈 **Analytics Dashboard** | Revenue metrics (MRR/ARR), user growth, course performance, engagement KPIs |
| 📝 **Course Management** | Create, edit, publish/unpublish, duplicate courses with drag-and-drop lesson ordering |
| ✏️ **Task Review Queue** | Approve/reject submissions with feedback, bulk actions, and type-specific previews |
| 👥 **User Management** | Search, filter, ban/unban, role changes, manual premium grants |
| 🎟️ **Discount Management** | Create/edit discount codes with point requirements, max redemptions, and expiry dates |
| 💰 **Currency Pricing** | Multi-currency subscription pricing management |
| 📋 **Audit Log** | Track all admin actions with full audit trail |

---

## 🛠 Tech Stack

### Backend
| Technology | Purpose |
|-----------|---------|
| **.NET 8 / ASP.NET Core** | Web API framework |
| **Entity Framework Core 8** | ORM with code-first migrations |
| **PostgreSQL 15+** | Primary database with JSONB support |
| **MediatR** | CQRS pattern (commands/queries) |
| **FluentValidation** | Request validation |
| **AutoMapper** | DTO mapping |
| **JWT + ASP.NET Identity** | Authentication & authorization |
| **Stripe SDK** | Payment processing |
| **Serilog** | Structured logging |
| **Polly** | Resilience & retry policies |
| **Swashbuckle** | Swagger/OpenAPI documentation |

### Frontend
| Technology | Purpose |
|-----------|---------|
| **React 19** | UI framework |
| **TypeScript 5.9** | Type safety |
| **Vite 7** | Build tool & dev server |
| **TailwindCSS 3.4** | Utility-first styling |
| **React Router 7** | Client-side routing |
| **React Query (TanStack) 5** | Server state management & caching |
| **Zustand** | Client state management |
| **React Hook Form + Zod** | Form handling & validation |
| **react-player** | YouTube video embedding |
| **Recharts** | Data visualization / charts |
| **Radix UI** | Accessible headless UI primitives |
| **Framer Motion** | Animations |
| **Lucide React** | Icons |
| **Sonner** | Toast notifications |

### Testing
| Technology | Purpose |
|-----------|---------|
| **xUnit** | Backend unit & integration tests |
| **Vitest** | Frontend unit tests |
| **React Testing Library** | Component testing |
| **Playwright** | End-to-end testing |
| **MSW (Mock Service Worker)** | API mocking |
| **axe-core** | Accessibility testing |

### DevOps
| Technology | Purpose |
|-----------|---------|
| **Docker** | Containerization |
| **docker-compose** | Local development orchestration |
| **GitHub Actions** | CI/CD pipelines |

---

## 🏗 Architecture

The backend follows a **layered architecture** with clear separation of concerns:

```
┌──────────────────────────────────────────────────────────┐
│                    API Layer                              │
│  Controllers · Middleware · Filters · Validators          │
│  (WahadiniCryptoQuest.API)                               │
├──────────────────────────────────────────────────────────┤
│                  Service Layer                            │
│  MediatR Handlers · Commands · Queries · Services        │
│  Mappings · Validators · Background Jobs                 │
│  (WahadiniCryptoQuest.Service)                           │
├──────────────────────────────────────────────────────────┤
│                   Core Layer                             │
│  Entities · DTOs · Interfaces · Enums · Exceptions       │
│  Value Objects · Specifications                          │
│  (WahadiniCryptoQuest.Core)                              │
├──────────────────────────────────────────────────────────┤
│              Data Access Layer (DAL)                      │
│  EF Core DbContext · Repositories · Migrations           │
│  Seeders · Configurations · Unit of Work                 │
│  (WahadiniCryptoQuest.DAL)                               │
├──────────────────────────────────────────────────────────┤
│                   PostgreSQL 15                           │
│  JSONB · Indexes · Soft Delete · Time Partitioning       │
└──────────────────────────────────────────────────────────┘
```

The frontend follows a **feature-based structure** with:
- **Pages** — Route-level components
- **Components** — Reusable UI building blocks (organized by domain)
- **Hooks** — Custom React hooks for data fetching & business logic
- **Services** — API client layer (Axios)
- **Stores** — Global state (Zustand)
- **Types** — TypeScript interfaces and type definitions

---

## 📁 Project Structure

```
WahadiniCryptoQuest/
├── backend/
│   ├── src/
│   │   ├── WahadiniCryptoQuest.API/          # ASP.NET Core Web API
│   │   │   ├── Controllers/                  # REST endpoints
│   │   │   │   ├── Admin/                    # Admin-specific controllers
│   │   │   │   ├── AuthController.cs
│   │   │   │   ├── CoursesController.cs
│   │   │   │   ├── LessonsController.cs
│   │   │   │   ├── ProgressController.cs
│   │   │   │   ├── TaskSubmissionsController.cs
│   │   │   │   ├── RewardsController.cs
│   │   │   │   ├── SubscriptionsController.cs
│   │   │   │   ├── DiscountController.cs
│   │   │   │   ├── WebhooksController.cs
│   │   │   │   └── HealthController.cs
│   │   │   ├── Middleware/                    # JWT, rate limiting, exception handling, audit
│   │   │   ├── Filters/                      # Action & exception filters
│   │   │   ├── Validators/                   # Request validators
│   │   │   ├── BackgroundServices/           # Deduplication cleanup, file cleanup
│   │   │   ├── HealthChecks/                 # Custom health checks
│   │   │   ├── Authorization/                # Policy handlers
│   │   │   ├── Policies/                     # Rate limit policies
│   │   │   ├── Program.cs                    # Application entry point
│   │   │   └── appsettings.json              # Configuration
│   │   │
│   │   ├── WahadiniCryptoQuest.Core/         # Domain layer
│   │   │   ├── Entities/                     # 30+ domain entities
│   │   │   ├── DTOs/                         # Data transfer objects
│   │   │   ├── Interfaces/                   # Service & repository contracts
│   │   │   │   ├── Services/                 # 18 service interfaces
│   │   │   │   └── Repositories/             # 28 repository interfaces
│   │   │   ├── Enums/                        # Domain enumerations
│   │   │   ├── Exceptions/                   # Custom exception types
│   │   │   ├── Specifications/               # Query specifications
│   │   │   └── ValueObjects/                 # Domain value objects
│   │   │
│   │   ├── WahadiniCryptoQuest.DAL/          # Data access layer
│   │   │   ├── Context/                      # EF Core DbContext
│   │   │   ├── Configurations/               # Fluent API entity configs
│   │   │   ├── Repositories/                 # Repository implementations
│   │   │   ├── Migrations/                   # EF Core migrations
│   │   │   ├── Seeders/                      # Data seeders (courses, RBAC, defaults)
│   │   │   ├── UnitOfWork/                   # Unit of Work pattern
│   │   │   └── Identity/                     # ASP.NET Identity integration
│   │   │
│   │   └── WahadiniCryptoQuest.Service/      # Business logic layer
│   │       ├── Services/                     # 15 service implementations
│   │       ├── Commands/                     # MediatR command handlers
│   │       ├── Queries/                      # MediatR query handlers
│   │       ├── Handlers/                     # Event handlers
│   │       ├── Mappings/                     # AutoMapper profiles
│   │       ├── Validators/                   # FluentValidation rules
│   │       ├── BackgroundJobs/               # Scheduled tasks
│   │       └── Notifications/                # Notification services
│   │
│   ├── tests/
│   │   ├── WahadiniCryptoQuest.API.Tests/
│   │   ├── WahadiniCryptoQuest.Core.Tests/
│   │   ├── WahadiniCryptoQuest.DAL.Tests/
│   │   ├── WahadiniCryptoQuest.Service.Tests/
│   │   ├── WahadiniCryptoQuest.Performance.Tests/
│   │   └── WahadiniCryptoQuest.Security.Tests/
│   │
│   ├── Dockerfile
│   └── WahadiniCryptoQuest.sln
│
├── frontend/
│   ├── src/
│   │   ├── pages/                            # Route-level page components
│   │   │   ├── HomePage.tsx
│   │   │   ├── DashboardPage.tsx
│   │   │   ├── LoginPage.tsx
│   │   │   ├── MyCoursesPage.tsx
│   │   │   ├── MySubmissionsPage.tsx
│   │   │   ├── UnauthorizedPage.tsx
│   │   │   ├── auth/                         # Register, forgot/reset password, email verify
│   │   │   ├── courses/                      # Course listing & detail pages
│   │   │   ├── lesson/                       # Lesson player page
│   │   │   ├── rewards/                      # Rewards, leaderboard, transaction history
│   │   │   ├── subscription/                 # Pricing, checkout success/cancel, manage
│   │   │   ├── discount/                     # Available & redeemed discounts
│   │   │   └── admin/                        # Admin dashboard, courses, tasks, users, analytics
│   │   │
│   │   ├── components/                       # Reusable UI components
│   │   │   ├── ui/                           # Base UI primitives (Radix-based)
│   │   │   ├── common/                       # Shared components
│   │   │   ├── layout/                       # Navbar, sidebar, footer
│   │   │   ├── auth/                         # Auth forms & guards
│   │   │   ├── courses/                      # Course cards, filters, detail
│   │   │   ├── lesson/                       # Video player, progress
│   │   │   ├── tasks/                        # Task cards, submission forms
│   │   │   ├── rewards/                      # Point balance, leaderboard
│   │   │   ├── subscription/                 # Pricing, premium gates
│   │   │   ├── discount/                     # Discount cards, redemption
│   │   │   └── admin/                        # Admin-specific components
│   │   │
│   │   ├── hooks/                            # Custom React hooks by domain
│   │   ├── services/                         # API client (Axios) & service modules
│   │   ├── store/                            # Zustand stores (auth, course)
│   │   ├── types/                            # TypeScript type definitions
│   │   ├── routes/                           # Route definitions & guards
│   │   ├── providers/                        # React Query & Theme providers
│   │   ├── layouts/                          # Admin layout
│   │   ├── lib/                              # Utility libraries
│   │   ├── utils/                            # Helpers, formatters, constants
│   │   ├── styles/                           # Global styles
│   │   ├── assets/                           # Static assets
│   │   ├── App.tsx                           # Root component
│   │   └── main.tsx                          # Entry point
│   │
│   ├── tests/                                # Playwright E2E tests
│   ├── playwright.config.ts
│   ├── vitest.config.ts
│   ├── vite.config.ts
│   ├── tailwind.config.js
│   ├── tsconfig.json
│   └── package.json
│
├── docs/                                     # Project documentation
│   ├── COURSE_MANAGEMENT_FEATURE.md
│   ├── DISCOUNT_CODE_INTEGRATION.md
│   ├── PRODUCTION_DEPLOYMENT_GUIDE.md
│   ├── SIDEBAR_HEADER_IMPLEMENTATION.md
│   ├── TEST_COVERAGE_GUIDE.md
│   ├── youtube-integration.md
│   ├── subscription-setup-guide.md
│   ├── webhook-testing-guide.md
│   ├── admin-course-management-guide.md
│   ├── mobile-responsiveness-testing.md
│   ├── security/                             # Security audit reports
│   │   ├── input-validation-audit-report.md
│   │   └── sql-injection-audit-report.md
│   └── user-guides/                          # End-user guides
│
├── specs/                                    # Feature specifications
│   ├── 001-user-auth/
│   ├── 002-database-schema/
│   ├── 003-course-management/
│   ├── 004-youtube-video-player/
│   ├── 005-task-submission-system/
│   ├── 006-reward-system/
│   ├── 007-discount-redemption/
│   ├── 008-stripe-subscription/
│   ├── 009-admin-dashboard/
│   └── 010-admin-dashboard/
│
├── .github/
│   └── workflows/                            # CI/CD pipelines
│       ├── ci.yml
│       ├── ci-cd.yml
│       ├── deploy.yml
│       └── dependencies.yml
│
├── docker-compose.yml                        # PostgreSQL container
├── migration.sql                             # Database migration script
├── setup-database.bat                        # Database setup helper (Windows)
└── README.md
```

---

## 📦 Prerequisites

Ensure the following tools are installed:

| Tool | Version | Installation |
|------|---------|-------------|
| **.NET SDK** | 8.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **Node.js** | 18+ | [Download](https://nodejs.org/) |
| **npm** | 9+ | Included with Node.js |
| **PostgreSQL** | 15+ | [Download](https://www.postgresql.org/download/) or use Docker |
| **Docker** (optional) | Latest | [Download](https://www.docker.com/products/docker-desktop/) |
| **Git** | Latest | [Download](https://git-scm.com/) |

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/WahadiniCryptoQuest.git
cd WahadiniCryptoQuest
```

### 2. Start the Database

**Option A: Using Docker (recommended)**

```bash
docker-compose up -d
```

This starts a PostgreSQL 15 container on port `5432` with:
- Database: `wahadini_crypto_quest`
- Username: `postgres`
- Password: `password`

**Option B: Using the setup script (Windows)**

```bash
setup-database.bat
```

**Option C: Manual PostgreSQL**

Create a database named `wahadini_crypto_quest` in your local PostgreSQL instance, then update the connection string in `backend/src/WahadiniCryptoQuest.API/appsettings.json`.

### 3. Set Up the Backend

```bash
cd backend/src/WahadiniCryptoQuest.API

# Restore dependencies
dotnet restore

# Apply EF Core migrations
dotnet ef database update --project ..\WahadiniCryptoQuest.DAL\WahadiniCryptoQuest.DAL.csproj

# Run the API (default: https://localhost:5171)
dotnet run
```

The Swagger UI is available at: `https://localhost:5171/swagger`

### 4. Set Up the Frontend

```bash
cd frontend

# Install dependencies
npm install

# Copy environment template
cp .env.example .env

# Start dev server (default: http://localhost:5173)
npm run dev
```

### 5. Seed Initial Data

The backend automatically seeds default data on startup, including:
- Default categories (Airdrops, GameFi, DeFi, etc.)
- RBAC roles and permissions (SuperAdmin, ContentCreator, Moderator)
- Sample course data

---

## 🔐 Environment Variables

### Backend (`backend/src/WahadiniCryptoQuest.API/appsettings.json`)

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Database=wahadini_crypto_quest;...` |
| `JwtSettings:SecretKey` | JWT signing key (min 32 chars) | Placeholder — **change in production** |
| `JwtSettings:Issuer` | JWT token issuer | `WahadiniCryptoQuest` |
| `JwtSettings:Audience` | JWT token audience | `WahadiniCryptoQuest-Users` |
| `JwtSettings:AccessTokenExpirationMinutes` | Access token TTL | `60` |
| `JwtSettings:RefreshTokenExpirationDays` | Refresh token TTL | `7` |
| `CorsSettings:AllowedOrigins` | Allowed frontend origins | `http://localhost:5173, ...` |
| `Stripe:SecretKey` | Stripe secret key | Placeholder — set from Stripe Dashboard |
| `Stripe:PublishableKey` | Stripe publishable key | Placeholder — set from Stripe Dashboard |
| `Stripe:WebhookSecret` | Stripe webhook signing secret | Placeholder |
| `Stripe:Prices:*` | Stripe Price IDs per currency | Placeholder |
| `RewardSettings:Streaks:BaseBonus` | Daily streak base points | `5` |
| `RewardSettings:Referral:BonusPoints` | Referral reward points | `100` |
| `Performance:RateLimitPerMinute` | Global API rate limit | `100` |
| `Performance:CacheDurationMinutes` | Response cache duration | `5` |

### Frontend (`frontend/.env`)

| Variable | Description | Default |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | Backend API base URL | `http://localhost:5171/api` |
| `VITE_API_URL` | Backend root URL | `http://localhost:5171` |
| `VITE_STRIPE_PUBLISHABLE_KEY` | Stripe publishable key | Placeholder |

---

## 📡 API Reference

The API is organized into the following controller groups. Full interactive documentation is available via **Swagger UI** at `/swagger` when running the backend.

### Authentication
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/register` | Register a new user | Public |
| `POST` | `/api/auth/login` | Login and receive JWT tokens | Public |
| `POST` | `/api/auth/refresh-token` | Refresh an expired access token | Public |
| `POST` | `/api/auth/forgot-password` | Send password reset email | Public |
| `POST` | `/api/auth/reset-password` | Reset password with token | Public |
| `GET` | `/api/auth/verify-email/{token}` | Verify email address | Public |

### Courses
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/courses` | List courses (with filters, search, pagination) | Public |
| `GET` | `/api/courses/{id}` | Get course details with lessons | Public |
| `POST` | `/api/courses/{id}/enroll` | Enroll in a course | User |
| `GET` | `/api/courses/my-courses` | List user's enrolled courses | User |

### Lessons & Progress
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/lessons/{id}` | Get lesson details with video & tasks | User |
| `PUT` | `/api/lessons/{id}/progress` | Update watch position | User |
| `POST` | `/api/lessons/{id}/complete` | Mark lesson as completed | User |
| `GET` | `/api/progress/...` | Progress tracking endpoints | User |

### Task Submissions
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/tasks/{id}/submit` | Submit a task response | User |
| `GET` | `/api/tasks/my-submissions` | List user's task submissions | User |

### Rewards
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/rewards/points` | Get user's point balance | User |
| `GET` | `/api/rewards/transactions` | Get transaction history | User |
| `GET` | `/api/rewards/leaderboard` | Get leaderboard | User |
| `GET` | `/api/rewards/achievements` | Get user's achievements | User |

### Subscriptions
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/subscriptions/plans` | List subscription plans | Public |
| `POST` | `/api/subscriptions/checkout` | Create Stripe checkout session | User |
| `GET` | `/api/subscriptions/status` | Get subscription status | User |
| `POST` | `/api/subscriptions/cancel` | Cancel subscription | User |

### Discounts
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/discounts/available` | List available discount codes | User |
| `POST` | `/api/rewards/redeem-discount` | Redeem points for a discount code | User |

### Webhooks
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/webhooks/stripe` | Stripe payment webhook handler | Stripe Signature |

### Admin Endpoints
All admin endpoints require the `Admin` role and are prefixed with `/api/admin/`.

| Group | Key Endpoints |
|-------|---------------|
| **Users** | List, view detail, change role, ban/unban, grant premium |
| **Courses** | CRUD operations, publish/unpublish, duplicate, analytics |
| **Task Reviews** | Pending queue, approve/reject, bulk review, request resubmit |
| **Rewards** | Manual point adjustment, discount code CRUD, analytics |
| **Analytics** | Dashboard KPIs, revenue, user growth, course performance |
| **Currency Pricing** | Multi-currency Stripe price management |
| **Audit Log** | Admin action history |

### Health Check
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Application health status |

---

## 🗄 Database Schema

The application uses **30+ entities** managed through EF Core code-first migrations. Key entities include:

### Core Entities

| Entity | Description |
|--------|-------------|
| `User` | User accounts with subscription status, reward points, and profile info |
| `Role` / `UserRole` / `Permission` / `RolePermission` | RBAC system |
| `Category` | Learning categories (Airdrops, GameFi, DeFi, etc.) |
| `Course` | Courses with difficulty level, premium flag, and point rewards |
| `Lesson` | Lessons with YouTube video IDs, ordering, and content markdown |
| `LearningTask` | Tasks with type-specific JSONB data |
| `UserTaskSubmission` | User submissions with status tracking and review workflow |
| `UserProgress` | Video watch position and lesson completion tracking |
| `UserCourseEnrollment` | Course enrollment and completion status |
| `RewardTransaction` | Immutable points ledger (earn, redeem, bonus, penalty) |
| `UserAchievement` | Badge and milestone tracking |
| `UserStreak` | Daily login streak tracking |
| `DiscountCode` | Discount codes with point requirements and usage limits |
| `UserDiscountRedemption` | Discount redemption records |
| `Subscription` / `SubscriptionHistory` | Stripe subscription management |
| `CurrencyPricing` | Multi-currency pricing configuration |
| `AuditLogEntry` | Admin action audit trail |
| `UserNotification` | In-app notifications |
| `ReferralAttribution` | Referral tracking |
| `WebhookEvent` | Stripe webhook event log |
| `RefreshToken` | JWT refresh token storage |
| `EmailVerificationToken` / `PasswordResetToken` | Auth flow tokens |

### Key Indexes
- User lookup by email and username
- Course filtering by category, difficulty, and premium status
- Task submissions by user and status
- Reward transactions by user with deduplication index
- Leaderboard queries optimized with composite indexes

### Migrations

Migrations are stored in `backend/src/WahadiniCryptoQuest.DAL/Migrations/` and can be applied with:

```bash
cd backend/src/WahadiniCryptoQuest.API
dotnet ef database update --project ..\WahadiniCryptoQuest.DAL\WahadiniCryptoQuest.DAL.csproj
```

---

## �� Frontend Pages

### Public Pages
| Page | Route | Description |
|------|-------|-------------|
| Home | `/` | Landing page with hero section, featured courses, and categories |
| Courses | `/courses` | Browse courses with filters (category, difficulty, premium/free, search) |
| Course Detail | `/courses/:id` | Course info, lesson list with progress, enrollment button |
| Login | `/login` | Email & password login |
| Register | `/auth/register` | User registration |
| Forgot Password | `/auth/forgot-password` | Request password reset |
| Reset Password | `/auth/reset-password` | Set new password with token |
| Email Verification | `/auth/verify-email` | Email verification callback |
| Pricing | `/subscription/pricing` | Subscription plans with feature comparison |

### Authenticated Pages
| Page | Route | Description |
|------|-------|-------------|
| Dashboard | `/dashboard` | User stats, continue learning, recent activity |
| My Courses | `/my-courses` | Enrolled courses with progress |
| Lesson Player | `/lesson/:id` | YouTube video player with progress tracking, tasks section |
| My Submissions | `/my-submissions` | Task submission history with status |
| Rewards | `/rewards` | Points balance, transaction history, achievements |
| Leaderboard | `/rewards/leaderboard` | Top users by points |
| Available Discounts | `/discounts/available` | Browse and redeem discount codes |
| My Discounts | `/discounts/my` | Redeemed discount codes |
| Manage Subscription | `/subscription/manage` | Current plan, cancel, billing |
| Checkout Success | `/subscription/success` | Post-payment confirmation |

### Admin Pages (requires Admin role)
| Page | Route | Description |
|------|-------|-------------|
| Admin Dashboard | `/admin` | KPI cards, charts, recent activity feed |
| Course Management | `/admin/courses` | CRUD courses with lesson & task editors |
| Task Review | `/admin/tasks` | Approve/reject submissions with type-specific previews |
| User Management | `/admin/users` | Search, filter, role changes, ban/unban |
| Analytics | `/admin/analytics` | Revenue, engagement, content performance charts |
| Discount Codes | `/admin/discounts` | Create/edit discount codes |
| Discount Analytics | `/admin/discounts/analytics` | Redemption metrics |
| Currency Management | `/admin/currency` | Multi-currency pricing setup |
| Audit Log | `/admin/audit-log` | Admin action history |

---

## 🧪 Testing

### Backend Tests

The backend has **6 test projects** covering different aspects:

```bash
cd backend

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/WahadiniCryptoQuest.API.Tests
dotnet test tests/WahadiniCryptoQuest.Service.Tests
dotnet test tests/WahadiniCryptoQuest.Core.Tests
dotnet test tests/WahadiniCryptoQuest.DAL.Tests
dotnet test tests/WahadiniCryptoQuest.Performance.Tests
dotnet test tests/WahadiniCryptoQuest.Security.Tests
```

| Test Project | Scope |
|-------------|-------|
| `API.Tests` | Controller integration tests, middleware tests |
| `Service.Tests` | Business logic unit tests, MediatR handler tests |
| `Core.Tests` | Entity and value object tests |
| `DAL.Tests` | Repository and database tests |
| `Performance.Tests` | Load and performance benchmarks |
| `Security.Tests` | Auth, authorization, and vulnerability tests |

### Frontend Tests

```bash
cd frontend

# Run unit tests (Vitest)
npm test

# Run with UI
npm run test:ui

# Run with coverage
npm run test:coverage

# Run E2E tests (Playwright)
npx playwright test

# Run linter
npm run lint
```

| Test Type | Tool | Config |
|-----------|------|--------|
| Unit tests | Vitest | `vitest.config.ts` |
| Component tests | React Testing Library + Vitest | `vitest.config.ts` |
| API mocking | MSW | `src/test/` |
| E2E tests | Playwright | `playwright.config.ts` |
| Accessibility | axe-core + Playwright | `playwright.config.ts` |
| Coverage | `@vitest/coverage-v8` | `vitest.config.ts` |

---

## 🔄 CI/CD

GitHub Actions workflows are located in `.github/workflows/`:

| Workflow | File | Description |
|----------|------|-------------|
| **CI** | `ci.yml` | Runs on PRs — build, lint, test (backend + frontend) |
| **CI/CD** | `ci-cd.yml` | Full pipeline — build, test, and deploy |
| **Deploy** | `deploy.yml` | Production deployment |
| **Dependencies** | `dependencies.yml` | Dependency update checks |

---

## 🐳 Docker Deployment

### Development (PostgreSQL only)

```bash
# Start PostgreSQL
docker-compose up -d

# Verify it's running
docker-compose ps

# View logs
docker-compose logs -f postgres

# Stop
docker-compose down
```

### Full Stack Docker Build

The backend includes a production-ready **multi-stage Dockerfile** (`backend/Dockerfile`) that:
1. Builds the .NET application in a SDK container
2. Publishes optimized binaries
3. Runs in a lightweight `aspnet:8.0` runtime image
4. Uses a non-root user for security
5. Includes a health check endpoint

```bash
# Build backend image
cd backend
docker build -t wahadini-api .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;..." \
  wahadini-api
```

---

## 📚 Documentation

Detailed documentation is available in the `docs/` directory:

| Document | Description |
|----------|-------------|
| [Course Management Feature](docs/COURSE_MANAGEMENT_FEATURE.md) | Course & lesson system technical details |
| [Discount Code Integration](docs/DISCOUNT_CODE_INTEGRATION.md) | Discount system architecture |
| [YouTube Integration](docs/youtube-integration.md) | Video player setup and progress tracking |
| [Subscription Setup Guide](docs/subscription-setup-guide.md) | Stripe subscription configuration |
| [Webhook Testing Guide](docs/webhook-testing-guide.md) | Testing Stripe webhooks locally |
| [Admin Course Management](docs/admin-course-management-guide.md) | Admin course editor guide |
| [Production Deployment](docs/PRODUCTION_DEPLOYMENT_GUIDE.md) | Deployment checklist and guide |
| [Test Coverage Guide](docs/TEST_COVERAGE_GUIDE.md) | Testing strategy and coverage targets |
| [Mobile Responsiveness](docs/mobile-responsiveness-testing.md) | Responsive design testing guide |
| [Security Audits](docs/security/) | SQL injection and input validation audits |
| [User Guides](docs/user-guides/) | End-user documentation |

Feature specifications are available in `specs/` with detailed requirements for each module.

---

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/amazing-feature`
3. **Commit** your changes: `git commit -m 'Add amazing feature'`
4. **Push** to the branch: `git push origin feature/amazing-feature`
5. **Open** a Pull Request

### Development Guidelines

- **Backend**: Follow .NET 8 / C# conventions with nullable reference types enabled
- **Frontend**: Follow TypeScript strict mode, use functional components and hooks
- **Commits**: Use conventional commit messages (`feat:`, `fix:`, `docs:`, `refactor:`, etc.)
- **Tests**: Write tests for new features — maintain existing coverage
- **Code Review**: All PRs require review before merging

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  Built with ❤️ by the WahadiniCryptoQuest Team
</p>
