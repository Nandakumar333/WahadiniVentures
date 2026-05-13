# Feature: Point-Based Discount Redemption System

## /speckit.specify

### Feature Overview
Implement a point-based discount redemption system where users can exchange their earned reward points for subscription discounts or free trials. This system integrates with the existing rewards infrastructure and Stripe checkout flow.

### Feature Scope
-   **Discount Management**: Admin ability to create and manage discount types (percentage off, free trial).
-   **Redemption Logic**: Validation of user points, deduction of points, and generation of unique or static discount codes.
-   **User Interface**: Gallery of available discounts, redemption modal, and "My Discounts" wallet.
-   **Admin Interface**: Dashboard for managing discount rules and viewing redemption statistics.
-   **Stripe Integration**: Ensuring generated codes are valid for use during Stripe checkout.

### User Stories
1.  **As a User**, I want to view a list of available discounts so I can see what I can buy with my points.
2.  **As a User**, I want to redeem my points for a specific discount code.
3.  **As a User**, I want to see my redeemed codes so I can copy them for use during checkout.
4.  **As a User**, I want to be prevented from redeeming a code if I don't have enough points or have already redeemed it.
5.  **As an Admin**, I want to create new discount types (e.g., "50% OFF") and set their point costs.
6.  **As an Admin**, I want to see how many users are redeeming specific discounts.
7.  **As an Admin**, I want to set expiry dates and redemption limits to control campaign duration.

### Technical Requirements
-   **Transactional Integrity**: Point deduction and discount issuance must happen in a single atomic transaction.
-   **Concurrency Handling**: Prevent double-spending of points via optimistic concurrency control.
-   **Validation**: Strict checks for point balance, prior redemptions (if limited), and expiry dates.
-   **Audit Trail**: All redemptions must create a negative `RewardTransaction` record.
-   **Security**: Admin endpoints must be secured with `RequireAdmin` policy.

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Backend Core & Data Layer
**Tasks:**
1.  Create `DiscountType` and `UserDiscountRedemption` entities.
2.  Create EF Core configurations and migrations.
3.  Implement `IDiscountRepository`.
4.  Update `RewardService` to handle point deductions for redemptions.

**Deliverables:**
-   Database schema for discounts.
-   Repository layer implementation.
-   Updated `RewardService`.

#### Phase 2: Business Logic & API
**Tasks:**
1.  Create `DiscountService` implementing redemption logic (Validate -> Deduct -> Issue).
2.  Implement `DiscountController` with User and Admin endpoints.
3.  Create DTOs (`DiscountTypeDto`, `RedemptionRequestDto`, `MyRedemptionDto`).
4.  Add unit tests for redemption logic.

**Deliverables:**
-   Functional API endpoints.
-   Swagger documentation.
-   Unit tests covering edge cases (insufficient points, already redeemed).

#### Phase 3: Frontend User Experience
**Tasks:**
1.  Create `DiscountCard` component.
2.  Build `AvailableDiscounts` page with grid layout.
3.  Implement `RedemptionModal` with confirmation and success states.
4.  Build `MyDiscounts` page to list active codes.
5.  Integrate with `RewardStore` to update point balance immediately after redemption.

**Deliverables:**
-   User-facing discount gallery.
-   Redemption flow UI.
-   Wallet view for codes.

#### Phase 4: Admin Management
**Tasks:**
1.  Create Admin Discount Management page.
2.  Implement Create/Edit forms for Discount Types.
3.  Add Analytics view (total redemptions, points burned).

**Deliverables:**
-   Admin CRUD interface.
-   Basic analytics dashboard.

---

## /speckit.clarify

### Questions & Answers

**Q: How are discount codes generated?**
A: For this version, we will use static codes defined in the `DiscountType` (e.g., "SAVE10") or generate unique suffixes if `IsUniqueCode` is enabled. The prompt implies standard codes like "SAVE10", so we will store the code pattern in `DiscountType`.

**Q: Do these codes automatically sync to Stripe?**
A: The system assumes the codes (e.g., "SAVE10") are already created in Stripe as Promotion Codes. The system manages the *access* to these codes via point redemption.

**Q: What happens if a user cancels the transaction?**
A: Points are deducted immediately upon clicking "Redeem" and confirming. There is no "cancel" after redemption.

**Q: Is there a limit on how many times a global code (like SAVE10) can be used by different users?**
A: Yes, `MaxRedemptions` on `DiscountType` controls global usage. `OnePerUser` controls individual usage.

---

## /speckit.analyze

