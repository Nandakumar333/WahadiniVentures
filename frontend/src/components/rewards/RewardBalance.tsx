import { useRewardBalance } from '@/hooks/reward';
import { Badge } from '@/components/ui/badge';
import { Coins, TrendingUp, Loader2, AlertCircle } from 'lucide-react';

/**
 * RewardBalance Component
 * 
 * Displays user's current reward points in the navbar
 * Features:
 * - Real-time balance display
 * - Loading state with spinner
 * - Error state with retry option
 * - Responsive design (collapses points text on mobile)
 * - Animated coin icon
 * - Hover effects
 * 
 * @example
 * ```tsx
 * // In Header/Navbar component
 * <Header>
 *   <Logo />
 *   <Nav />
 *   <RewardBalance />
 *   <UserMenu />
 * </Header>
 * ```
 */
export function RewardBalance() {
  const { data: balance, isLoading, error, refetch } = useRewardBalance();

  // Loading state
  if (isLoading) {
    return (
      <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-muted/50">
        <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
        <span className="text-sm font-medium text-muted-foreground hidden sm:inline">
          Loading...
        </span>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <button
        onClick={() => refetch()}
        className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-destructive/10 hover:bg-destructive/20 transition-colors"
        title="Click to retry"
      >
        <AlertCircle className="h-4 w-4 text-destructive" />
        <span className="text-sm font-medium text-destructive hidden sm:inline">
          Retry
        </span>
      </button>
    );
  }

  // Success state with balance display
  if (!balance) return null;

  const formattedPoints = balance.currentPoints.toLocaleString();
  const hasRank = balance.rank > 0;

  return (
    <div className="flex items-center gap-2">
      {/* Main balance badge */}
      <Badge
        variant="secondary"
        className="flex items-center gap-2 px-3 py-1.5 cursor-default hover:scale-105 transition-transform"
      >
        <Coins className="h-4 w-4 text-amber-500 animate-pulse" />
        <span className="font-semibold text-foreground">
          {formattedPoints}
        </span>
        <span className="text-muted-foreground hidden sm:inline">points</span>
      </Badge>

      {/* Rank badge (if user has rank) */}
      {hasRank && (
        <Badge
          variant="outline"
          className="flex items-center gap-1.5 px-2 py-1"
          title={`Global rank: #${balance.rank}`}
        >
          <TrendingUp className="h-3.5 w-3.5 text-green-500" />
          <span className="text-xs font-medium">#{balance.rank}</span>
        </Badge>
      )}
    </div>
  );
}

/**
 * RewardBalanceCompact Component
 * 
 * Compact version for mobile or sidebar navigation
 * Shows only the coin icon and points value
 */
export function RewardBalanceCompact() {
  const { data: balance, isLoading, error } = useRewardBalance();

  if (isLoading) {
    return <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />;
  }

  if (error || !balance) {
    return <AlertCircle className="h-4 w-4 text-destructive" />;
  }

  return (
    <div className="flex items-center gap-1.5">
      <Coins className="h-4 w-4 text-amber-500" />
      <span className="text-sm font-semibold">{balance.currentPoints.toLocaleString()}</span>
    </div>
  );
}

/**
 * RewardBalanceDetailed Component
 * 
 * Detailed version for profile pages or dashboards
 * Shows current points, total earned, and rank
 */
export function RewardBalanceDetailed() {
  const { data: balance, isLoading, error, refetch } = useRewardBalance();

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2 p-4 rounded-lg border bg-card">
        <div className="flex items-center gap-2">
          <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
          <span className="text-sm text-muted-foreground">Loading balance...</span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col gap-2 p-4 rounded-lg border bg-destructive/5">
        <div className="flex items-center gap-2">
          <AlertCircle className="h-5 w-5 text-destructive" />
          <span className="text-sm font-medium text-destructive">Failed to load balance</span>
        </div>
        <button
          onClick={() => refetch()}
          className="text-xs text-destructive underline hover:no-underline"
        >
          Retry
        </button>
      </div>
    );
  }

  if (!balance) return null;

  return (
    <div className="flex flex-col gap-3 p-4 rounded-lg border bg-card">
      {/* Current Points */}
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-muted-foreground">Current Points</span>
        <div className="flex items-center gap-2">
          <Coins className="h-5 w-5 text-amber-500" />
          <span className="text-2xl font-bold">{balance.currentPoints.toLocaleString()}</span>
        </div>
      </div>

      {/* Total Earned */}
      <div className="flex items-center justify-between pt-2 border-t">
        <span className="text-xs text-muted-foreground">Total Earned</span>
        <span className="text-sm font-semibold">{balance.totalEarned.toLocaleString()}</span>
      </div>

      {/* Global Rank */}
      {balance.rank > 0 && (
        <div className="flex items-center justify-between">
          <span className="text-xs text-muted-foreground">Global Rank</span>
          <div className="flex items-center gap-1">
            <TrendingUp className="h-4 w-4 text-green-500" />
            <span className="text-sm font-semibold">#{balance.rank}</span>
          </div>
        </div>
      )}
    </div>
  );
}
