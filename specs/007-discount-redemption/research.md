# Research & Technical Decisions: Point-Based Discount Redemption System

**Feature**: 007-discount-redemption  
**Date**: December 5, 2025  
**Status**: Phase 0 Complete

## Overview

This document captures research findings and technical decisions made to resolve implementation uncertainties for the discount redemption system. All decisions align with WahadiniCryptoQuest's Clean Architecture principles and existing technology stack.

---

## Ambiguity Resolution

This section clarifies ambiguous requirements identified during design review.

### IsUsed Tracking Implementation

**Ambiguity**: Should IsUsed be updated manually or automatically via Stripe webhooks?

**Resolution**: **Phase 1 = Manual placeholder, Phase 2 = Stripe webhooks**
- Phase 1: IsUsed remains false (field exists but not actively updated)
- Phase 2 Enhancement: Implement Stripe webhook listener for `promotion_code.redeemed` event
- Rationale: Core redemption flow doesn't depend on usage tracking; this is analytics enhancement

### CurrentRedemptions Increment Timing

**Ambiguity**: Should CurrentRedemptions be incremented before or after transaction commit?

**Resolution**: **Increment within transaction, before commit**
```csharp
// Within database transaction
discount.IncrementRedemptions(); // Increments counter
await _dbContext.SaveChangesAsync(); // Persists atomically
await transaction.CommitAsync(); // Commits all or rolls back all
```
- Rationale: Must be part of atomic transaction to ensure consistency
- If commit fails, rollback automatically reverts counter increment

### Discount Percentage Application

**Ambiguity**: Does our system calculate the discount amount or does Stripe?

**Resolution**: **Stripe handles all discount calculations**
- Our system only stores `DiscountPercentage` for display purposes
- Stripe Promotion Code configuration determines actual discount applied
- Admin must ensure Stripe Promotion Code percentage matches our stored value
- Rationale: Single source of truth (Stripe) prevents calculation mismatches

### Code Expiry Enforcement

**Ambiguity**: Does system prevent displaying expired codes or does Stripe reject at checkout?

**Resolution**: **Both layers enforce expiry**
- System: Filters expired codes from "My Discounts" view (visual indicator)
- System: Does NOT prevent copying expired codes (user may still try)
- Stripe: Rejects expired codes at checkout as final validation
- Rationale: UX guidance from system, security enforcement from Stripe

### Unique Code Collision Prevention

**Ambiguity**: How does the system prevent unique code collisions?

**Resolution**: **GUID-based suffixes provide probabilistic uniqueness**
```csharp
public string GenerateCode(Guid redemptionId)
{
    if (!IsUniqueCode) return Code;
    
    // Last 8 characters of GUID = 4.3 billion combinations
    var suffix = redemptionId.ToString("N")[^8..].ToUpper();
    return $"{Code}-{suffix}"; // e.g., "SAVE10-A3B2C1D4"
}
```
- Collision probability: 1 in 4.3 billion for same base code
- Database unique constraint on CodeIssued provides final safety net
- If collision occurs (extremely rare), transaction fails and user retries with new GUID

### Audit Retention Strategy

**Ambiguity**: How long are audit records retained and what's the archival strategy?

**Resolution**: **Permanent retention with time-based partitioning**
- All RewardTransaction records retained indefinitely (regulatory compliance)
- PostgreSQL time-based partitioning (monthly partitions) for performance
- Old partitions can be moved to cheaper storage (e.g., S3) but remain queryable
- No automatic deletion; manual archival only after legal review
- Rationale: Financial audit trail must be immutable and permanent



### 1. Concurrency Control Strategy

**Question**: How to prevent double-spending of points when multiple redemption requests occur simultaneously?

**Decision**: Implement **Optimistic Concurrency Control** using Entity Framework Core's concurrency tokens

**Rationale**:
- PostgreSQL supports row-level locking and optimistic concurrency via `xmin` or timestamp columns
- Entity Framework Core provides built-in support via `[ConcurrencyCheck]` or `[Timestamp]` attributes
- Optimistic approach preferred over pessimistic locking for better throughput in read-heavy workload
- User entities already include `RowVersion` property suitable for concurrency checks

