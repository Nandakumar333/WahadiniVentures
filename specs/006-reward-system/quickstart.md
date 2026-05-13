# Quickstart Guide: Reward System Implementation

**Feature**: 006-reward-system  
**Date**: 2025-12-04  
**For**: Developers implementing the reward gamification system

## Prerequisites

- .NET 8 SDK installed
- Node.js 18+ (for frontend)
- PostgreSQL 15+ running
- Existing WahadiniCryptoQuest codebase cloned
- Branch `006-reward-system` checked out

## Phase 1: Database Setup (1 hour)

### 1.1 Create Migration

```bash
cd backend/src/WahadiniCryptoQuest.DAL
dotnet ef migrations add AddRewardSystem
```

### 1.2 Review Generated Migration

Verify migration includes:
- `RewardTransactions` table with indexes
- User table columns: `CurrentPoints`, `TotalPointsEarned`, `ReferralCode`
- `UserStreaks` table
- `UserAchievements` table
- `ReferralAttributions` table with constraints

### 1.3 Apply Migration

```bash
dotnet ef database update
```

### 1.4 Seed Test Data (Optional)

```sql
-- Generate referral codes for existing users
UPDATE "Users" 
SET "ReferralCode" = CONCAT(
    SUBSTRING(MD5(RANDOM()::TEXT), 1, 4),
    SUBSTRING(MD5(RANDOM()::TEXT), 1, 4)
)
WHERE "ReferralCode" IS NULL OR "ReferralCode" = '';
```

## Phase 2: Domain Entities (2 hours)

### 2.1 Create Base Entity (if not exists)

**File**: `backend/src/WahadiniCryptoQuest.Core/Entities/BaseEntity.cs`

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }
}
```

### 2.2 Create Entities

**Files to create**:
- `Core/Entities/RewardTransaction.cs`
- `Core/Entities/UserStreak.cs`
- `Core/Entities/UserAchievement.cs`
- `Core/Entities/ReferralAttribution.cs`
- `Core/Enums/TransactionType.cs`
- `Core/Enums/ReferralStatus.cs`

**Reference**: See `specs/006-reward-system/data-model.md` for complete entity implementations

### 2.3 Update User Entity

**File**: `Core/Entities/User.cs`

Add properties:
```csharp
public int CurrentPoints { get; private set; }
public int TotalPointsEarned { get; private set; }
public string ReferralCode { get; private set; } = null!;

// Navigation properties
public ICollection<RewardTransaction> Transactions { get; private set; } = new List<RewardTransaction>();
public ICollection<UserAchievement> Achievements { get; private set; } = new List<UserAchievement>();
public UserStreak? Streak { get; private set; }
public ICollection<ReferralAttribution> ReferralsMade { get; private set; } = new List<ReferralAttribution>();
```

Add methods:
```csharp
public void AwardPoints(int amount) { /* implementation */ }
public void DeductPoints(int amount) { /* implementation */ }
```

### 2.4 Create Achievement Definitions

**File**: `Core/Domain/AchievementCatalog.cs`

```csharp
public static class AchievementCatalog
{
    public static readonly List<AchievementDefinition> All = new()
    {
        new AchievementDefinition
        {
            Id = "FIRST_STEPS",
            Name = "First Steps",
            Description = "Complete your first lesson",
            Criteria = user => user.CompletedLessons >= 1,
            PointBonus = 10,
            Icon = "🎓",
            DisplayOrder = 1
        },
        // Add more achievements...
    };
}
```

## Phase 3: Repository Layer (2 hours)

### 3.1 Create Repository Interfaces

**File**: `Core/Interfaces/Repositories/IRewardTransactionRepository.cs`

```csharp
public interface IRewardTransactionRepository : IRepository<RewardTransaction>
{
    Task<PaginatedResult<RewardTransaction>> GetUserTransactionsAsync(
        Guid userId, 
        string? cursor, 
        int pageSize, 
        CancellationToken ct = default);
        
