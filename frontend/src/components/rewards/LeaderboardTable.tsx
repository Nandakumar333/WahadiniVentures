import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Trophy, Medal, Award, TrendingUp } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { LeaderboardEntryDto } from '@/types/reward'

/**
 * LeaderboardTable Component
 * Implements T075: Display leaderboard with rank, avatar, name, points
 * 
 * Features:
 * - Rank badges for top 3 positions
 * - Current user highlighting
 * - Loading and error states
 * - Responsive design
 */

interface LeaderboardTableProps {
  entries: LeaderboardEntryDto[]
  currentUserId?: string
  isLoading?: boolean
  className?: string
}

export function LeaderboardTable({ 
  entries, 
  currentUserId, 
  isLoading = false,
  className 
}: LeaderboardTableProps) {
  
  /**
   * Get rank icon/badge for top 3 positions
   */
  const getRankBadge = (rank: number) => {
    switch (rank) {
      case 1:
        return <Trophy className="w-6 h-6 text-yellow-500" />
      case 2:
        return <Medal className="w-6 h-6 text-gray-400" />
      case 3:
        return <Award className="w-6 h-6 text-amber-600" />
      default:
        return (
          <div className="w-8 h-8 flex items-center justify-center rounded-full bg-muted text-muted-foreground font-semibold text-sm">
            {rank}
          </div>
        )
    }
  }

  /**
   * Get initials from name for avatar fallback
   */
  const getInitials = (name: string): string => {
    return name
      .split(' ')
      .map(part => part[0])
      .join('')
      .toUpperCase()
      .substring(0, 2)
  }

  /**
   * Loading skeleton
   */
  if (isLoading) {
    return (
      <Card className={cn('w-full', className)}>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="w-5 h-5" />
            Leaderboard
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {[...Array(10)].map((_, i) => (
              <div key={i} className="flex items-center gap-4">
                <Skeleton className="w-8 h-8 rounded-full" />
                <Skeleton className="w-10 h-10 rounded-full" />
                <div className="flex-1 space-y-2">
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-3 w-20" />
                </div>
                <Skeleton className="h-6 w-16" />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    )
  }

  /**
   * Main leaderboard table
   */
  return (
    <Card className={cn('w-full', className)}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <TrendingUp className="w-5 h-5" />
          Leaderboard
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-2">
          {entries.map((entry) => {
            const isCurrentUser = entry.userId === currentUserId
            
            return (
              <div
                key={entry.userId}
                className={cn(
                  'flex items-center gap-4 p-3 rounded-lg transition-colors',
                  isCurrentUser 
                    ? 'bg-primary/10 border-2 border-primary ring-2 ring-primary/20' 
                    : 'hover:bg-muted/50',
                  entry.rank <= 3 && 'bg-gradient-to-r from-muted/50 to-transparent'
                )}
              >
                {/* Rank Badge */}
                <div className="flex-shrink-0 w-10 flex items-center justify-center">
                  {getRankBadge(entry.rank)}
                </div>

                {/* User Avatar */}
                <Avatar className="w-10 h-10 border-2 border-background">
                  <AvatarImage src={entry.avatarUrl} alt={entry.name} />
                  <AvatarFallback className="bg-gradient-to-br from-primary/20 to-primary/10">
                    {getInitials(entry.name)}
                  </AvatarFallback>
                </Avatar>

                {/* User Info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className={cn(
                      'font-semibold truncate',
                      isCurrentUser && 'text-primary'
                    )}>
                      {entry.name}
                    </p>
                    {isCurrentUser && (
                      <Badge variant="secondary" className="text-xs">
                        You
                      </Badge>
                    )}
                  </div>
                  <p className="text-sm text-muted-foreground">
                    Rank #{entry.rank}
                  </p>
                </div>

                {/* Points Display */}
                <div className="flex-shrink-0 text-right">
                  <div className="flex items-center gap-1">
                    <span className={cn(
                      'font-bold text-lg',
                      entry.rank === 1 && 'text-yellow-600',
                      entry.rank === 2 && 'text-gray-500',
                      entry.rank === 3 && 'text-amber-600'
                    )}>
                      {entry.points.toLocaleString()}
                    </span>
                  </div>
                  <p className="text-xs text-muted-foreground">points</p>
                </div>
              </div>
            )
          })}
        </div>

        {/* Empty state handled by parent component */}
        {entries.length === 0 && (
          <div className="text-center py-8 text-muted-foreground">
            No leaderboard data available
          </div>
        )}
      </CardContent>
    </Card>
  )
}
