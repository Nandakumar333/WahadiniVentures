import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Trophy, TrendingUp, Award, ArrowRight } from 'lucide-react'
import { cn } from '@/lib/utils'

/**
 * EmptyLeaderboardState Component
 * Implements T077: Display when no leaderboard data exists
 * 
 * Features:
 * - Friendly empty state message
 * - Call-to-action to earn points
 * - Helpful tips for getting started
 * - Animated illustrations
 */

interface EmptyLeaderboardStateProps {
  onStartEarning?: () => void
  className?: string
}

export function EmptyLeaderboardState({ 
  onStartEarning,
  className 
}: EmptyLeaderboardStateProps) {
  
  const tips = [
    {
      icon: Trophy,
      title: 'Complete Courses',
      description: 'Finish crypto courses to earn points and climb the leaderboard'
    },
    {
      icon: TrendingUp,
      title: 'Submit Tasks',
      description: 'Complete quizzes, wallet setups, and other tasks for rewards'
    },
    {
      icon: Award,
      title: 'Daily Streaks',
      description: 'Log in daily to maintain your streak and earn bonus points'
    }
  ]

  return (
    <Card className={cn('w-full', className)}>
      <CardContent className="flex flex-col items-center justify-center py-12 px-6">
        {/* Illustration */}
        <div className="relative mb-6">
          <div className="absolute inset-0 bg-gradient-to-r from-primary/20 to-purple-500/20 blur-3xl rounded-full" />
          <div className="relative bg-gradient-to-br from-background to-muted p-8 rounded-full border-4 border-muted">
            <Trophy className="w-16 h-16 text-muted-foreground opacity-50" />
          </div>
        </div>

        {/* Main Message */}
        <div className="text-center mb-8 max-w-md">
          <h3 className="text-2xl font-bold mb-2">
            No Leaderboard Data Yet
          </h3>
          <p className="text-muted-foreground">
            Be the first to earn points and claim your spot on the leaderboard! 
            Start completing courses and tasks to begin your crypto learning journey.
          </p>
        </div>

        {/* Call to Action */}
        {onStartEarning && (
          <Button 
            size="lg" 
            onClick={onStartEarning}
            className="mb-8 group"
          >
            Start Earning Points
            <ArrowRight className="w-4 h-4 ml-2 group-hover:translate-x-1 transition-transform" />
          </Button>
        )}

        {/* Tips */}
        <div className="w-full max-w-2xl">
          <h4 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider mb-4 text-center">
            How to Earn Points
          </h4>
          <div className="grid gap-4 sm:grid-cols-3">
            {tips.map((tip, index) => {
              const Icon = tip.icon
              return (
                <div
                  key={index}
                  className="bg-muted/50 p-4 rounded-lg text-center space-y-2 hover:bg-muted transition-colors"
                >
                  <div className="flex justify-center">
                    <div className="p-2 rounded-full bg-primary/10">
                      <Icon className="w-6 h-6 text-primary" />
                    </div>
                  </div>
                  <h5 className="font-semibold text-sm">
                    {tip.title}
                  </h5>
                  <p className="text-xs text-muted-foreground">
                    {tip.description}
                  </p>
                </div>
              )
            })}
          </div>
        </div>

        {/* Additional Info */}
        <div className="mt-8 text-center">
          <p className="text-xs text-muted-foreground">
            Points are updated in real-time. Check back after completing activities!
          </p>
        </div>
      </CardContent>
    </Card>
  )
}
