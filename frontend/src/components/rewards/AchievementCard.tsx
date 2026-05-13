import { Card, CardContent, CardFooter } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Trophy, Lock, Check, Sparkles } from 'lucide-react';
import type { AchievementDto, AchievementCategory } from '@/types/reward.types';
import { cn } from '@/lib/utils';

interface AchievementCardProps {
  achievement: AchievementDto;
  /** Optional click handler for card interaction */
  onClick?: () => void;
  /** Highlight card with animation (used for newly unlocked achievements) */
  highlight?: boolean;
}

/**
 * AchievementCard Component
 * 
 * Displays an individual achievement with unlock status, progress, and rewards
 * Features:
 * - Locked/unlocked states with distinct visual styles
 * - Progress bar for locked achievements
 * - Category badge with color coding
 * - Point bonus display
 * - Unlock date display
 * - Grayscale filter for locked achievements
 * - Hover effects and animations
 * - Optional highlight animation for newly unlocked
 * 
 * @example
 * ```tsx
 * <AchievementCard achievement={achievement} />
 * <AchievementCard achievement={achievement} highlight={true} />
 * <AchievementCard achievement={achievement} onClick={() => showDetails(achievement.id)} />
 * ```
 */
export function AchievementCard({ achievement, onClick, highlight = false }: AchievementCardProps) {
  const {
    name,
    description,
    category,
    iconUrl,
    pointBonus,
    isUnlocked,
    unlockedAt,
    progress,
  } = achievement;

  const categoryColor = getCategoryColor(category);
  const formattedDate = unlockedAt ? new Date(unlockedAt).toLocaleDateString() : null;

  return (
    <Card
      className={cn(
        'group relative overflow-hidden transition-all duration-300',
        isUnlocked
          ? 'hover:scale-105 hover:shadow-lg cursor-pointer'
          : 'hover:scale-102 opacity-75 grayscale hover:grayscale-0',
        highlight && 'animate-pulse ring-2 ring-amber-500',
        onClick && 'cursor-pointer'
      )}
      onClick={onClick}
    >
      {/* Background gradient for unlocked achievements */}
      {isUnlocked && (
        <div className="absolute inset-0 bg-gradient-to-br from-amber-500/10 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
      )}

      <CardContent className="pt-6 pb-4 space-y-4">
        {/* Icon and status */}
        <div className="flex items-start justify-between">
          <div className="relative">
            {/* Achievement icon */}
            <div
              className={cn(
                'w-16 h-16 rounded-lg flex items-center justify-center',
                isUnlocked ? 'bg-gradient-to-br from-amber-400 to-amber-600' : 'bg-muted'
              )}
            >
              {iconUrl ? (
                <img
                  src={iconUrl}
                  alt={name}
                  className="w-10 h-10 object-contain"
                />
              ) : (
                <Trophy
                  className={cn(
                    'w-8 h-8',
                    isUnlocked ? 'text-white' : 'text-muted-foreground'
                  )}
                />
              )}
            </div>

            {/* Lock/unlock indicator */}
            <div
              className={cn(
                'absolute -bottom-1 -right-1 w-6 h-6 rounded-full flex items-center justify-center',
                isUnlocked ? 'bg-green-500' : 'bg-muted-foreground'
              )}
            >
              {isUnlocked ? (
                <Check className="w-4 h-4 text-white" />
              ) : (
                <Lock className="w-3 h-3 text-white" />
              )}
            </div>
          </div>

          {/* Category badge */}
          <Badge
            variant="outline"
            className={cn('text-xs font-medium', categoryColor)}
          >
            {category}
          </Badge>
        </div>

        {/* Achievement info */}
        <div className="space-y-2">
          <h3 className="font-semibold text-lg leading-tight">{name}</h3>
          <p className="text-sm text-muted-foreground line-clamp-2">
            {description}
          </p>
        </div>

        {/* Progress bar (for locked achievements) */}
        {!isUnlocked && (
          <div className="space-y-2">
            <div className="flex items-center justify-between text-xs">
              <span className="text-muted-foreground">Progress</span>
              <span className="font-medium">{progress}%</span>
            </div>
            <Progress value={progress} className="h-2" />
          </div>
        )}

        {/* Unlock date (for unlocked achievements) */}
        {isUnlocked && formattedDate && (
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <Sparkles className="w-3 h-3" />
            <span>Unlocked on {formattedDate}</span>
          </div>
        )}
      </CardContent>

      <CardFooter className="pt-0 pb-4">
        {/* Point bonus display */}
        <div className="flex items-center justify-between w-full">
          <span className="text-sm text-muted-foreground">Reward</span>
          <div className="flex items-center gap-1.5">
            <Trophy className="w-4 h-4 text-amber-500" />
            <span className="font-bold text-amber-600">
              +{pointBonus} points
            </span>
          </div>
        </div>
      </CardFooter>
    </Card>
  );
}

/**
 * Helper function to get category-specific colors
 */
function getCategoryColor(category: AchievementCategory): string {
  const colors: Record<AchievementCategory, string> = {
    Course: 'border-blue-500 text-blue-600',
    Task: 'border-green-500 text-green-600',
    Streak: 'border-orange-500 text-orange-600',
    Points: 'border-amber-500 text-amber-600',
    Social: 'border-purple-500 text-purple-600',
    Special: 'border-pink-500 text-pink-600',
  };
  
  return colors[category] || 'border-gray-500 text-gray-600';
}