### Technical Analysis

#### Backend Architecture
```
WahadiniCryptoQuest.Core/
├── Entities/
│   ├── DiscountType.cs (Id, Code, Description, Cost, Percentage, Expiry, MaxRedemptions)
│   └── UserDiscountRedemption.cs (Id, UserId, DiscountTypeId, CodeIssued, RedeemedAt, ExpiryDate)
├── Interfaces/
│   └── IDiscountService.cs

WahadiniCryptoQuest.Service/
├── Services/
│   └── DiscountService.cs (Handles validation, point deduction via RewardService, record creation)

WahadiniCryptoQuest.API/
├── Controllers/
│   └── DiscountController.cs
```

#### Database Schema
```sql
CREATE TABLE DiscountTypes (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Code VARCHAR(50) NOT NULL, -- e.g., SAVE10
    Description TEXT,
    PointCost INT NOT NULL,
    DiscountPercentage DECIMAL(5,2),
    IsActive BOOLEAN DEFAULT TRUE,
    ExpiryDate TIMESTAMP NULL,
    MaxRedemptions INT DEFAULT 0, -- 0 = unlimited
    DurationDays INT NULL -- For calculating user-specific expiry
);

CREATE TABLE UserDiscountRedemptions (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id),
    DiscountTypeId UUID NOT NULL REFERENCES DiscountTypes(Id),
    CodeIssued VARCHAR(50) NOT NULL,
    RedeemedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ExpiryDate TIMESTAMP NULL,
    IsUsed BOOLEAN DEFAULT FALSE
);
```

#### API Endpoints
-   `GET /api/discounts/available`: Returns `DiscountTypeDto[]`. Filters out expired or fully redeemed types. Checks user eligibility.
-   `POST /api/discounts/{id}/redeem`: Deducts points, returns `UserDiscountRedemptionDto` with code.
-   `GET /api/discounts/my-redeemed`: Returns history of user's redemptions.
-   `POST /api/admin/discounts`: Create new discount type.
-   `PUT /api/admin/discounts/{id}`: Update existing.
-   `GET /api/admin/discounts/analytics`: Stats on redemptions.

#### State Flow
1.  **User** views `AvailableDiscounts`.
2.  **User** clicks "Redeem" on "SAVE10" (Cost: 500).
3.  **Frontend** calls `POST /api/discounts/{id}/redeem`.
4.  **Backend**:
    -   Checks if User Points >= 500.
    -   Checks if User already redeemed "SAVE10" (if restricted).
    -   Checks global `MaxRedemptions`.
    -   **Transaction Start**:
        -   Call `RewardService.DeductPoints(userId, 500, "Redeemed SAVE10")`.
        -   Create `UserDiscountRedemption` record.
    -   **Transaction Commit**.
5.  **Backend** returns `{ code: "SAVE10", expiry: "..." }`.
6.  **Frontend** shows modal: "Success! Use code SAVE10".

---

## /speckit.checklist

### Implementation Checklist

#### Backend
-   [ ] Create `DiscountType` entity.
-   [ ] Create `UserDiscountRedemption` entity.
-   [ ] Add DbSets to `ApplicationDbContext`.
-   [ ] Create Migration `AddDiscountSystem`.
-   [ ] Implement `DiscountService.GetAvailableDiscountsAsync`.
-   [ ] Implement `DiscountService.RedeemDiscountAsync`.
-   [ ] Implement `DiscountController` endpoints.
-   [ ] Add Admin endpoints for CRUD.

#### Frontend
-   [ ] Create `DiscountType` interface/type definition.
-   [ ] Create `DiscountCard` component.
-   [ ] Create `AvailableDiscountsPage`.
-   [ ] Create `RedemptionModal` component.
-   [ ] Create `MyDiscountsPage`.
-   [ ] Add `DiscountService` to frontend API layer.

#### Testing
-   [ ] Unit Test: Redeem with insufficient points (Should Fail).
-   [ ] Unit Test: Redeem expired discount (Should Fail).
-   [ ] Unit Test: Successful redemption (Points deducted, record created).
-   [ ] Integration Test: Full flow from API to DB.

---

## /speckit.tasks

### Task Breakdown (Estimated 15-20 hours)

#### Task 1: Domain & Data Layer (3 hours)
**Description**: Set up entities and database schema.
**Subtasks**:
1.  Define `DiscountType` and `UserDiscountRedemption` entities.
2.  Configure EF Core mappings.
3.  Run migrations.
4.  Seed initial data (SAVE10, SAVE20, etc.).

