# Admin Dashboard - Developer Quickstart

**Feature**: 009-admin-dashboard  
**Branch**: `009-admin-dashboard`  
**Estimated Setup Time**: 30 minutes

---

## Prerequisites

Before starting development, ensure you have:

- [x] **.NET 8 SDK** installed
- [x] **Node.js 18+** and npm installed
- [x] **PostgreSQL 15+** running locally or accessible
- [x] **Git** configured with WahadiniCryptoQuest repository access
- [x] **VS Code** (recommended) or preferred IDE
- [x] **pgAdmin** or similar database tool (optional)

---

## 1. Environment Setup

### Clone & Switch to Feature Branch

```bash
cd c:\My code\my\WahadiniCryptoQuest_new
git checkout 009-admin-dashboard
git pull origin 009-admin-dashboard
```

### Configure Backend Environment

Create or update `backend/src/WahadiniCryptoQuest.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=wahadini_dev;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyForJWT_MinLength32Characters",
    "Issuer": "WahadiniCryptoQuest",
    "Audience": "WahadiniCryptoQuest",
    "ExpiryInMinutes": 60
  },
  "Email": {
    "SmtpHost": "smtp.mailtrap.io",
    "SmtpPort": 2525,
    "SmtpUser": "your_mailtrap_user",
    "SmtpPassword": "your_mailtrap_password",
    "FromEmail": "noreply@wahadini.com",
    "FromName": "WahadiniCryptoQuest"
  },
  "AdminSettings": {
    "DefaultSuperAdminEmail": "admin@wahadini.com",
    "DefaultSuperAdminPassword": "Admin@12345",
    "EnableAuditLog": true,
    "AuditLogRetentionDays": 365
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Configure Frontend Environment

Create `frontend/.env.development`:

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_ENABLE_ADMIN_DASHBOARD=true
VITE_ADMIN_ROUTE=/admin
```

---

## 2. Database Setup

### Run Migrations

```bash
cd backend/src/WahadiniCryptoQuest.API

# Apply all migrations including admin dashboard entities
dotnet ef database update --project ../WahadiniCryptoQuest.DAL

# Verify migration applied
dotnet ef migrations list --project ../WahadiniCryptoQuest.DAL
```

**Expected migrations**:
- `20250101_AddAuditLogEntities`
- `20250101_AddUserNotifications`
- `20250101_AddPointAdjustments`
- `20250101_ExtendDiscountCodeManagement`

### Seed Initial SuperAdmin Account

Run the database seed script:

```bash
cd backend
dotnet run --project src/WahadiniCryptoQuest.API -- seed-admin
```

**Output**:
```
✅ SuperAdmin account created
   Email: admin@wahadini.com
   Password: Admin@12345
   Role: SuperAdmin
⚠️  IMPORTANT: Change password after first login!
```

### Verify Database Schema

Connect to PostgreSQL and verify tables:

```sql
-- Check new tables
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('AuditLogEntries', 'UserNotifications', 'PointAdjustments');

-- Verify indexes
SELECT indexname, tablename 
FROM pg_indexes 
WHERE schemaname = 'public' 
AND tablename = 'AuditLogEntries';
```

---

## 3. Backend Development

### Install Dependencies

```bash
cd backend
dotnet restore
```

### Run Backend Server

```bash
cd backend/src/WahadiniCryptoQuest.API
dotnet run
```

**Expected output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### Verify API Health

Test endpoints:

```bash
# Health check
curl http://localhost:5000/health

# Auth endpoint (should return 401)
curl http://localhost:5000/api/admin/stats
```

### Run Backend Tests

```bash
cd backend/tests
dotnet test --logger "console;verbosity=detailed"
```

---

## 4. Frontend Development

### Install Dependencies

```bash
cd frontend
npm install
```

### Run Frontend Dev Server

```bash
npm run dev
```

**Expected output**:
```
VITE v5.0.0  ready in 500 ms

➜  Local:   http://localhost:5173/
➜  Network: use --host to expose
```

### Access Admin Dashboard

