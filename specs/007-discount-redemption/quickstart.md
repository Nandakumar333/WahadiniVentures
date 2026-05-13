# Quickstart Guide: Discount Redemption System

**Feature**: 007-discount-redemption  
**Last Updated**: December 5, 2025

## Overview

This guide helps developers quickly understand and work with the discount redemption system. It covers setup, key concepts, common tasks, and troubleshooting.

---

## Prerequisites

- **.NET 8 SDK** installed
- **PostgreSQL 15+** running locally or accessible
- **Node.js 18+** and npm for frontend
- **Stripe Account** with Promotion Codes feature enabled
- **IDE**: Visual Studio 2022, VS Code, or Rider

---

## Quick Setup (5 minutes)

### 1. Database Migration

```bash
# Navigate to DAL project
cd backend/src/WahadiniCryptoQuest.DAL

# Create and apply migration
dotnet ef migrations add AddDiscountSystem --startup-project ../WahadiniCryptoQuest.API
dotnet ef database update --startup-project ../WahadiniCryptoQuest.API
```

**Verify**: Check that `DiscountTypes` and `UserDiscountRedemptions` tables exist in your database.

### 2. Seed Initial Data

The migration automatically seeds 3 sample discount types:
- **SAVE10**: 500 points for 10% off monthly
- **SAVE20**: 1000 points for 20% off monthly
- **SAVE50**: 5000 points for 50% off annual (limited to 100 redemptions)

### 3. Create Stripe Promotion Codes

**Important**: The system expects these codes to exist in Stripe.