**Alternatives Considered**:
- **Pessimistic Locking**: Would require explicit `SELECT FOR UPDATE` locks, reducing concurrent throughput
- **Distributed Lock (Redis)**: Adds external dependency, overkill for single-database architecture
- **Database Constraints**: Check constraints can't prevent race conditions between validation and insert

**Implementation Pattern**:
```csharp
// User entity (existing)
public class User : BaseEntity
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
    public int PointsBalance { get; set; }
}

// In DiscountService.RedeemDiscountAsync
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try
{
    var user = await _userRepository.GetByIdAsync(userId);
    
    // Validation checks...
    
    // Point deduction with concurrency check
    user.DeductPoints(discount.PointCost); // Domain method
    await _userRepository.UpdateAsync(user); // Will throw DbUpdateConcurrencyException if stale
    
    // Create redemption record
    var redemption = UserDiscountRedemption.Create(user, discount);
    await _discountRepository.AddRedemptionAsync(redemption);
    
    await transaction.CommitAsync();
}
catch (DbUpdateConcurrencyException)
{
    await transaction.RollbackAsync();
    throw new ConcurrencyException("Points balance changed. Please retry.");
}
```

---

### 2. Stripe Integration Pattern

**Question**: How to validate Stripe Promotion Codes and track their usage?

**Decision**: **Pre-validation approach** - Codes must exist in Stripe before being configured in the system

**Rationale**:
- Feature specification explicitly states codes are "pre-created in Stripe as Promotion Codes"
- System manages access control (who can obtain codes) not code creation
- Avoids complexity of Stripe API integration for code generation
- Admin manually creates codes in Stripe dashboard, then references them in discount configuration

**Alternatives Considered**:
- **Programmatic Code Creation**: Would require Stripe API integration, webhooks for status tracking - marked as out of scope
- **Dynamic Code Generation**: System generates and syncs to Stripe on-the-fly - adds significant complexity
- **No Validation**: Trust admin input - risky, could lead to invalid codes being redeemed

**Implementation Pattern**:
```csharp
// Admin creates discount type
public class CreateDiscountTypeCommand
{
    public string StripePromotionCode { get; set; } // e.g., "SAVE10"
    public int PointCost { get; set; }
    // ...
}

// Optional: Validate code exists in Stripe during creation (Phase 2 enhancement)
public async Task<bool> ValidateStripeCodeAsync(string code)
{
    try
    {
        var stripeService = new Stripe.PromotionCodeService();
        var codes = await stripeService.ListAsync(new Stripe.PromotionCodeListOptions
        {
            Code = code,
            Active = true
        });
        return codes.Data.Any();
    }
    catch
    {
        return false; // Stripe API unavailable, log warning but don't block
    }
}
```

**Tracking Code Usage**: Out of scope for v1. Future enhancement would use Stripe webhooks to mark `UserDiscountRedemption.IsUsed = true` when code is applied to a subscription.

---

### 3. Transaction Boundary Design

**Question**: What should be included in the atomic redemption transaction?

**Decision**: Single database transaction encompassing:
1. User points validation and deduction
2. Discount availability validation (expiry, max redemptions)
3. User eligibility validation (one-per-user check)
4. Creation of `UserDiscountRedemption` record
5. Creation of negative `RewardTransaction` audit entry
6. Increment `DiscountType.CurrentRedemptions` counter

**Rationale**:
- Ensures ACID properties for all redemption-related state changes
- Prevents partial redemptions (points deducted but code not issued)
- Audit trail creation is mandatory per specification requirements
- Counter increment within transaction ensures accurate max redemption enforcement

**Alternatives Considered**:
- **Separate Audit Transaction**: Would create inconsistency window where redemption exists but audit doesn't
- **Eventual Consistency**: Not acceptable per specification requirement for "atomic transaction"
- **Two-Phase Commit**: Overkill for single-database architecture

