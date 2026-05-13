import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Trophy, Users, TrendingUp, Award } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { UserRankDto } from '@/types/reward'
import { LeaderboardPeriod } from '@/hooks/reward/useLeaderboard'

/**
 * UserRankCard Component
 * Implements T076: Display current user's rank, points, total users
 * 
 * Features:
 * - Compact and detailed variants
 * - Period indicator
 * - Rank badge with color coding
 * - Loading states
 */

interface UserRankCardProps {
  userRank?: UserRankDto
  isLoading?: boolean
  variant?: 'compact' | 'detailed'
  className?: string
}

export function UserRankCard({ 
  userRank, 
  isLoading = false,
  variant = 'detailed',
  className 
}: UserRankCardProps) {
  
  /**
   * Get rank category and styling
   */
  const getRankInfo = (rank: number, totalUsers: number) => {
    const percentile = (rank / totalUsers) * 100
    
    if (rank === 1) {
      return {
        category: 'Champion',
        color: 'text-yellow-600',
        bgColor: 'bg-yellow-500/10',
        borderColor: 'border-yellow-500/20',
        icon: Trophy
      }
    } else if (rank <= 3) {
      return {
        category: 'Elite',
        color: 'text-blue-600',
        bgColor: 'bg-blue-500/10',
        borderColor: 'border-blue-500/20',
        icon: Award
      }
    } else if (percentile <= 10) {
      return {
        category: 'Top 10%',
        color: 'text-green-600',
        bgColor: 'bg-green-500/10',
        borderColor: 'border-green-500/20',
        icon: TrendingUp
      }
    } else if (percentile <= 25) {
      return {
        category: 'Top 25%',
        color: 'text-purple-600',
        bgColor: 'bg-purple-500/10',
        borderColor: 'border-purple-500/20',
        icon: TrendingUp
      }
    } else {
      return {
        category: 'Rising Star',
        color: 'text-gray-600',
        bgColor: 'bg-gray-500/10',
        borderColor: 'border-gray-500/20',
        icon: Users
      }
    }
  }

  /**
   * Format period display
   */
  const getPeriodLabel = (period: LeaderboardPeriod): string => {
    switch (period) {
      case LeaderboardPeriod.Weekly:
        return 'This Week'
      case LeaderboardPeriod.Monthly:
        return 'This Month'
      case LeaderboardPeriod.AllTime:
        return 'All Time'
      default:
        return period
    }
  }

  /**
   * Loading skeleton
   */
  if (isLoading) {
    return (
      <Card className={cn('w-full', className)}>
        <CardHeader>
          <CardTitle className="text-lg">Your Rank</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <Skeleton className="h-16 w-full" />
            <div className="grid grid-cols-2 gap-4">
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
            </div>
          </div>
        </CardContent>
      </Card>
    )
  }

  /**
   * No data state
   */
  if (!userRank) {
    return (
      <Card className={cn('w-full', className)}>
        <CardContent className="flex items-center justify-center py-8">
          <div className="text-center text-muted-foreground">
            <Users className="w-12 h-12 mx-auto mb-2 opacity-50" />
            <p>No rank data available</p>
            <p className="text-sm">Start earning points to join the leaderboard!</p>
          </div>
        </CardContent>
      </Card>
    )
  }

  const rankInfo = getRankInfo(userRank.rank, userRank.totalUsers)
  const RankIcon = rankInfo.icon
  const percentile = ((userRank.rank / userRank.totalUsers) * 100).toFixed(1)

  /**
   * Compact variant
   */
  if (variant === 'compact') {
    return (
      <Card className={cn('w-full', rankInfo.borderColor, 'border-2', className)}>
        <CardContent className="p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className={cn('p-2 rounded-full', rankInfo.bgColor)}>
                <RankIcon className={cn('w-6 h-6', rankInfo.color)} />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Your Rank</p>
                <p className={cn('text-2xl font-bold', rankInfo.color)}>
                  #{userRank.rank}
                </p>
              </div>
            </div>
            <div className="text-right">
              <p className="text-sm text-muted-foreground">Points</p>
              <p className="text-xl font-semibold">
                {userRank.points.toLocaleString()}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    )
  }

  /**
   * Detailed variant
   */
  return (
    <Card className={cn('w-full', rankInfo.borderColor, 'border-2', className)}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">Your Rank</CardTitle>
          <Badge variant="outline" className={cn(rankInfo.color, 'border-current')}>
            {getPeriodLabel(userRank.period)}
          </Badge>
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {/* Main Rank Display */}
          <div className={cn(
            'flex items-center gap-4 p-4 rounded-lg',
            rankInfo.bgColor
          )}>
            <div className={cn('p-3 rounded-full', 'bg-background')}>
              <RankIcon className={cn('w-8 h-8', rankInfo.color)} />
            </div>
            <div className="flex-1">
              <p className="text-sm text-muted-foreground mb-1">
                {rankInfo.category}
              </p>
              <div className="flex items-baseline gap-2">
                <span className={cn('text-4xl font-bold', rankInfo.color)}>
                  #{userRank.rank}
                </span>
                <span className="text-sm text-muted-foreground">
                  out of {userRank.totalUsers.toLocaleString()}
                </span>
              </div>
            </div>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-2 gap-4">
            {/* Points */}
            <div className="bg-muted/50 p-4 rounded-lg">
              <div className="flex items-center gap-2 mb-1">
                <Trophy className="w-4 h-4 text-muted-foreground" />
                <p className="text-xs text-muted-foreground uppercase font-medium">
                  Points
                </p>
              </div>
              <p className="text-2xl font-bold">
                {userRank.points.toLocaleString()}
              </p>
            </div>

            {/* Percentile */}
            <div className="bg-muted/50 p-4 rounded-lg">
              <div className="flex items-center gap-2 mb-1">
                <TrendingUp className="w-4 h-4 text-muted-foreground" />
                <p className="text-xs text-muted-foreground uppercase font-medium">
                  Percentile
                </p>
              </div>
              <p className="text-2xl font-bold">
                Top {percentile}%
              </p>
            </div>
          </div>

          {/* Motivational Message */}
          {userRank.rank > 1 && (
            <div className="text-sm text-muted-foreground text-center pt-2 border-t">
              {userRank.rank <= 10 
                ? `You're in the top 10! Keep going to reach #1!` 
                : `Complete more tasks to climb higher!`}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  )
}
