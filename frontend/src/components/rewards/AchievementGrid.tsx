import { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { AchievementCard } from './AchievementCard';
import type { AchievementDto } from '@/types/reward.types';
import { Trophy, Lock, CheckCircle } from 'lucide-react';

interface AchievementGridProps {
  achievements: AchievementDto[];
  /** Optional callback when achievement is clicked */
  onAchievementClick?: (achievement: AchievementDto) => void;
  /** Show highlighted animation for newly unlocked achievements */
  highlightedIds?: string[];
}

/**
 * AchievementGrid Component
 * 
 * Displays all achievements in a responsive grid with filtering tabs
 * Features:
 * - Tabbed filtering: All, Unlocked, Locked
 * - Achievement count badges
 * - Responsive grid layout (1-4 columns based on screen size)
 * - Progress statistics
 * - Empty states for each filter
 * - Highlight animations for newly unlocked achievements
 * 
 * @example
 * ```tsx
 * function AchievementsPage() {
 *   const { data: achievements } = useAchievements();
 *   
 *   return (
 *     <AchievementGrid 
 *       achievements={achievements}
 *       onAchievementClick={(a) => showDetails(a)}
 *       highlightedIds={['achievement-1', 'achievement-2']}
 *     />
 *   );
 * }
 * ```
 */
export function AchievementGrid({
  achievements,
  onAchievementClick,
  highlightedIds = [],
}: AchievementGridProps) {
  const [activeTab, setActiveTab] = useState<'all' | 'unlocked' | 'locked'>('all');

  // Filter achievements based on active tab
  const unlockedAchievements = achievements.filter((a) => a.isUnlocked);
  const lockedAchievements = achievements.filter((a) => !a.isUnlocked);

  // Calculate statistics
  const totalCount = achievements.length;
  const unlockedCount = unlockedAchievements.length;
  const completionPercentage =
    totalCount > 0 ? Math.round((unlockedCount / totalCount) * 100) : 0;

  return (
    <div className="space-y-6">
      {/* Header with statistics */}
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div className="space-y-1">
          <h2 className="text-2xl font-bold tracking-tight">Achievements</h2>
          <p className="text-sm text-muted-foreground">
            Unlock achievements by completing lessons, tasks, and maintaining streaks
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="secondary" className="text-base px-4 py-2">
            <Trophy className="w-4 h-4 mr-2 text-amber-500" />
            {unlockedCount} / {totalCount}
          </Badge>
          <Badge variant="outline" className="text-base px-4 py-2">
            {completionPercentage}%
          </Badge>
        </div>
      </div>

      {/* Filter tabs */}
      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as typeof activeTab)}>
        <TabsList className="grid w-full max-w-md grid-cols-3">
          <TabsTrigger value="all" className="gap-2">
            <Trophy className="w-4 h-4" />
            <span>All</span>
            <Badge variant="secondary" className="ml-1">
              {totalCount}
            </Badge>
          </TabsTrigger>
          <TabsTrigger value="unlocked" className="gap-2">
            <CheckCircle className="w-4 h-4" />
            <span>Unlocked</span>
            <Badge variant="secondary" className="ml-1">
              {unlockedCount}
            </Badge>
          </TabsTrigger>
          <TabsTrigger value="locked" className="gap-2">
            <Lock className="w-4 h-4" />
            <span>Locked</span>
            <Badge variant="secondary" className="ml-1">
              {lockedAchievements.length}
            </Badge>
          </TabsTrigger>
        </TabsList>

        {/* All achievements tab */}
        <TabsContent value="all" className="mt-6">
          {achievements.length === 0 ? (
            <EmptyState message="No achievements available yet. Check back soon!" />
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {achievements.map((achievement) => (
                <AchievementCard
                  key={achievement.id}
                  achievement={achievement}
                  onClick={onAchievementClick ? () => onAchievementClick(achievement) : undefined}
                  highlight={highlightedIds.includes(achievement.id)}
                />
              ))}
            </div>
          )}
        </TabsContent>

        {/* Unlocked achievements tab */}
        <TabsContent value="unlocked" className="mt-6">
          {unlockedAchievements.length === 0 ? (
            <EmptyState
              icon={CheckCircle}
              message="No achievements unlocked yet. Complete lessons and tasks to start earning!"
            />
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {unlockedAchievements.map((achievement) => (
                <AchievementCard
                  key={achievement.id}
                  achievement={achievement}
                  onClick={onAchievementClick ? () => onAchievementClick(achievement) : undefined}
                  highlight={highlightedIds.includes(achievement.id)}
                />
              ))}
            </div>
          )}
        </TabsContent>

        {/* Locked achievements tab */}
        <TabsContent value="locked" className="mt-6">
          {lockedAchievements.length === 0 ? (
            <EmptyState
              icon={Trophy}
              message="Congratulations! You've unlocked all achievements! 🎉"
            />
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {lockedAchievements.map((achievement) => (
                <AchievementCard
                  key={achievement.id}
                  achievement={achievement}
                  onClick={onAchievementClick ? () => onAchievementClick(achievement) : undefined}
                />
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}

/**
 * Empty state component for when no achievements match the filter
 */
function EmptyState({
  icon: Icon = Trophy,
  message,
}: {
  icon?: typeof Trophy;
  message: string;
}) {
  return (
    <div className="flex flex-col items-center justify-center py-16 px-4 text-center">
      <div className="w-20 h-20 rounded-full bg-muted flex items-center justify-center mb-4">
        <Icon className="w-10 h-10 text-muted-foreground" />
      </div>
      <p className="text-muted-foreground max-w-sm">{message}</p>
    </div>
  );
}