**Implementation Pattern**:
```csharp
public async Task<Result<RedemptionResponseDto>> RedeemDiscountAsync(Guid userId, Guid discountTypeId)
{
    await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
    
    try
    {
        // 1. Fetch and lock user (optimistic concurrency via RowVersion)
        var user = await _userRepository.GetByIdAsync(userId);
        
        // 2. Fetch and validate discount
        var discount = await _discountRepository.GetByIdWithRedemptionsAsync(discountTypeId);
        if (!discount.IsAvailable()) return Result.Fail("Discount unavailable");
        
        // 3. Validate user eligibility
        if (user.PointsBalance < discount.PointCost) return Result.Fail("Insufficient points");
        if (discount.OnePerUser && await _discountRepository.HasUserRedeemedAsync(userId, discountTypeId))
            return Result.Fail("Already redeemed");
        
        // 4. Check global redemption limit
        if (discount.MaxRedemptions > 0 && discount.CurrentRedemptions >= discount.MaxRedemptions)
            return Result.Fail("Offer fully redeemed");
        
        // 5. Deduct points (domain method updates RowVersion)
        user.DeductPoints(discount.PointCost);
        
        // 6. Create redemption record
        var redemption = UserDiscountRedemption.Create(userId, discountTypeId, discount.Code);
        await _discountRepository.AddRedemptionAsync(redemption);
        
        // 7. Create audit trail
        var auditEntry = RewardTransaction.CreateDeduction(userId, discount.PointCost, $"Redeemed {discount.Name}");
        await _rewardRepository.AddTransactionAsync(auditEntry);
        
        // 8. Increment counter
        discount.IncrementRedemptions();
        
        // 9. Save all changes (will throw DbUpdateConcurrencyException if stale)
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return Result.Ok(new RedemptionResponseDto { Code = discount.Code, ExpiryDate = redemption.ExpiryDate });
    }
    catch (DbUpdateConcurrencyException ex)
    {
        await transaction.RollbackAsync();
        return Result.Fail("Redemption conflict. Please retry.");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Redemption failed for user {UserId}, discount {DiscountId}", userId, discountTypeId);
        return Result.Fail("Redemption failed. Please try again.");
    }
}
```

---

### 4. Discount Code Generation Strategy

**Question**: Should discount codes be static (same for all users) or unique per redemption?

**Decision**: **Hybrid approach** - Support both static and unique codes via `IsUniqueCode` flag

**Rationale**:
- Specification mentions both "static codes like SAVE10" and "generation of unique codes"
- Static codes are simpler and sufficient for most use cases (Stripe handles single-use enforcement)
- Unique codes provide additional tracking and prevent sharing but add complexity
- Hybrid approach provides flexibility for different campaign types

**Alternatives Considered**:
- **Static Only**: Simpler but less flexible for anti-fraud campaigns
- **Unique Only**: More secure but overhead for all campaigns including low-risk ones
- **Admin Choice Per Campaign**: Selected approach - best of both worlds

**Implementation Pattern**:
```csharp
public class DiscountType : BaseEntity
{
    public string Code { get; set; } // Base code or pattern (e.g., "SAVE10")
    public bool IsUniqueCode { get; set; } // If true, append unique suffix
    
    public string GenerateCode(Guid redemptionId)
    {
        if (!IsUniqueCode) return Code;
        
        // Generate unique suffix: CODE-XXXXX (last 8 chars of GUID)
        var suffix = redemptionId.ToString("N")[^8..].ToUpper();
        return $"{Code}-{suffix}";
    }
}

// In redemption flow
var redemptionId = Guid.NewGuid();
var codeIssued = discount.GenerateCode(redemptionId);
var redemption = new UserDiscountRedemption
{
    Id = redemptionId,
    UserId = userId,
    DiscountTypeId = discountTypeId,
    CodeIssued = codeIssued, // Either "SAVE10" or "SAVE10-A3B2C1D4"
    RedeemedAt = DateTime.UtcNow
};
```

**Note**: For Phase 1, implement static codes only. Unique code generation can be added in Phase 2 if needed.

---

### 8. Retry Logic and Error Recovery

**Question**: How should the system handle transient failures and when should operations be retried?

**Decision**: Implement **layered retry strategy** with exponential backoff for different failure types

**Rationale**:
- Transient database failures (network blips, deadlocks) should be retried automatically
- Stripe API calls may fail due to rate limits or temporary unavailability
- User-facing redemptions should fail fast with clear errors (no automatic retry to prevent confusion)
- Background validation operations can retry more aggressively

**Retry Policies**:

