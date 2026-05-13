import { useStreak } from '@/hooks/reward';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Flame, TrendingUp, Loader2, AlertCircle } from 'lucide-react';

/**
 * StreakTracker Component
 * 
 * Displays user's daily login streak with milestone progress
 * Features:
 * - Current streak count with flame icon
 * - Longest streak badge
 * - Progress bar to next milestone
 * - Milestone rewards display
 * - Loading and error states
 * - Responsive design
 * 
 * @example
 * ```tsx
 * // In dashboard or header
 * <StreakTracker />
 * ```
 */
export function StreakTracker() {
  const { data: streak, isLoading, error, refetch } = useStreak();

  // Loading state
  if (isLoading) {
    return (
      <Card className="w-full">
        <CardContent className="pt-6">
          <div className="flex items-center justify-center gap-2">
            <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
            <span className="text-sm text-muted-foreground">Loading streak...</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Error state
  if (error) {
    return (
      <Card className="w-full border-destructive/50">
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <AlertCircle className="h-5 w-5 text-destructive" />
              <span className="text-sm text-destructive">Failed to load streak</span>
            </div>
            <button
              onClick={() => refetch()}
              className="text-sm font-medium text-destructive hover:underline"
            >
              Retry
            </button>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!streak) return null;

  const { currentStreak, longestStreak, bonusPointsAwarded, nextMilestoneAt } = streak;
  const hasBonus = bonusPointsAwarded > 0;

  // Calculate progress to next milestone
  const progressToMilestone = nextMilestoneAt
    ? (currentStreak / nextMilestoneAt) * 100
    : 100; // 100% if no next milestone (passed all)

  return (
    <Card className="w-full">
      <CardContent className="pt-6 space-y-4">
        {/* Header with current streak */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-orange-500/10">
              <Flame className={`h-6 w-6 text-orange-500 ${currentStreak > 0 ? 'animate-pulse' : ''}`} />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="text-2xl font-bold">{currentStreak}</span>
                <span className="text-sm text-muted-foreground">day streak</span>
              </div>
              {hasBonus && (
                <span className="text-xs text-green-600 font-medium">
                  +{bonusPointsAwarded} points earned today!
                </span>
              )}
            </div>
          </div>

          {/* Longest streak badge */}
          {longestStreak > 0 && (
            <Badge variant="outline" className="flex items-center gap-1.5">
              <TrendingUp className="h-3.5 w-3.5 text-amber-500" />
              <span className="text-xs">Best: {longestStreak}</span>
            </Badge>
          )}
        </div>

        {/* Progress to next milestone */}
        {nextMilestoneAt && (
          <div className="space-y-2">
            <div className="flex items-center justify-between text-xs">
              <span className="text-muted-foreground">
                Next milestone at {nextMilestoneAt} days
              </span>
              <span className="font-medium text-muted-foreground">
                {nextMilestoneAt - currentStreak} days to go
              </span>
            </div>
            <Progress value={progressToMilestone} className="h-2" />
          </div>
        )}

        {/* No next milestone message */}
        {!nextMilestoneAt && currentStreak > 0 && (
          <div className="text-center py-2">
            <span className="text-xs text-muted-foreground italic">
              🎉 You've reached all milestones! Keep the streak alive!
            </span>
          </div>
        )}

        {/* Milestone rewards info */}
        {currentStreak === 0 && (
          <div className="text-xs text-muted-foreground text-center py-2 border-t">
            Log in daily to build your streak and earn bonus points at 5, 10, 30, and 100 days!
          </div>
        )}
      </CardContent>
    </Card>
  );
}

/**
 * StreakTrackerCompact Component
 * 
 * Compact version for navbar or sidebar
 * Shows only streak count with flame icon
 */
export function StreakTrackerCompact() {
  const { data: streak, isLoading } = useStreak();

  if (isLoading) {
    return (
      <div className="flex items-center gap-2 px-3 py-1.5">
        <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!streak || streak.currentStreak === 0) return null;

  return (
    <Badge
      variant="secondary"
      className="flex items-center gap-1.5 px-2 py-1 cursor-default"
      title={`${streak.currentStreak} day login streak`}
    >
      <Flame className="h-3.5 w-3.5 text-orange-500 animate-pulse" />
      <span className="text-xs font-semibold">{streak.currentStreak}</span>
    </Badge>
  );
}

/**
 * StreakTrackerDetailed Component
 * 
 * Detailed version for dashboard or profile
 * Shows full streak statistics with milestone info
 */
export function StreakTrackerDetailed() {
  const { data: streak, isLoading, error, refetch } = useStreak();

  if (isLoading) {
    return (
      <Card className="w-full">
        <CardContent className="flex items-center justify-center py-8">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className="w-full border-destructive/50">
        <CardContent className="py-6 text-center space-y-3">
          <AlertCircle className="h-8 w-8 text-destructive mx-auto" />
          <p className="text-sm text-destructive">Failed to load streak data</p>
          <button
            onClick={() => refetch()}
            className="text-sm font-medium text-primary hover:underline"
          >
            Try again
          </button>
        </CardContent>
      </Card>
    );
  }

  if (!streak) return null;

  const { currentStreak, longestStreak, lastLoginDate, bonusPointsAwarded, nextMilestoneAt } = streak;
  const progressToMilestone = nextMilestoneAt
    ? (currentStreak / nextMilestoneAt) * 100
    : 100;

  const milestones = [
    { days: 5, points: 25 },
    { days: 10, points: 50 },
    { days: 30, points: 100 },
    { days: 100, points: 250 },
  ];

  return (
    <Card className="w-full">
      <CardContent className="pt-6 space-y-6">
        {/* Hero stats */}
        <div className="flex items-center justify-around">
          <div className="text-center space-y-1">
            <div className="flex items-center justify-center gap-2">
              <Flame className={`h-8 w-8 text-orange-500 ${currentStreak > 0 ? 'animate-pulse' : ''}`} />
              <span className="text-4xl font-bold">{currentStreak}</span>
            </div>
            <span className="text-sm text-muted-foreground">Current Streak</span>
          </div>

          <div className="h-12 w-px bg-border" />

          <div className="text-center space-y-1">
            <div className="flex items-center justify-center gap-2">
              <TrendingUp className="h-6 w-6 text-amber-500" />
              <span className="text-3xl font-bold text-amber-500">{longestStreak}</span>
            </div>
            <span className="text-sm text-muted-foreground">Best Streak</span>
          </div>
        </div>

        {/* Today's bonus */}
        {bonusPointsAwarded > 0 && (
          <div className="bg-green-500/10 border border-green-500/20 rounded-lg p-4 text-center">
            <p className="text-sm font-medium text-green-600">
              🎉 +{bonusPointsAwarded} points earned today!
            </p>
          </div>
        )}

        {/* Progress section */}
        {nextMilestoneAt && (
          <div className="space-y-3">
            <div>
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium">Progress to next milestone</span>
                <span className="text-sm text-muted-foreground">
                  {currentStreak} / {nextMilestoneAt} days
                </span>
              </div>
              <Progress value={progressToMilestone} className="h-3" />
            </div>
            <p className="text-xs text-muted-foreground text-center">
              {nextMilestoneAt - currentStreak} more {nextMilestoneAt - currentStreak === 1 ? 'day' : 'days'} to unlock bonus!
            </p>
          </div>
        )}

        {/* Milestone rewards */}
        <div className="space-y-2">
          <h4 className="text-sm font-medium">Milestone Rewards</h4>
          <div className="grid grid-cols-2 gap-2">
            {milestones.map((milestone) => {
              const isReached = currentStreak >= milestone.days;
              return (
                <div
                  key={milestone.days}
                  className={`p-3 rounded-lg border text-center ${
                    isReached
                      ? 'bg-green-500/10 border-green-500/20'
                      : 'bg-muted/50 border-border'
                  }`}
                >
                  <div className="text-lg font-bold">
                    {milestone.days} days
                  </div>
                  <div className={`text-xs ${isReached ? 'text-green-600' : 'text-muted-foreground'}`}>
                    +{milestone.points} points {isReached && '✓'}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Last login */}
        <div className="text-xs text-muted-foreground text-center pt-2 border-t">
          Last login: {new Date(lastLoginDate).toLocaleDateString()}
        </div>
      </CardContent>
    </Card>
  );
}