1. Navigate to [http://localhost:5173](http://localhost:5173)
2. Login with SuperAdmin credentials:
   - Email: `admin@wahadini.com`
   - Password: `Admin@12345`
3. Navigate to [http://localhost:5173/admin](http://localhost:5173/admin)

**Expected UI**:
- Admin navigation sidebar visible
- Dashboard KPI cards loading
- No errors in browser console

### Run Frontend Tests

```bash
cd frontend

# Unit tests
npm run test

# E2E tests (requires backend running)
npm run test:e2e
```

---

## 5. Development Workflow

### Creating a New Admin Endpoint

**Backend** (`backend/src/WahadiniCryptoQuest.API/Controllers/AdminController.cs`):

```csharp
[Authorize(Roles = "Admin,SuperAdmin")]
[HttpGet("example")]
public async Task<IActionResult> GetExample()
{
    var query = new GetExampleQuery();
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

**Frontend** (`frontend/src/services/adminService.ts`):

```typescript
export const getExample = async (): Promise<ExampleDto> => {
  const response = await apiClient.get<ExampleDto>('/admin/example');
  return response.data;
};
```

### Adding a New Admin Page

1. Create component: `frontend/src/pages/admin/NewFeature.tsx`
2. Add route: `frontend/src/routes/admin.routes.tsx`
3. Update sidebar: `frontend/src/components/admin/AdminSidebar.tsx`

### Database Migration Workflow

```bash
cd backend/src/WahadiniCryptoQuest.API

# Create migration
dotnet ef migrations add YourMigrationName --project ../WahadiniCryptoQuest.DAL

# Review migration file
# Apply migration
dotnet ef database update --project ../WahadiniCryptoQuest.DAL
```

---

## 6. Testing Admin Features

### Manual Testing Checklist

- [ ] **Login as SuperAdmin**: Verify dashboard loads
- [ ] **User Management**: Search, filter, ban user
- [ ] **Task Review**: Approve/reject submission
- [ ] **Course Creation**: Create course with rich text
- [ ] **Discount Code**: Create and view redemptions
- [ ] **Audit Log**: Verify actions logged
- [ ] **Role Change**: Promote user to Admin (SuperAdmin only)
- [ ] **Analytics**: Charts render correctly

### Test Accounts

| Role | Email | Password |
|------|-------|----------|
| SuperAdmin | admin@wahadini.com | Admin@12345 |
| Admin | testadmin@test.com | TestAdmin@123 |
| User | testuser@test.com | TestUser@123 |

### API Testing with Postman

1. **Authenticate**:
   ```
   POST http://localhost:5000/api/auth/login
   Body: { "email": "admin@wahadini.com", "password": "Admin@12345" }
   ```
2. **Copy JWT token** from response
3. **Set Authorization header**: `Bearer <token>`
4. **Test admin endpoints** as documented in `contracts/api-spec.md`

---

## 7. Common Issues & Troubleshooting

### ❌ Migration Fails: "Table already exists"

**Solution**:
```bash
# Drop and recreate database
dotnet ef database drop --force --project ../WahadiniCryptoQuest.DAL
dotnet ef database update --project ../WahadiniCryptoQuest.DAL
```

### ❌ Frontend: "401 Unauthorized" on admin routes

**Check**:
- JWT token in localStorage (`authToken`)
- Token not expired (60-minute default)
- User has Admin/SuperAdmin role

**Fix**: Re-login to refresh token

### ❌ Backend: "Insufficient permissions"

**Check** `appsettings.Development.json`:
- `AdminSettings:DefaultSuperAdminEmail` matches login email
- Database seeding completed successfully

**Fix**:
```bash
dotnet run --project src/WahadiniCryptoQuest.API -- seed-admin --force
```

### ❌ Charts not rendering

**Check**:
- `recharts` package installed: `npm list recharts`
- API returning valid data structure
- Browser console for errors

**Fix**:
```bash
cd frontend
npm install recharts
```

---

## 8. Next Steps

After completing quickstart:

1. **Read Architecture Docs**:
   - `.specify/prompts/architecture.prompt.md`
   - `.specify/prompts/backend.prompt.md`
   - `.specify/prompts/frontend.prompt.md`

2. **Review Feature Spec**: `specs/009-admin-dashboard/spec.md`

3. **Explore Data Model**: `specs/009-admin-dashboard/data-model.md`

4. **API Contracts**: `specs/009-admin-dashboard/contracts/api-spec.md`

5. **Implementation Plan**: `specs/009-admin-dashboard/plan.md`

---

## 9. Development Tools

### Recommended VS Code Extensions

- **C# Dev Kit** (ms-dotnettools.csdevkit)
- **ESLint** (dbaeumer.vscode-eslint)
- **Prettier** (esbenp.prettier-vscode)
- **Tailwind CSS IntelliSense** (bradlc.vscode-tailwindcss)
- **REST Client** (humao.rest-client) - Test API endpoints

### Useful Commands

```bash
# Backend: Watch mode
cd backend/src/WahadiniCryptoQuest.API
dotnet watch run

# Frontend: Type checking
cd frontend
npm run type-check

# Database: View logs
docker logs wahadini-postgres -f

# Git: View branch changes
git diff develop..009-admin-dashboard
```

---

## 10. Support & Resources

- **Feature Spec**: [specs/009-admin-dashboard/spec.md](./spec.md)
- **API Contracts**: [contracts/api-spec.md](./contracts/api-spec.md)
- **Implementation Plan**: [plan.md](./plan.md)
- **Architecture Guide**: `.specify/prompts/architecture.prompt.md`
- **Team Lead**: Project maintainer (see CODEOWNERS)

---

**Ready to start coding?** 🚀  
Begin with Phase 1 tasks in `plan.md` and reference this guide as needed.