1. **Database Transient Failures**: 3 retries with 1-second intervals
   ```csharp
   var retryPolicy = Policy
       .Handle<DbUpdateException>()
       .Or<TimeoutException>()
       .WaitAndRetryAsync(3, retryAttempt => 
           TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
   ```

2. **Stripe API Validation** (Admin discount creation): 3 retries with exponential backoff, 10-second timeout
   ```csharp
   var stripePolicy = Policy
       .Handle<StripeException>()
       .Or<HttpRequestException>()
       .WaitAndRetryAsync(3, retryAttempt => 
           TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
       onRetry: (exception, timeSpan, retryCount, context) => {
           _logger.LogWarning($"Stripe API retry {retryCount}/3 after {timeSpan}");
       });
   
   var timeoutPolicy = Policy.TimeoutAsync(10, TimeoutStrategy.Pessimistic);
   var combinedPolicy = Policy.WrapAsync(stripePolicy, timeoutPolicy);
   ```

3. **User Redemption Requests**: NO automatic retry - fail fast with actionable error
   - Concurrency conflicts: Return 409 with "Please retry" message (let user decide)
   - Insufficient points: Return 400 immediately (no point in retrying)
   - Discount unavailable: Return 400 immediately (race condition detected)

4. **Frontend Retry**: User-initiated only via React Query
   ```typescript
   const { mutate, isLoading } = useMutation({
     mutationFn: redeemDiscount,
     retry: false, // User must explicitly retry on failure
     onError: (error) => {
       if (error.code === 'CONCURRENCY_CONFLICT') {
         toast.error('Points balance changed. Please try again.');
       }
     }
   });
   ```

**Transaction Rollback Handling**:
```csharp
try
{
    await using var transaction = await _dbContext.Database.BeginTransactionAsync();
    // ... redemption logic ...
    await transaction.CommitAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Explicit rollback for clarity (automatic on exception but good practice)
    await transaction.RollbackAsync();
    _logger.LogWarning(ex, "Concurrency conflict during redemption");
    return Result.Fail<RedemptionResponseDto>("CONCURRENCY_CONFLICT", 
        "Your points balance has changed. Please retry.");
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    _logger.LogError(ex, "Redemption transaction failed");
    return Result.Fail<RedemptionResponseDto>("TRANSACTION_FAILED", 
        "Redemption failed. Please try again later.");
}
```

---

### 9. Timezone Handling Strategy

**Question**: How should the system handle timezone differences for expiry dates and redemption timestamps?

**Decision**: **UTC storage with client-side timezone conversion**

**Rationale**:
- All timestamps stored in UTC in database (PostgreSQL `timestamp with time zone`)
- Server always performs comparisons in UTC to ensure consistency
- Frontend converts to user's browser timezone for display only
- Prevents ambiguity during daylight saving time transitions

**Implementation Pattern**:

**Backend (UTC only)**:
```csharp
public class DiscountType
{
    public DateTime? ExpiryDate { get; set; } // Always UTC in database
    
    public bool IsExpired()
    {
        // All comparisons in UTC
        return ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }
}

public class UserDiscountRedemption
{
    public DateTime RedeemedAt { get; set; } // UTC
    public DateTime ExpiryDate { get; set; } // UTC
    
    public bool IsValid()
    {
        return ExpiryDate > DateTime.UtcNow; // UTC comparison
    }
}

// EF Core configuration ensures UTC storage
public class DiscountTypeConfiguration : IEntityTypeConfiguration<DiscountType>
{
    public void Configure(EntityTypeBuilder<DiscountType> builder)
    {
        builder.Property(d => d.ExpiryDate)
            .HasColumnType("timestamp with time zone");
    }
}
```

**Frontend (display conversion)**:
```typescript
interface DiscountTypeDto {
  expiryDate: string; // ISO 8601 UTC string from API
}

function DiscountCard({ discount }: { discount: DiscountTypeDto }) {
  // Convert to user's local timezone for display
  const localExpiry = new Date(discount.expiryDate);
  const formattedExpiry = localExpiry.toLocaleString();
  
  // Calculate days until expiry (using UTC for accuracy)
  const now = new Date();
  const daysUntilExpiry = Math.floor(
    (localExpiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24)
  );
  
  return (
    <div>
      <p>Expires: {formattedExpiry}</p>
      <p>({daysUntilExpiry} days remaining)</p>
    </div>
  );
}
```

