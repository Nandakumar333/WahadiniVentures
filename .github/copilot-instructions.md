# WahadiniCryptoQuest Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-11-16

## Active Technologies
- PostgreSQL with time-based partitioning for user activity data (001-platform-baseline-spec)
- .NET 8 C# (backend), TypeScript/React 18 (frontend) + ASP.NET Core Web API, Entity Framework Core, React 18, Vite, TailwindCSS, ASP.NET Identity, JWT authentication, Stripe SDK (001-user-auth)
- .NET 8 C# (backend), TypeScript 4.9+ with React 18 (frontend) + ASP.NET Core Web API, Entity Framework Core 8.0, PostgreSQL, AutoMapper, FluentValidation, MediatR, JWT Bearer tokens, React Router 7, React Query 5, TailwindCSS 3.4, Zod validation (001-user-auth)
- PostgreSQL 15+ with time-based partitioning for user activity data, Entity Framework Core with code-first migrations (001-user-auth)
- .NET 8 C# (backend), TypeScript 4.9+ with React 18 (frontend) + ASP.NET Core Web API, Entity Framework Core 8.0, PostgreSQL, AutoMapper, FluentValidation, MediatR, JWT Bearer tokens, React Router 7, React Query 5, TailwindCSS 3.4, Zod validation, Vite build tool (001-user-auth)
- .NET 8 C# (backend), TypeScript/React 18 (frontend) + ASP.NET Core Web API, Entity Framework Core, React 18, Vite, TailwindCSS, react-player (YouTube integration), Stripe SDK, ASP.NET Identity, JWT authentication (001-platform-baseline-spec)
- PostgreSQL 15+ with JSONB support, Entity Framework Core 8.0, Repository pattern + Unit of Work, time-based partitioning, soft delete strategy (007-database-schema)
- react-player library for YouTube video embedding, React Query 5 for progress caching, Zustand for player state, exponential backoff retry logic, localStorage queue for offline sync (004-youtube-video-player)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

npm test; npm run lint

## Code Style

.NET 8 C# (backend), TypeScript/React 18 (frontend): Follow standard conventions

## Recent Changes
- 004-youtube-video-player: Added react-player library for YouTube video embedding with automatic progress tracking, resume functionality, completion detection at 80% threshold, and point rewards integration
- 007-database-schema: Added PostgreSQL 15+ with JSONB support, Entity Framework Core 8.0, Repository pattern + Unit of Work, time-based partitioning, soft delete strategy
- 001-user-auth: Added .NET 8 C# (backend), TypeScript 4.9+ with React 18 (frontend) + ASP.NET Core Web API, Entity Framework Core 8.0, PostgreSQL, AutoMapper, FluentValidation, MediatR, JWT Bearer tokens, React Router 7, React Query 5, TailwindCSS 3.4, Zod validation, Vite build tool
- 001-user-auth: Added .NET 8 C# (backend), TypeScript 4.9+ with React 18 (frontend) + ASP.NET Core Web API, Entity Framework Core 8.0, PostgreSQL, AutoMapper, FluentValidation, MediatR, JWT Bearer tokens, React Router 7, React Query 5, TailwindCSS 3.4, Zod validation, Vite build tool


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