    Task<int> GetUserBalanceAsync(Guid userId, CancellationToken ct = default);
}
```

**Files to create**:
- `IUserStreakRepository.cs`
- `IUserAchievementRepository.cs`
- `IReferralAttributionRepository.cs`

### 3.2 Implement Repositories

**File**: `DAL/Repositories/RewardTransactionRepository.cs`

```csharp
public class RewardTransactionRepository : Repository<RewardTransaction>, IRewardTransactionRepository
{
    public RewardTransactionRepository(AppDbContext context) : base(context) { }
    
    public async Task<PaginatedResult<RewardTransaction>> GetUserTransactionsAsync(
        Guid userId, 
        string? cursor, 
        int pageSize, 
        CancellationToken ct = default)
    {
        // Implementation with cursor pagination
    }
    
    // ... other methods
}
```

**Files to create**:
- `DAL/Repositories/UserStreakRepository.cs`
- `DAL/Repositories/UserAchievementRepository.cs`
- `DAL/Repositories/ReferralAttributionRepository.cs`

### 3.3 Configure Entity Framework

**File**: `DAL/Configurations/RewardTransactionConfiguration.cs`

```csharp
public class RewardTransactionConfiguration : IEntityTypeConfiguration<RewardTransaction>
{
    public void Configure(EntityTypeBuilder<RewardTransaction> builder)
    {
        builder.ToTable("RewardTransactions");
        
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Amount).IsRequired();
        builder.Property(rt => rt.TransactionType).IsRequired().HasConversion<string>();
        builder.Property(rt => rt.Description).HasMaxLength(500);
        builder.Property(rt => rt.CreatedAt).IsRequired();
        