#### Task 2: Backend Logic (6 hours)
**Description**: Implement service and controller logic.
**Subtasks**:
1.  Implement `DiscountService`.
2.  Integrate `IRewardService` for point deduction.
3.  Create `DiscountController`.
4.  Implement Admin CRUD operations.

#### Task 3: Frontend Implementation (6 hours)
**Description**: Build UI for browsing and redeeming.
**Subtasks**:
1.  Build `DiscountCard` UI.
2.  Implement `AvailableDiscounts` grid.
3.  Build `RedemptionModal` with copy-to-clipboard.
4.  Implement `MyDiscounts` view.

#### Task 4: Testing & Polish (3 hours)
**Description**: Verify flows and fix bugs.
**Subtasks**:
1.  Test redemption flow manually.
2.  Verify point deduction accuracy.
3.  Check mobile responsiveness of cards.

---

## /speckit.implement

### Implementation Guide

#### Step 1: Entities

**File:** `WahadiniCryptoQuest.Core/Entities/DiscountType.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.Entities;

public class DiscountType
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty; // e.g., "10% Off Monthly"

    [Required]
    public string Code { get; set; } = string.Empty; // e.g., "SAVE10"

    public string Description { get; set; } = string.Empty;

    public int PointCost { get; set; }

    public decimal DiscountPercentage { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiryDate { get; set; } // Global expiry for the campaign

    public int MaxRedemptions { get; set; } = 0; // 0 = Unlimited

    public int CurrentRedemptions { get; set; } = 0;
    
    public int DurationDays { get; set; } = 30; // How long the code is valid after redemption
}
```

**File:** `WahadiniCryptoQuest.Core/Entities/UserDiscountRedemption.cs`
```csharp
namespace WahadiniCryptoQuest.Core.Entities;

public class UserDiscountRedemption
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DiscountTypeId { get; set; }
    public string CodeIssued { get; set; } = string.Empty;
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsUsed { get; set; } = false;

    public virtual DiscountType DiscountType { get; set; } = null!;
}
```

#### Step 2: Discount Service

**File:** `WahadiniCryptoQuest.Service/Services/DiscountService.cs`
```csharp
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WahadiniCryptoQuest.Service.Services;

public class DiscountService : IDiscountService
{
    private readonly ApplicationDbContext _context;
    private readonly IRewardService _rewardService;

    public DiscountService(ApplicationDbContext context, IRewardService rewardService)
    {
        _context = context;
        _rewardService = rewardService;
    }

    public async Task<UserDiscountRedemption> RedeemDiscountAsync(Guid userId, Guid discountId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var discount = await _context.DiscountTypes
                .FirstOrDefaultAsync(d => d.Id == discountId && d.IsActive);

            if (discount == null) throw new Exception("Discount not found or inactive.");
            
            // Check global expiry
            if (discount.ExpiryDate.HasValue && discount.ExpiryDate < DateTime.UtcNow)
                throw new Exception("Discount campaign has expired.");

            // Check max redemptions
            if (discount.MaxRedemptions > 0 && discount.CurrentRedemptions >= discount.MaxRedemptions)
                throw new Exception("Maximum redemptions reached for this discount.");

            // Check if user already redeemed (if rule applies - assuming 1 per user for now)
            var existing = await _context.UserDiscountRedemptions
                .AnyAsync(r => r.UserId == userId && r.DiscountTypeId == discountId);
            if (existing) throw new Exception("You have already redeemed this discount.");

            // Deduct points (RewardService handles balance check)
            await _rewardService.DeductPointsAsync(userId, discount.PointCost, $"Redeemed {discount.Code}");

            // Create redemption record
            var redemption = new UserDiscountRedemption
            {
                UserId = userId,
                DiscountTypeId = discountId,
                CodeIssued = discount.Code,
                RedeemedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(discount.DurationDays)
            };

            _context.UserDiscountRedemptions.Add(redemption);
            
            // Update stats
            discount.CurrentRedemptions++;
            _context.DiscountTypes.Update(discount);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return redemption;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

#### Step 3: Controller

**File:** `WahadiniCryptoQuest.API/Controllers/DiscountController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WahadiniCryptoQuest.API.Controllers;

[ApiController]
[Route("api/discounts")]
[Authorize]
public class DiscountController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        // Implementation to get discounts
        return Ok();
    }

    [HttpPost("{id}/redeem")]
    public async Task<IActionResult> Redeem(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        try 
        {
            var result = await _discountService.RedeemDiscountAsync(userId, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
```
