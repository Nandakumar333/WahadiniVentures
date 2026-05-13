import { useState } from 'react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { RefreshCw, Trophy, Calendar, Clock, Infinity } from 'lucide-react'
import { LeaderboardTable } from '@/components/rewards/LeaderboardTable'
import { UserRankCard } from '@/components/rewards/UserRankCard'
import { EmptyLeaderboardState } from '@/components/rewards/EmptyLeaderboardState'
import { 
  useLeaderboard, 
  useUserRank, 
  useRefreshLeaderboard,
  LeaderboardPeriod 
} from '@/hooks/reward/useLeaderboard'
import { useAuth } from '@/hooks/auth/useAuth'
import { cn } from '@/lib/utils'

/**
 * Leaderboard Page Component
 * Implements T074: Complete leaderboard page with period tabs
 * 
 * Features:
 * - Weekly/Monthly/All-Time tabs
 * - User rank card integration
 * - Leaderboard table with current user highlighting
 * - Manual refresh functionality
 * - Loading and error states
 * - Empty state handling
 * - Responsive layout
 */

export function LeaderboardPage() {
  const { user } = useAuth()
  const [selectedPeriod, setSelectedPeriod] = useState<LeaderboardPeriod>(
    LeaderboardPeriod.Weekly
  )
  const [isRefreshing, setIsRefreshing] = useState(false)

  // Fetch leaderboard data
  const { 
    data: leaderboard, 
    isLoading: isLoadingLeaderboard, 
    error: leaderboardError,
    refetch: refetchLeaderboard
  } = useLeaderboard(selectedPeriod, 100)

  // Fetch user rank
  const { 
    data: userRank, 
    isLoading: isLoadingUserRank,
    error: userRankError,
    refetch: refetchUserRank
  } = useUserRank(selectedPeriod)

  // Refresh functionality
  const { refreshLeaderboard } = useRefreshLeaderboard()

  /**
   * Handle manual refresh
   */
  const handleRefresh = async () => {
    setIsRefreshing(true)
    try {
      await Promise.all([
        refreshLeaderboard(selectedPeriod),
        refetchLeaderboard(),
        refetchUserRank()
      ])
    } finally {
      setIsRefreshing(false)
    }
  }

  /**
   * Handle period tab change
   */
  const handlePeriodChange = (value: string) => {
    setSelectedPeriod(value as LeaderboardPeriod)
  }

  /**
   * Navigate to courses
   */
  const handleStartEarning = () => {
    // Navigate to courses page
    window.location.href = '/courses'
  }

  /**
   * Get period icon
   */
  const getPeriodIcon = (period: LeaderboardPeriod) => {
    switch (period) {
      case LeaderboardPeriod.Weekly:
        return <Clock className="w-4 h-4" />
      case LeaderboardPeriod.Monthly:
        return <Calendar className="w-4 h-4" />
      case LeaderboardPeriod.AllTime:
        return <Infinity className="w-4 h-4" />
    }
  }

  /**
   * Error state
   */
  if (leaderboardError || userRankError) {
    return (
      <div className="container mx-auto px-4 py-8 max-w-6xl">
        <Alert variant="destructive">
          <AlertDescription>
            Failed to load leaderboard data. Please try again.
            <Button 
              variant="outline" 
              size="sm" 
              onClick={handleRefresh}
              className="ml-4"
            >
              Retry
            </Button>
          </AlertDescription>
        </Alert>
      </div>
    )
  }

  const hasData = leaderboard && leaderboard.length > 0

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      {/* Page Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-4xl font-bold flex items-center gap-3 mb-2">
            <Trophy className="w-10 h-10 text-primary" />
            Leaderboard
          </h1>
          <p className="text-muted-foreground">
            Compete with other learners and climb to the top!
          </p>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={handleRefresh}
          disabled={isRefreshing || isLoadingLeaderboard}
          className="gap-2"
        >
          <RefreshCw className={cn(
            'w-4 h-4',
            isRefreshing && 'animate-spin'
          )} />
          Refresh
        </Button>
      </div>

      {/* Main Content */}
      <Tabs 
        value={selectedPeriod} 
        onValueChange={handlePeriodChange}
        className="space-y-6"
      >
        {/* Period Tabs */}
        <TabsList className="grid w-full max-w-md mx-auto grid-cols-3">
          <TabsTrigger value={LeaderboardPeriod.Weekly} className="gap-2">
            {getPeriodIcon(LeaderboardPeriod.Weekly)}
            Weekly
          </TabsTrigger>
          <TabsTrigger value={LeaderboardPeriod.Monthly} className="gap-2">
            {getPeriodIcon(LeaderboardPeriod.Monthly)}
            Monthly
          </TabsTrigger>
          <TabsTrigger value={LeaderboardPeriod.AllTime} className="gap-2">
            {getPeriodIcon(LeaderboardPeriod.AllTime)}
            All Time
          </TabsTrigger>
        </TabsList>

        {/* Tab Content for each period */}
        {[LeaderboardPeriod.Weekly, LeaderboardPeriod.Monthly, LeaderboardPeriod.AllTime].map((period) => (
          <TabsContent key={period} value={period} className="space-y-6">
            {hasData ? (
              <div className="grid gap-6 lg:grid-cols-3">
                {/* User Rank Card - Takes 1 column */}
                <div className="lg:col-span-1">
                  <UserRankCard
                    userRank={userRank}
                    isLoading={isLoadingUserRank}
                    variant="detailed"
                  />
                </div>

                {/* Leaderboard Table - Takes 2 columns */}
                <div className="lg:col-span-2">
                  <LeaderboardTable
                    entries={leaderboard || []}
                    currentUserId={user?.id}
                    isLoading={isLoadingLeaderboard}
                  />
                </div>
              </div>
            ) : (
              /* Empty State */
              <EmptyLeaderboardState onStartEarning={handleStartEarning} />
            )}

            {/* Info Card */}
            <Card className="bg-muted/50 border-none">
              <CardHeader>
                <CardTitle className="text-sm font-medium">
                  How Rankings Work
                </CardTitle>
              </CardHeader>
              <CardContent className="text-sm text-muted-foreground space-y-2">
                <p>
                  <strong>Weekly:</strong> Rankings based on points earned in the last 7 days
                </p>
                <p>
                  <strong>Monthly:</strong> Rankings based on points earned in the last 30 days
                </p>
                <p>
                  <strong>All Time:</strong> Rankings based on your total lifetime points
                </p>
                <p className="pt-2 text-xs">
                  Leaderboard updates every 15 minutes. Ties are broken by account creation date.
                </p>
              </CardContent>
            </Card>
          </TabsContent>
        ))}
      </Tabs>
    </div>
  )
}

export default LeaderboardPage
