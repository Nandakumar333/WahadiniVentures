import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { AchievementGrid } from '@/components/rewards/AchievementGrid';
import { ReferralLink } from '@/components/rewards/ReferralLink';
import { useAchievements } from '@/hooks/reward/useAchievements';
import { useQuery } from '@tanstack/react-query';
import { rewardService } from '@/services/api/reward.service';
import { Loader2, AlertCircle, Trophy, Users, Gift, TrendingUp } from 'lucide-react';
import { RewardBalance } from '@/components/rewards/RewardBalance';
import { StreakTracker } from '@/components/rewards/StreakTracker';
import { useState } from 'react';

/**
 * RewardsPage Component
 * 
 * Main rewards dashboard showing achievements, referrals, and reward stats
 * Features:
 * - Tabbed interface for Achievements and Referrals
 * - Achievement grid with progress tracking
 * - Referral link with statistics
 * - Reward balance and streak info
 * - Loading and error states
 * 
 * @example
 * Route: /rewards
 */
export function RewardsPage() {
  const [activeTab, setActiveTab] = useState<'achievements' | 'referrals'>('achievements');
  const { data: achievements, isLoading: achievementsLoading, error: achievementsError } = useAchievements();
  const { data: referralData, isLoading: referralsLoading, error: referralsError } = useQuery({
    queryKey: ['reward', 'referrals'],
    queryFn: () => rewardService.getReferralInfo(),
    staleTime: 5 * 60 * 1000,
  });

  // Combined loading state
  const isLoading = achievementsLoading || referralsLoading;

  // Loading state
  if (isLoading) {
    return (
      <div className="container max-w-7xl py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="flex flex-col items-center gap-4">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-muted-foreground">Loading rewards...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container max-w-7xl py-8 space-y-8">
      {/* Page Header */}
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tight">Rewards</h1>
        <p className="text-muted-foreground">
          Track your achievements, manage referrals, and earn bonus points
        </p>
      </div>

      {/* Stats Cards Row */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* Balance Card */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Gift className="h-4 w-4 text-amber-500" />
              Your Balance
            </CardTitle>
          </CardHeader>
          <CardContent>
            <RewardBalance />
          </CardContent>
        </Card>

        {/* Achievements Summary */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Trophy className="h-4 w-4 text-amber-500" />
              Achievements
            </CardTitle>
          </CardHeader>
          <CardContent>
            {achievementsError ? (
              <div className="flex items-center gap-2 text-destructive">
                <AlertCircle className="h-4 w-4" />
                <span className="text-sm">Failed to load</span>
              </div>
            ) : achievements ? (
              <div className="space-y-1">
                <div className="text-2xl font-bold">
                  {achievements.filter(a => a.isUnlocked).length} / {achievements.length}
                </div>
                <p className="text-xs text-muted-foreground">Unlocked achievements</p>
              </div>
            ) : null}
          </CardContent>
        </Card>

        {/* Referrals Summary */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Users className="h-4 w-4 text-purple-500" />
              Referrals
            </CardTitle>
          </CardHeader>
          <CardContent>
            {referralsError ? (
              <div className="flex items-center gap-2 text-destructive">
                <AlertCircle className="h-4 w-4" />
                <span className="text-sm">Failed to load</span>
              </div>
            ) : referralData ? (
              <div className="space-y-1">
                <div className="text-2xl font-bold">
                  {referralData.totalReferrals}
                </div>
                <p className="text-xs text-muted-foreground">
                  {referralData.totalPointsEarned} points earned
                </p>
              </div>
            ) : null}
          </CardContent>
        </Card>
      </div>

      {/* Streak Tracker */}
      <StreakTracker />

      {/* Main Tabbed Content */}
      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as typeof activeTab)} className="space-y-6">
        <TabsList className="grid w-full max-w-md grid-cols-2">
          <TabsTrigger value="achievements" className="gap-2">
            <Trophy className="h-4 w-4" />
            Achievements
          </TabsTrigger>
          <TabsTrigger value="referrals" className="gap-2">
            <Users className="h-4 w-4" />
            Referrals
          </TabsTrigger>
        </TabsList>

        {/* Achievements Tab */}
        <TabsContent value="achievements" className="space-y-6">
          {achievementsError ? (
            <Card className="border-destructive/50">
              <CardContent className="pt-6">
                <div className="flex flex-col items-center justify-center py-8 gap-4">
                  <AlertCircle className="h-12 w-12 text-destructive" />
                  <div className="text-center">
                    <p className="font-medium text-destructive">Failed to load achievements</p>
                    <p className="text-sm text-muted-foreground">Please try refreshing the page</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ) : achievements ? (
            <AchievementGrid achievements={achievements} />
          ) : null}
        </TabsContent>

        {/* Referrals Tab */}
        <TabsContent value="referrals" className="space-y-6">
          {referralsError ? (
            <Card className="border-destructive/50">
              <CardContent className="pt-6">
                <div className="flex flex-col items-center justify-center py-8 gap-4">
                  <AlertCircle className="h-12 w-12 text-destructive" />
                  <div className="text-center">
                    <p className="font-medium text-destructive">Failed to load referral data</p>
                    <p className="text-sm text-muted-foreground">Please try refreshing the page</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ) : referralData ? (
            <ReferralLink 
              referralData={referralData}
              showStats={true}
            />
          ) : null}
        </TabsContent>
      </Tabs>

      {/* Info Card */}
      <Card className="bg-muted/50">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5 text-green-500" />
            How to Earn More Points
          </CardTitle>
          <CardDescription>
            Maximize your rewards with these actions
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ul className="space-y-2 text-sm">
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span><strong>Complete lessons</strong> - Earn points for each lesson you finish</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span><strong>Submit tasks</strong> - Get approved and earn bonus points</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span><strong>Finish courses</strong> - Complete entire courses for big rewards</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span><strong>Maintain streaks</strong> - Log in daily to earn streak bonuses</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span><strong>Unlock achievements</strong> - Each achievement awards bonus points</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span><strong>Refer friends</strong> - Earn 100 points when they complete their first course</span>
            </li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