**Campaign Expiry Edge Case**:
- Admin sets expiry to "2026-12-31" (intended as end of day)
- System stores as "2026-12-31T23:59:59Z" UTC
- User in UTC-8 timezone sees expiry as "2026-12-31 3:59 PM" local time
- Comparison still performed in UTC server-side for consistency

---

### 10. Accessibility Implementation

**Question**: What specific accessibility requirements must be implemented for WCAG 2.1 Level AA compliance?

**Decision**: Implement **comprehensive keyboard navigation, ARIA support, and focus management**

**Rationale**:
- Platform requirement: WCAG 2.1 Level AA compliance for all features
- Redemption flow involves critical financial transactions requiring accessible UX
- Keyboard-only users must be able to browse, select, and redeem discounts
- Screen reader users need proper announcements for state changes

**Implementation Requirements**:

1. **Keyboard Navigation**:
   - All discount cards: Focusable with `tabindex="0"`, Enter key to select
   - Redeem buttons: Native `<button>` elements (not `<div>` with click handlers)
   - Modal dialogs: Trap focus, Escape key to close, Return focus on close
   - Copy code button: Space/Enter to activate

2. **Focus Management**:
   ```tsx
   function RedemptionModal({ discount, onClose }: Props) {
     const firstFocusRef = useRef<HTMLButtonElement>(null);
     const lastFocusRef = useRef<HTMLButtonElement>(null);
     
     useEffect(() => {
       // Focus first element when modal opens
       firstFocusRef.current?.focus();
       
       // Trap focus within modal
       const handleTab = (e: KeyboardEvent) => {
         if (e.key === 'Tab') {
           if (e.shiftKey && document.activeElement === firstFocusRef.current) {
             e.preventDefault();
             lastFocusRef.current?.focus();
           } else if (!e.shiftKey && document.activeElement === lastFocusRef.current) {
             e.preventDefault();
             firstFocusRef.current?.focus();
           }
         } else if (e.key === 'Escape') {
           onClose();
         }
       };
       
       document.addEventListener('keydown', handleTab);
       return () => document.removeEventListener('keydown', handleTab);
     }, [onClose]);
   }
   ```

3. **ARIA Attributes**:
   ```tsx
   <button
     onClick={handleRedeem}
     aria-label={`Redeem ${discount.name} for ${discount.pointCost} points`}
     aria-describedby="discount-description"
   >
     Redeem
   </button>
   
   <div
     role="status"
     aria-live="polite"
     aria-atomic="true"
     className="sr-only"
   >
     {isLoading && 'Processing redemption...'}
     {isSuccess && 'Discount redeemed successfully!'}
     {isError && `Redemption failed: ${error.message}`}
   </div>
   ```

4. **Visual Focus Indicators** (TailwindCSS):
   ```tsx
   <button className="
     focus:outline-none 
     focus:ring-2 
     focus:ring-offset-2 
     focus:ring-blue-500
     transition-shadow
   ">
     Redeem
   </button>
   ```

5. **Screen Reader Announcements**:
   ```tsx
   import { useAnnouncer } from '@/hooks/useAnnouncer';
   
   function DiscountGallery() {
     const announce = useAnnouncer();
     
     const handleRedeemSuccess = (code: string) => {
       announce(`Success! Your discount code is ${code}. Code copied to clipboard.`, 'assertive');
     };
     
     const handleRedeemError = (message: string) => {
       announce(`Redemption failed. ${message}`, 'assertive');
     };
   }
   ```

6. **Alt Text for Images**:
   ```tsx
   <img 
     src={discount.imageUrl} 
     alt={`${discount.name} - ${discount.discountPercentage}% discount`}
   />
   ```


---

### 5. Frontend State Management

**Question**: Where to manage discount state - React Query, Zustand, or both?

**Decision**: **React Query for server state, Zustand for UI state only**

**Rationale**:
- React Query already used for server state management in existing features (courses, lessons)
- Provides automatic caching, background refetching, and optimistic updates
- Zustand better suited for client-side UI state (modals, filters, selections)
- Avoids state duplication and keeps architecture consistent