1. Log in to [Stripe Dashboard](https://dashboard.stripe.com)
2. Navigate to **Products → Coupons**
3. Create coupons for 10%, 20%, and 50% off
4. Navigate to **Products → Promotion Codes**
5. Create codes:
   - `SAVE10` → 10% coupon
   - `SAVE20` → 20% coupon
   - `SAVE50` → 50% coupon

### 4. Run Backend

```bash
cd backend/src/WahadiniCryptoQuest.API
dotnet run
```

Backend will start on `https://localhost:5001`

### 5. Run Frontend

```bash
cd frontend
npm install  # First time only
npm run dev
```

Frontend will start on `http://localhost:5173`

### 6. Test the System

1. **Register/Login** as a user
2. **Earn points** (complete tasks, watch videos, etc.)
3. **Navigate** to `/discounts` page
4. **View** available discounts
5. **Click "Redeem"** on an eligible discount
6. **Confirm** redemption in modal
7. **Copy code** from success modal
8. **Check** "My Discounts" page

---

## Key Concepts

### Discount Type

A campaign configuration created by admins. Properties:
- **Name**: Display name (e.g., "10% Off Monthly")
- **Code**: Stripe Promotion Code (e.g., "SAVE10")
- **PointCost**: Points required to redeem
- **DiscountPercentage**: Discount amount (0-100)
- **DurationDays**: Days code is valid after redemption
- **MaxRedemptions**: Global limit (0 = unlimited)
- **ExpiryDate**: Campaign end date (null = no expiry)

### User Redemption

A record of a user redeeming a discount. Properties:
- **CodeIssued**: Actual code given to user
- **RedeemedAt**: Redemption timestamp
- **ExpiryDate**: When code expires
- **IsUsed**: Whether used at checkout
- **UsedAt**: Usage timestamp (via Stripe webhook)

### Redemption Flow

```
User Browses Discounts → Clicks Redeem → System Validates
    ↓
Points Sufficient? → Discount Available? → User Eligible?
    ↓
Atomic Transaction:
  - Deduct points from user
  - Create redemption record
  - Create audit trail (RewardTransaction)
  - Increment redemption counter
    ↓
Return Code to User → Display in Modal
```

---

## Common Development Tasks

### Add a New Discount Type (As Admin)

**API Request**:
```http
POST /api/admin/discounts
Authorization: Bearer {admin_jwt_token}
Content-Type: application/json

{
  "name": "30% Off Annual Subscription",
  "code": "SAVE30",
  "description": "Redeem 3000 points for 30% off annual",
  "pointCost": 3000,
  "discountPercentage": 30.00,
  "durationDays": 90,
  "maxRedemptions": 0,
  "expiryDate": null,
  "isActive": true
}
```

**Using Swagger**:
1. Navigate to `https://localhost:5001/swagger`
2. Authorize with admin JWT token
3. Find `POST /api/admin/discounts`
4. Fill in request body
5. Execute

### Test Redemption Logic (Unit Test)

```csharp
[Fact]
public async Task RedeemDiscount_WithSufficientPoints_ShouldSucceed()
{
    // Arrange
    var user = new User { Id = Guid.NewGuid(), PointsBalance = 1000 };
    var discount = DiscountType.Create("Test", "TEST10", "Description", 500, 10);
    
    _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id))
        .ReturnsAsync(user);
    _discountRepositoryMock.Setup(r => r.GetByIdAsync(discount.Id))
        .ReturnsAsync(discount);
    
    // Act
    var result = await _discountService.RedeemDiscountAsync(user.Id, discount.Id);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Code.Should().Be("TEST10");
    user.PointsBalance.Should().Be(500);
}
```

### Query Available Discounts (Service Layer)

```csharp
public async Task<List<DiscountTypeDto>> GetAvailableDiscountsAsync(Guid userId)
{
    var now = DateTime.UtcNow;
    
    var discounts = await _dbContext.DiscountTypes
        .AsNoTracking()
        .Where(d => d.IsActive &&
                    (d.ExpiryDate == null || d.ExpiryDate > now) &&
                    (d.MaxRedemptions == 0 || d.CurrentRedemptions < d.MaxRedemptions))
        .OrderBy(d => d.PointCost)
        .ToListAsync();
    
    // Filter out discounts user already redeemed (if one-per-user)
    var userRedemptions = await _dbContext.UserDiscountRedemptions
        .Where(r => r.UserId == userId)
        .Select(r => r.DiscountTypeId)
        .ToListAsync();
    
    return discounts
        .Where(d => !userRedemptions.Contains(d.Id)) // Simplified - should check OnePerUser flag
        .Select(d => _mapper.Map<DiscountTypeDto>(d))
        .ToList();
}
```

### Frontend: Redeem Discount Hook

```typescript
export const useRedeemDiscount = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (discountId: string) => {
      const response = await api.post(`/discounts/${discountId}/redeem`);
      return response.data.data;
    },
    
    onSuccess: (data) => {
      // Invalidate queries to refetch updated data
      queryClient.invalidateQueries({ queryKey: ['discounts', 'available'] });
      queryClient.invalidateQueries({ queryKey: ['discounts', 'my-redemptions'] });
      queryClient.invalidateQueries({ queryKey: ['user', 'points'] });
      
      // Show success toast
      toast.success(`Code ${data.code} redeemed successfully!`);
    },
    
    onError: (error: ApiError) => {
      toast.error(error.message || 'Failed to redeem discount');
    },
  });
};
```

---

## Testing

### Run Unit Tests

```bash
cd backend/tests/WahadiniCryptoQuest.Service.Tests
dotnet test
```

### Run Integration Tests

```bash
cd backend/tests/WahadiniCryptoQuest.API.Tests
dotnet test
```

### Run Frontend Tests

```bash
cd frontend
npm test
```

### Test Concurrency Scenario

```csharp
[Fact]
public async Task RedeemDiscount_ConcurrentRequests_OnlyOneSucceeds()
{
    // Arrange
    var user = new User { Id = Guid.NewGuid(), PointsBalance = 500, RowVersion = new byte[] { 1 } };
    var discount = DiscountType.Create("Test", "TEST", "Desc", 500, 10);
    
    // Act - Simulate concurrent redemption attempts
    var tasks = Enumerable.Range(0, 10)
        .Select(_ => _discountService.RedeemDiscountAsync(user.Id, discount.Id))
        .ToList();
    
    var results = await Task.WhenAll(tasks);
    
    // Assert
    var successful = results.Count(r => r.IsSuccess);
    successful.Should().Be(1, "Only one concurrent redemption should succeed");
}
```

---

## Troubleshooting

### Issue: "Insufficient Points" error but user has enough points

**Cause**: Points balance not up-to-date due to caching or stale data.

**Solution**:
1. Check `Users.PointsBalance` in database
2. Verify `RewardTransaction` records sum correctly
3. Clear frontend cache: `localStorage.clear()` and reload
4. Invalidate React Query cache: `queryClient.clear()`

### Issue: "Discount unavailable" but discount is active

**Possible Causes**:
1. **Expired**: Check `ExpiryDate` is null or future date
2. **Max Redemptions Reached**: Check `CurrentRedemptions < MaxRedemptions`
3. **Already Redeemed**: User already redeemed this discount (if one-per-user)
4. **Soft Deleted**: Check `IsDeleted = false`

**Solution**:
```sql
SELECT * FROM "DiscountTypes" WHERE "Code" = 'SAVE10';
SELECT "CurrentRedemptions", "MaxRedemptions", "ExpiryDate", "IsActive", "IsDeleted" 
FROM "DiscountTypes" WHERE "Id" = '{discount-id}';
```

### Issue: Concurrency exception during redemption

**Cause**: Another transaction modified user's points between read and write.

**Solution**: This is expected behavior. Frontend should:
1. Catch `409 Conflict` response
2. Display "Please retry" message to user
3. User clicks "Redeem" again with refreshed data

### Issue: Code not appearing in Stripe checkout

**Cause**: Code doesn't exist in Stripe or is inactive.

**Solution**:
1. Verify code exists: Stripe Dashboard → Promotion Codes
2. Check code is active and not expired in Stripe
3. Ensure code spelling matches exactly (case-sensitive)
4. User must manually enter code at checkout (auto-apply not supported in v1)

### Issue: Database migration fails

**Error**: `Sequence contains no elements` or `Table already exists`

**Solution**:
```bash
# Check current migrations
dotnet ef migrations list --startup-project ../WahadiniCryptoQuest.API

# Remove last migration if failed
dotnet ef migrations remove --startup-project ../WahadiniCryptoQuest.API --force

# Recreate and apply
dotnet ef migrations add AddDiscountSystem --startup-project ../WahadiniCryptoQuest.API
dotnet ef database update --startup-project ../WahadiniCryptoQuest.API
```

---

## API Endpoints Quick Reference

### User Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/discounts/available` | Get discounts user can redeem |
| POST | `/api/discounts/{id}/redeem` | Redeem specific discount |
| GET | `/api/discounts/my-redemptions` | Get user's redeemed codes |

### Admin Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/discounts` | Get all discount types |
| POST | `/api/admin/discounts` | Create new discount type |
| PUT | `/api/admin/discounts/{id}` | Update discount type |
| DELETE | `/api/admin/discounts/{id}` | Soft delete discount type |
| POST | `/api/admin/discounts/{id}/activate` | Activate discount |
| POST | `/api/admin/discounts/{id}/deactivate` | Deactivate discount |
| GET | `/api/admin/discounts/{id}/analytics` | Get discount analytics |
| GET | `/api/admin/discounts/analytics/summary` | Get overall analytics |

---

## Useful Database Queries

### Check User's Points Balance

```sql
SELECT "Id", "Email", "PointsBalance" 
FROM "Users" 
WHERE "Email" = 'user@example.com';
```

### View All Active Discounts

```sql
SELECT "Name", "Code", "PointCost", "CurrentRedemptions", "MaxRedemptions"
FROM "DiscountTypes"
WHERE "IsActive" = true 
  AND "IsDeleted" = false
  AND ("ExpiryDate" IS NULL OR "ExpiryDate" > NOW());
```

### Check User's Redemptions

```sql
SELECT 
    r."CodeIssued",
    r."RedeemedAt",
    r."ExpiryDate",
    r."IsUsed",
    d."Name" as "DiscountName"
FROM "UserDiscountRedemptions" r
JOIN "DiscountTypes" d ON r."DiscountTypeId" = d."Id"
WHERE r."UserId" = '{user-id}'
ORDER BY r."RedeemedAt" DESC;
```

### Audit Trail for Specific User

```sql
SELECT 
    "Amount",
    "Type",
    "Description",
    "CreatedAt"
FROM "RewardTransactions"
WHERE "UserId" = '{user-id}'
ORDER BY "CreatedAt" DESC
LIMIT 20;
```

---

## Configuration

### appsettings.json (Backend)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=wahadinicryptoquest;Username=postgres;Password=yourpassword"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_..."
  },
  "DiscountSettings": {
    "DefaultDurationDays": 30,
    "MaxConcurrentRedemptions": 50
  }
}
```

### .env (Frontend)

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_...
```

---

## Next Steps

1. **Read Documentation**: Review `data-model.md` for entity details and `research.md` for technical decisions
2. **Explore API**: Use Swagger UI at `https://localhost:5001/swagger` to test endpoints
3. **Run Tests**: Execute unit and integration tests to verify functionality
4. **Implement Frontend**: Build UI components following the contracts
5. **Deploy**: Follow deployment checklist for staging and production

---

## Support & Resources

- **API Documentation**: Swagger UI at `/swagger`
- **Database Schema**: See `data-model.md`
- **Technical Decisions**: See `research.md`
- **API Contracts**: See `contracts/` directory
- **Implementation Plan**: See `plan.md`

---

## Contribution Guidelines

When working on this feature:
1. Follow Clean Architecture patterns
2. Write unit tests for business logic
3. Write integration tests for API endpoints
4. Update OpenAPI specs if changing contracts
5. Document breaking changes
6. Run full test suite before committing

---

**Happy Coding! 🚀**