        builder.HasIndex(rt => new { rt.UserId, rt.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(rt => rt.TransactionType);
        
        builder.HasOne(rt => rt.User)
               .WithMany(u => u.Transactions)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Files to create**:
- `DAL/Configurations/UserStreakConfiguration.cs`
- `DAL/Configurations/UserAchievementConfiguration.cs`
- `DAL/Configurations/ReferralAttributionConfiguration.cs`

## Phase 4: Application Services (4 hours)

### 4.1 Create Service Interfaces

**File**: `Core/Interfaces/Services/IRewardService.cs`

```csharp
public interface IRewardService
{
    Task<Result<int>> AwardPointsAsync(Guid userId, int amount, TransactionType type, Guid? referenceId = null, string? description = null);
    Task<Result<int>> DeductPointsAsync(Guid userId, int amount, string reason);
    Task<Result<BalanceDto>> GetBalanceAsync(Guid userId);
    Task<Result<PaginatedResult<TransactionDto>>> GetTransactionHistoryAsync(Guid userId, string? cursor, int pageSize);
}
```

**Files to create**:
- `IStreakService.cs`
- `IAchievementService.cs`
- `ILeaderboardService.cs`
- `IReferralService.cs`

### 4.2 Implement Core Services

**File**: `Service/Rewards/RewardService.cs`

```csharp
public class RewardService : IRewardService
{
    private readonly IRewardTransactionRepository _transactionRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RewardService> _logger;
    
    public async Task<Result<int>> AwardPointsAsync(
        Guid userId, 
        int amount, 
        TransactionType type, 
        Guid? referenceId = null, 
        string? description = null)
    {
        try
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return Result<int>.Failure("User not found");
            
            user.AwardPoints(amount);
            
            var rewardTransaction = RewardTransaction.Create(
                userId, 
                amount, 
                type, 
                referenceId, 
                description,
                user.CurrentPoints
            );
            
            await _transactionRepo.AddAsync(rewardTransaction);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return Result<int>.Success(user.CurrentPoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to award points to user {UserId}", userId);
            return Result<int>.Failure("Failed to award points");
        }
    }
    
    // ... other methods
}
```

**Files to create**:
- `Service/Rewards/StreakService.cs`
- `Service/Rewards/AchievementService.cs`
- `Service/Rewards/LeaderboardService.cs`
- `Service/Rewards/ReferralService.cs`

### 4.3 Implement CQRS Commands/Queries (Optional but Recommended)

**File**: `Service/Rewards/Commands/AwardPointsCommand.cs`

```csharp
public record AwardPointsCommand(
    Guid UserId,
    int Amount,
    TransactionType Type,
    Guid? ReferenceId,
    string? Description) : IRequest<Result<int>>;

public class AwardPointsCommandHandler : IRequestHandler<AwardPointsCommand, Result<int>>
{
    private readonly IRewardService _rewardService;
    
    public async Task<Result<int>> Handle(AwardPointsCommand request, CancellationToken ct)
    {
        return await _rewardService.AwardPointsAsync(
            request.UserId, 
            request.Amount, 
            request.Type, 
            request.ReferenceId, 
            request.Description
        );
    }
}
```

## Phase 5: API Controllers (3 hours)

### 5.1 Create DTOs

**File**: `Core/DTOs/Reward/BalanceDto.cs`

```csharp
public record BalanceDto(
    int CurrentPoints,
    int TotalPointsEarned,
    RankDto? Rank
);

public record RankDto(
    int Position,
    int TotalUsers
);
```

**Files to create**:
- `TransactionDto.cs`
- `LeaderboardDto.cs`
- `AchievementDto.cs`
- `StreakDto.cs`
- `ReferralDto.cs`

### 5.2 Configure AutoMapper Profiles

**File**: `Service/Mappings/RewardMappingProfile.cs`

```csharp
public class RewardMappingProfile : Profile
{
    public RewardMappingProfile()
    {
        CreateMap<RewardTransaction, TransactionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TransactionType.ToString()));
            
        CreateMap<UserStreak, StreakDto>();
        CreateMap<UserAchievement, AchievementDto>();
        // ... other mappings
    }
}
```

### 5.3 Create Controllers

**File**: `API/Controllers/RewardsController.cs`

```csharp
[ApiController]
[Route("api/v1/rewards")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IRewardService _rewardService;
    private readonly IAchievementService _achievementService;
    private readonly IStreakService _streakService;
    
    [HttpGet("balance")]
    [ProducesResponseType(typeof(BalanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance()
    {
        var userId = User.GetUserId(); // Extension method
        var result = await _rewardService.GetBalanceAsync(userId);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : NotFound(result.Error);
    }
    
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(PaginatedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var result = await _rewardService.GetTransactionHistoryAsync(userId, cursor, pageSize);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(result.Error);
    }
    
    // ... other endpoints
}
```

**Files to create**:
- `API/Controllers/LeaderboardController.cs`
- `API/Controllers/AchievementsController.cs`
- `API/Controllers/Admin/AdminRewardsController.cs`

### 5.4 Add Validation

**File**: `API/Validators/AwardPointsRequestValidator.cs`

```csharp
public class AwardPointsRequestValidator : AbstractValidator<AwardPointsRequest>
{
    public AwardPointsRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
```

## Phase 6: Frontend Integration (6 hours)

### 6.1 Create Type Definitions

**File**: `frontend/src/types/reward.types.ts`

```typescript
export interface Balance {
  currentPoints: number;
  totalPointsEarned: number;
  rank: {
    position: number;
    totalUsers: number;
  } | null;
}

export interface Transaction {
  id: string;
  amount: number;
  type: TransactionType;
  description: string | null;
  referenceId: string | null;
  balanceAfter: number;
  createdAt: string;
}

export enum TransactionType {
  LessonCompletion = "LessonCompletion",
  TaskApproval = "TaskApproval",
  CourseCompletion = "CourseCompletion",
  DailyStreak = "DailyStreak",
  ReferralBonus = "ReferralBonus",
  AdminBonus = "AdminBonus",
  AdminPenalty = "AdminPenalty",
  Redemption = "Redemption"
}

// ... other types
```

### 6.2 Create API Service

**File**: `frontend/src/services/api/reward.service.ts`

```typescript
import { apiClient } from './client';
import { Balance, Transaction, Leaderboard, Achievement, Streak, Referral } from '@/types/reward.types';

export const rewardService = {
  async getBalance(): Promise<Balance> {
    const { data } = await apiClient.get('/rewards/balance');
    return data;
  },

  async getTransactions(cursor?: string, pageSize = 20): Promise<PaginatedResult<Transaction>> {
    const { data } = await apiClient.get('/rewards/transactions', {
      params: { cursor, pageSize }
    });
    return data;
  },

  async getLeaderboard(type: 'weekly' | 'monthly' | 'all-time', limit = 100): Promise<Leaderboard> {
    const { data } = await apiClient.get('/rewards/leaderboard', {
      params: { type, limit }
    });
    return data;
  },

  // ... other methods
};
```

### 6.3 Create React Hooks

**File**: `frontend/src/hooks/reward/useRewards.ts`

```typescript
import { useQuery } from '@tanstack/react-query';
import { rewardService } from '@/services/api/reward.service';

export const useBalance = () => {
  return useQuery({
    queryKey: ['rewards', 'balance'],
    queryFn: () => rewardService.getBalance(),
    refetchInterval: 60000, // Refresh every minute
  });
};

export const useTransactions = (cursor?: string, pageSize = 20) => {
  return useQuery({
    queryKey: ['rewards', 'transactions', cursor, pageSize],
    queryFn: () => rewardService.getTransactions(cursor, pageSize),
    keepPreviousData: true,
  });
};

// ... other hooks
```

**Files to create**:
- `hooks/reward/useLeaderboard.ts`
- `hooks/reward/useAchievements.ts`
- `hooks/reward/useStreak.ts`
- `hooks/reward/useReferrals.ts`

### 6.4 Create UI Components

**File**: `frontend/src/components/rewards/PointsDisplay.tsx`

```typescript
import React from 'react';
import { useBalance } from '@/hooks/reward/useRewards';
import { Card } from '@/components/ui/card';
import { Coins } from 'lucide-react';

export const PointsDisplay: React.FC = () => {
  const { data: balance, isLoading } = useBalance();

  if (isLoading) return <div>Loading...</div>;

  return (
    <Card className="p-4">
      <div className="flex items-center gap-3">
        <Coins className="h-8 w-8 text-yellow-500" />
        <div>
          <p className="text-2xl font-bold">{balance?.currentPoints.toLocaleString()}</p>
          <p className="text-sm text-muted-foreground">Points</p>
        </div>
      </div>
    </Card>
  );
};
```

**Files to create**:
- `components/rewards/TransactionHistory.tsx`
- `components/rewards/Leaderboard.tsx`
- `components/rewards/AchievementCard.tsx`
- `components/rewards/StreakTracker.tsx`
- `components/rewards/ReferralWidget.tsx`

### 6.5 Create Pages

**File**: `frontend/src/pages/rewards/RewardsPage.tsx`

```typescript
import React from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { PointsDisplay } from '@/components/rewards/PointsDisplay';
import { TransactionHistory } from '@/components/rewards/TransactionHistory';
import { Leaderboard } from '@/components/rewards/Leaderboard';
import { AchievementGrid } from '@/components/rewards/AchievementGrid';

export const RewardsPage: React.FC = () => {
  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-6">Rewards & Achievements</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <PointsDisplay />
        <StreakTracker />
        <ReferralWidget />
      </div>

      <Tabs defaultValue="leaderboard">
        <TabsList>
          <TabsTrigger value="leaderboard">Leaderboard</TabsTrigger>
          <TabsTrigger value="achievements">Achievements</TabsTrigger>
          <TabsTrigger value="history">Transaction History</TabsTrigger>
        </TabsList>

        <TabsContent value="leaderboard">
          <Leaderboard />
        </TabsContent>

        <TabsContent value="achievements">
          <AchievementGrid />
        </TabsContent>

        <TabsContent value="history">
          <TransactionHistory />
        </TabsContent>
      </Tabs>
    </div>
  );
};
```

## Phase 7: Event Integration (2 hours)

### 7.1 Hook into Lesson Completion

**File**: `Service/Lessons/LessonService.cs` (existing)

```csharp
public async Task<Result> CompleteLessonAsync(Guid userId, Guid lessonId)
{
    // ... existing completion logic ...
    
    // Award points
    await _rewardService.AwardPointsAsync(
        userId,
        50, // Configurable
        TransactionType.LessonCompletion,
        lessonId,
        $"Completed lesson: {lesson.Title}"
    );
    
    // Check achievements
    await _achievementService.CheckAndUnlockAsync(userId);
    
    return Result.Success();
}
```

### 7.2 Hook into Task Approval

**File**: `Service/Tasks/TaskService.cs` (existing)

```csharp
public async Task<Result> ApproveTaskSubmissionAsync(Guid submissionId)
{
    // ... existing approval logic ...
    
    // Award points
    await _rewardService.AwardPointsAsync(
        submission.UserId,
        100, // Configurable
        TransactionType.TaskApproval,
        submissionId,
        $"Task approved: {task.Title}"
    );
    
    // Check achievements
    await _achievementService.CheckAndUnlockAsync(submission.UserId);
    
    return Result.Success();
}
```

### 7.3 Hook into Login Flow

**File**: `Service/Auth/AuthService.cs` (existing)

```csharp
public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
{
    // ... existing login logic ...
    
    // Process streak
    var streakResult = await _streakService.ProcessLoginAsync(user.Id);
    
    // Return response with streak info
    return Result<AuthResponseDto>.Success(new AuthResponseDto
    {
        // ... existing fields ...
        StreakUpdate = streakResult.Value
    });
}
```

## Phase 8: Testing (4 hours)

### 8.1 Unit Tests

**File**: `tests/WahadiniCryptoQuest.Service.Tests/Rewards/RewardServiceTests.cs`

```csharp
public class RewardServiceTests
{
    [Fact]
    public async Task AwardPoints_ValidRequest_UpdatesBalanceAndCreatesTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 50;
        
        // Act
        var result = await _rewardService.AwardPointsAsync(
            userId, 
            amount, 
            TransactionType.LessonCompletion
        );
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value);
        
        var transaction = await _transactionRepo.GetLastAsync(userId);
        Assert.NotNull(transaction);
        Assert.Equal(amount, transaction.Amount);
    }
    
    [Fact]
    public async Task DeductPoints_InsufficientBalance_ReturnsFailure()
    {
        // Test negative balance prevention
    }
    
    // ... more tests
}
```

**Files to create**:
- `StreakServiceTests.cs`
- `AchievementServiceTests.cs`
- `LeaderboardServiceTests.cs`

### 8.2 Integration Tests

**File**: `tests/WahadiniCryptoQuest.API.Tests/Controllers/RewardsControllerTests.cs`

```csharp
public class RewardsControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetBalance_AuthenticatedUser_ReturnsBalance()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        
        // Act
        var response = await client.GetAsync("/api/v1/rewards/balance");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var balance = await response.Content.ReadAsAsync<BalanceDto>();
        Assert.NotNull(balance);
        Assert.True(balance.CurrentPoints >= 0);
    }
    
    // ... more tests
}
```

### 8.3 Frontend Tests

**File**: `frontend/src/components/rewards/__tests__/PointsDisplay.test.tsx`

```typescript
import { render, screen } from '@testing-library/react';
import { PointsDisplay } from '../PointsDisplay';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

describe('PointsDisplay', () => {
  it('displays user point balance', async () => {
    const queryClient = new QueryClient();
    
    render(
      <QueryClientProvider client={queryClient}>
        <PointsDisplay />
      </QueryClientProvider>
    );
    
    expect(await screen.findByText(/Points/i)).toBeInTheDocument();
  });
});
```

## Phase 9: Configuration & Deployment

### 9.1 Add Configuration

**File**: `backend/src/WahadiniCryptoQuest.API/appsettings.json`

```json
{
  "RewardSystem": {
    "PointValues": {
      "LessonCompletion": 50,
      "TaskApproval": 100,
      "CourseCompletion": 500,
      "DailyStreakBase": 5,
      "ReferralBonus": 250
    },
    "LeaderboardCache": {
      "ExpirationMinutes": 15,
      "TopUsersLimit": 100
    },
    "Streak": {
      "BonusMultiplier": 2,
      "MilestoneInterval": 7
    }
  }
}
```

### 9.2 Register Services

**File**: `API/Extensions/DependencyInjection/ServiceExtensions.cs`

```csharp
public static IServiceCollection AddRewardServices(this IServiceCollection services)
{
    services.AddScoped<IRewardService, RewardService>();
    services.AddScoped<IStreakService, StreakService>();
    services.AddScoped<IAchievementService, AchievementService>();
    services.AddScoped<ILeaderboardService, LeaderboardService>();
    services.AddScoped<IReferralService, ReferralService>();
    
    services.AddScoped<IRewardTransactionRepository, RewardTransactionRepository>();
    services.AddScoped<IUserStreakRepository, UserStreakRepository>();
    services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
    services.AddScoped<IReferralAttributionRepository, ReferralAttributionRepository>();
    
    services.AddMemoryCache();
    
    return services;
}
```

### 9.3 Update Program.cs

**File**: `API/Program.cs`

```csharp
// Add reward services
builder.Services.AddRewardServices();

// Configure AutoMapper for reward DTOs
builder.Services.AddAutoMapper(typeof(RewardMappingProfile));
```

## Verification Checklist

- [ ] Database migration applied successfully
- [ ] All entities created with proper configurations
- [ ] Repository layer tested (unit tests passing)
- [ ] Service layer tested (unit + integration tests passing)
- [ ] API endpoints documented in Swagger
- [ ] Frontend components render without errors
- [ ] Points awarded on lesson completion
- [ ] Points awarded on task approval
- [ ] Streak updates on login
- [ ] Leaderboard displays correctly
- [ ] Achievements unlock properly
- [ ] Referral codes working
- [ ] Admin endpoints secured (role-based)
- [ ] No console errors in browser
- [ ] API rate limiting configured

## Troubleshooting

### Issue: Points not awarded after lesson completion
**Solution**: Check event handler integration in LessonService. Verify transaction created in database.

### Issue: Leaderboard not updating
**Solution**: Clear cache or wait for TTL expiration (15 minutes default). Check cache configuration.

### Issue: Streak not incrementing
**Solution**: Verify UTC date comparison logic. Check LastLoginDate stored correctly.

### Issue: Achievement not unlocking
**Solution**: Verify criteria evaluation logic. Check UserAchievement table for existing records.

## Next Steps

1. **Performance Optimization**: Add database indexes monitoring
2. **Analytics**: Implement reward analytics dashboard
3. **Notifications**: Add push notifications for point awards
4. **Gamification**: Add more achievements and badges
5. **Redemption**: Implement point redemption system (separate feature)

---

**Questions?** Refer to:
- `specs/006-reward-system/spec.md` - Feature specification
- `specs/006-reward-system/data-model.md` - Entity details
- `specs/006-reward-system/contracts/api-contracts.md` - API documentation
- `specs/006-reward-system/research.md` - Technical decisions

**Status**: Quickstart guide complete. Ready for implementation.