**Alternatives Considered**:
- **Zustand Only**: Would require manual cache invalidation and refetch logic
- **React Query Only**: Awkward for pure UI state like modal visibility
- **Redux Toolkit**: Overkill for this feature, inconsistent with existing patterns

**Implementation Pattern**:
```typescript
// React Query for server state
export const useAvailableDiscounts = () => {
  return useQuery({
    queryKey: ['discounts', 'available'],
    queryFn: () => discountService.getAvailable(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    refetchOnWindowFocus: true,
  });
};

export const useRedeemDiscount = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (discountId: string) => discountService.redeem(discountId),
    onSuccess: () => {
      // Invalidate and refetch
      queryClient.invalidateQueries({ queryKey: ['discounts', 'available'] });
      queryClient.invalidateQueries({ queryKey: ['discounts', 'my-redemptions'] });
      queryClient.invalidateQueries({ queryKey: ['user', 'points'] });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
};

// Zustand for UI state (optional, only if needed)
interface DiscountUIState {
  selectedDiscount: DiscountType | null;
  isRedemptionModalOpen: boolean;
  setSelectedDiscount: (discount: DiscountType | null) => void;
  openRedemptionModal: () => void;
  closeRedemptionModal: () => void;
}

export const useDiscountUIStore = create<DiscountUIState>((set) => ({
  selectedDiscount: null,
  isRedemptionModalOpen: false,
  setSelectedDiscount: (discount) => set({ selectedDiscount: discount }),
  openRedemptionModal: () => set({ isRedemptionModalOpen: true }),
  closeRedemptionModal: () => set({ isRedemptionModalOpen: false, selectedDiscount: null }),
}));
```

---

### 6. Reward Points Synchronization

**Question**: How to keep frontend point balance in sync after redemption?

**Decision**: **React Query automatic invalidation + optimistic updates**

**Rationale**:
- React Query's `invalidateQueries` automatically refetches user points after mutation
- Optimistic updates provide instant UI feedback before server confirmation
- No manual state synchronization required
- Existing reward store can be simplified or deprecated in favor of React Query

**Implementation Pattern**:
```typescript
export const useRedeemDiscount = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ discountId, pointCost }: { discountId: string; pointCost: number }) =>
      discountService.redeem(discountId),
    
    // Optimistic update
    onMutate: async ({ pointCost }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['user', 'points'] });
      
      // Snapshot current value
      const previousPoints = queryClient.getQueryData(['user', 'points']);
      
      // Optimistically update
      queryClient.setQueryData(['user', 'points'], (old: number) => old - pointCost);
      
      return { previousPoints };
    },
    
    // Rollback on error
    onError: (err, variables, context) => {
      queryClient.setQueryData(['user', 'points'], context?.previousPoints);
    },
    
    // Refetch on success to ensure accuracy
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user', 'points'] });
      queryClient.invalidateQueries({ queryKey: ['discounts', 'available'] });
    },
  });
};
```

---

### 7. Admin Analytics Implementation

**Question**: Real-time analytics or batch-computed statistics?

**Decision**: **Direct query approach with indexed columns** for Phase 1, optimize later if needed

**Rationale**:
- Expected volume (1000 redemptions/day, 100 discount types) is manageable with direct queries
- PostgreSQL aggregations are efficient with proper indexes
- Premature optimization should be avoided (YAGNI principle)
- Can migrate to materialized views or dedicated analytics tables in Phase 2 if performance degrades

**Implementation Pattern**:
```csharp
public async Task<DiscountAnalyticsDto> GetAnalyticsAsync(Guid discountTypeId)
{
    var stats = await _dbContext.UserDiscountRedemptions
        .Where(r => r.DiscountTypeId == discountTypeId)
        .GroupBy(r => 1) // Dummy group for aggregation
        .Select(g => new
        {
            TotalRedemptions = g.Count(),
            UniqueUsers = g.Select(r => r.UserId).Distinct().Count(),
            TotalPointsBurned = g.Sum(r => r.DiscountType.PointCost)
        })
        .FirstOrDefaultAsync();
    
    return new DiscountAnalyticsDto
    {
        DiscountTypeId = discountTypeId,
        TotalRedemptions = stats?.TotalRedemptions ?? 0,
        UniqueUsers = stats?.UniqueUsers ?? 0,
        TotalPointsBurned = stats?.TotalPointsBurned ?? 0
    };
}

// Index for performance
// Migration: CreateIndex("IX_UserDiscountRedemptions_DiscountTypeId_RedeemedAt", "UserDiscountRedemptions", new[] { "DiscountTypeId", "RedeemedAt" });
```

**Future Optimization**: If analytics queries slow down:
- Implement materialized view refreshed hourly
- Use PostgreSQL's `REFRESH MATERIALIZED VIEW CONCURRENTLY`
- Store aggregates in dedicated `DiscountStatistics` table updated via triggers

---

## Technology Choices & Best Practices

### Entity Framework Core Patterns

**Repository Pattern**: Use thin repository layer wrapping DbContext
- Abstracts EF Core specifics from service layer
- Enables unit testing with in-memory repository implementations
- Follows existing codebase patterns

**Unit of Work**: Leverage DbContext as implicit Unit of Work
- No separate UoW class needed (EF Core's DbContext already implements pattern)
- Transaction management via `DbContext.Database.BeginTransactionAsync()`

### MediatR CQRS Pattern

**Commands**: Redemption, Create Discount Type, Update Discount Type
```csharp
public class RedeemDiscountCommand : IRequest<Result<RedemptionResponseDto>>
{
    public Guid UserId { get; set; }
    public Guid DiscountTypeId { get; set; }
}

public class RedeemDiscountCommandHandler : IRequestHandler<RedeemDiscountCommand, Result<RedemptionResponseDto>>
{
    // Implementation
}
```

**Queries**: Get Available Discounts, Get User Redemptions, Get Analytics
```csharp
public class GetAvailableDiscountsQuery : IRequest<Result<List<DiscountTypeDto>>>
{
    public Guid UserId { get; set; } // For eligibility filtering
}
```

### FluentValidation Rules

**Redemption Validation**:
- DiscountTypeId must be valid GUID
- User must be authenticated
- (Business validation happens in service layer, not validator)

**Create Discount Type Validation**:
- Name required, max 100 chars
- Code required, max 50 chars, alphanumeric with hyphens
- PointCost must be positive integer
- DiscountPercentage between 0 and 100
- DurationDays must be positive if provided

---

## Security Considerations

### Authorization

**User Endpoints**:
- Require `[Authorize]` attribute
- User can only redeem for themselves (UserId extracted from JWT claims)
- User can only view their own redemptions

**Admin Endpoints**:
- Require `[Authorize(Policy = "RequireAdmin")]`
- Full CRUD access to discount types
- Access to analytics for all users

### Input Validation

**SQL Injection**: Mitigated by EF Core parameterized queries
**Mass Assignment**: Use separate DTOs for input validation
**XSS**: Frontend sanitizes user input (not applicable to this feature - no user-generated content)

### Rate Limiting (Future Enhancement)

Consider implementing rate limiting on redemption endpoint to prevent abuse:
- Max 10 redemptions per user per hour
- Implement via ASP.NET Core middleware or Redis-based rate limiter

---

## Performance Optimization

### Database Indexes

**Required Indexes**:
```sql
-- Lookup discounts by activity and expiry
CREATE INDEX IX_DiscountTypes_IsActive_ExpiryDate ON DiscountTypes(IsActive, ExpiryDate);

-- User redemption history lookup
CREATE INDEX IX_UserDiscountRedemptions_UserId_RedeemedAt ON UserDiscountRedemptions(UserId, RedeemedAt DESC);

-- Check if user already redeemed specific discount
CREATE INDEX IX_UserDiscountRedemptions_UserId_DiscountTypeId ON UserDiscountRedemptions(UserId, DiscountTypeId);

-- Analytics queries
CREATE INDEX IX_UserDiscountRedemptions_DiscountTypeId_RedeemedAt ON UserDiscountRedemptions(DiscountTypeId, RedeemedAt);
```

### Query Optimization

**AsNoTracking for Read-Only Queries**:
```csharp
public async Task<List<DiscountTypeDto>> GetAvailableDiscountsAsync()
{
    return await _dbContext.DiscountTypes
        .AsNoTracking() // Read-only, no change tracking overhead
        .Where(d => d.IsActive && 
                    (d.ExpiryDate == null || d.ExpiryDate > DateTime.UtcNow) &&
                    (d.MaxRedemptions == 0 || d.CurrentRedemptions < d.MaxRedemptions))
        .ProjectTo<DiscountTypeDto>(_mapper.ConfigurationProvider) // Project in database
        .ToListAsync();
}
```

---

## Testing Strategy

### Unit Tests

**DiscountService Tests**:
- Redeem with sufficient points → Success
- Redeem with insufficient points → Failure
- Redeem expired discount → Failure
- Redeem when max redemptions reached → Failure
- Redeem when user already redeemed (one-per-user) → Failure
- Concurrent redemption attempts → Only one succeeds

**Domain Entity Tests**:
- DiscountType.IsAvailable() logic
- DiscountType.GenerateCode() for static and unique variants
- UserDiscountRedemption.Create() factory method

### Integration Tests

**API Tests**:
- POST /api/discounts/{id}/redeem → 200 with code
- POST /api/discounts/{id}/redeem (insufficient points) → 400 error
- GET /api/discounts/available → 200 with filtered list
- GET /api/discounts/my-redemptions → 200 with user's codes

**Concurrency Tests**:
- Simulate 50 concurrent redemption requests for same discount
- Verify only valid redemptions succeed
- Verify point balances are accurate
- Verify no duplicate redemptions per user

### E2E Tests (Playwright)

**User Flow**:
1. Navigate to discounts page
2. View available discounts
3. Click "Redeem" on eligible discount
4. Confirm redemption in modal
5. Verify success message with code
6. Navigate to "My Discounts"
7. Verify code appears in wallet
8. Copy code to clipboard

---

## Migration Plan

### Database Migration Steps

1. **Create Migration**: `dotnet ef migrations add AddDiscountSystem`
2. **Review Generated SQL**: Verify indexes, constraints, foreign keys
3. **Test on Development**: Run migration on dev database
4. **Seed Initial Data**: Add 3-5 sample discount types for testing
5. **Deploy to Staging**: Test redemption flow end-to-end
6. **Deploy to Production**: Execute migration with monitoring

### Seed Data Example

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<DiscountType>().HasData(
        new DiscountType
        {
            Id = Guid.NewGuid(),
            Name = "10% Off Monthly Subscription",
            Code = "SAVE10",
            Description = "Get 10% off your next monthly subscription payment",
            PointCost = 500,
            DiscountPercentage = 10,
            IsActive = true,
            MaxRedemptions = 0, // Unlimited
            DurationDays = 30,
            CreatedAt = DateTime.UtcNow
        },
        new DiscountType
        {
            Id = Guid.NewGuid(),
            Name = "25% Off Annual Subscription",
            Code = "SAVE25",
            Description = "Get 25% off your annual subscription",
            PointCost = 2000,
            DiscountPercentage = 25,
            IsActive = true,
            MaxRedemptions = 100, // Limited offer
            DurationDays = 90,
            CreatedAt = DateTime.UtcNow
        }
    );
}
```

---

## Open Questions & Future Enhancements

### Resolved Questions

All NEEDS CLARIFICATION markers from specification have been resolved through research.

### Future Enhancements (Out of Current Scope)

1. **Stripe Webhook Integration**: Track when redeemed codes are actually used at checkout
2. **Email Notifications**: Send email with code upon successful redemption
3. **Expiry Reminders**: Notify users when codes are about to expire
4. **Gift Codes**: Allow users to transfer codes to other users
5. **Dynamic Pricing**: Adjust point costs based on demand or user behavior
6. **A/B Testing**: Test different discount offers to optimize conversion
7. **Rate Limiting**: Prevent redemption abuse via rate limits

---

## Conclusion

All technical uncertainties have been resolved with decisions that align with existing architecture patterns and meet specification requirements. The research provides a solid foundation for Phase 1 (Design & Contracts) and subsequent implementation phases.

**Next Steps**: Proceed to Phase 1 - Generate data-model.md and API contracts.
