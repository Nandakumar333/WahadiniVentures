import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Receipt, TrendingUp, BookOpen, CheckCircle, Award } from 'lucide-react';
import { cn } from '@/lib/utils';

interface EmptyTransactionStateProps {
  className?: string;
  onExploreClick?: () => void;
}

/**
 * EmptyTransactionState Component
 * 
 * Displays when user has no transaction history yet
 * Features:
 * - Friendly illustration/icon
 * - Helpful message guiding user to earn points
 * - Call-to-action button to explore courses
 * - Responsive design
 * 
 * @param className - Additional CSS classes
 * @param onExploreClick - Callback when explore button clicked
 */
export function EmptyTransactionState({ className, onExploreClick }: EmptyTransactionStateProps) {
  return (
    <Card className={cn("border-dashed", className)}>
      <CardContent className="flex flex-col items-center justify-center py-12 px-6 text-center">
        {/* Icon */}
        <div className="relative mb-6">
          <div className="w-20 h-20 rounded-full bg-muted flex items-center justify-center">
            <Receipt className="h-10 w-10 text-muted-foreground" />
          </div>
          <div className="absolute -top-2 -right-2 w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
            <TrendingUp className="h-4 w-4 text-primary" />
          </div>
        </div>

        {/* Heading */}
        <h3 className="text-lg font-semibold text-foreground mb-2">
          No Transactions Yet
        </h3>

        {/* Description */}
        <p className="text-sm text-muted-foreground max-w-sm mb-6">
          Start your learning journey to earn reward points! Complete lessons, submit tasks, 
          and finish courses to see your transaction history here.
        </p>

        {/* Ways to earn points */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6 w-full max-w-lg">
          <div className="flex flex-col items-center gap-2 p-4 rounded-lg bg-muted/50">
            <BookOpen className="h-6 w-6 text-blue-500" />
            <span className="text-xs font-medium">Complete Lessons</span>
          </div>
          <div className="flex flex-col items-center gap-2 p-4 rounded-lg bg-muted/50">
            <CheckCircle className="h-6 w-6 text-green-500" />
            <span className="text-xs font-medium">Submit Tasks</span>
          </div>
          <div className="flex flex-col items-center gap-2 p-4 rounded-lg bg-muted/50">
            <Award className="h-6 w-6 text-purple-500" />
            <span className="text-xs font-medium">Finish Courses</span>
          </div>
        </div>

        {/* CTA Button */}
        {onExploreClick && (
          <Button onClick={onExploreClick} size="lg">
            Explore Courses
          </Button>
        )}
      </CardContent>
    </Card>
  );
}

/**
 * EmptyFilteredTransactionState Component
 * 
 * Displays when no transactions match the current filter
 */
interface EmptyFilteredTransactionStateProps {
  filterType?: string;
  onClearFilter?: () => void;
  className?: string;
}

export function EmptyFilteredTransactionState({ 
  filterType, 
  onClearFilter,
  className 
}: EmptyFilteredTransactionStateProps) {
  return (
    <Card className={cn("border-dashed", className)}>
      <CardContent className="flex flex-col items-center justify-center py-12 px-6 text-center">
        {/* Icon */}
        <div className="w-16 h-16 rounded-full bg-muted flex items-center justify-center mb-4">
          <Receipt className="h-8 w-8 text-muted-foreground" />
        </div>

        {/* Heading */}
        <h3 className="text-base font-semibold text-foreground mb-2">
          No {filterType ? filterType : 'Matching'} Transactions
        </h3>

        {/* Description */}
        <p className="text-sm text-muted-foreground max-w-sm mb-4">
          {filterType 
            ? `You don't have any ${filterType} transactions yet.`
            : 'No transactions match your current filter.'
          }
        </p>

        {/* Clear filter button */}
        {onClearFilter && (
          <Button onClick={onClearFilter} variant="outline" size="sm">
            Clear Filter
          </Button>
        )}
      </CardContent>
    </Card>
  );
}

/**
 * EmptyTransactionStateCompact Component
 * 
 * Compact version for sidebars or smaller spaces
 */
export function EmptyTransactionStateCompact({ className }: { className?: string }) {
  return (
    <div className={cn("flex flex-col items-center gap-2 p-4 text-center", className)}>
      <Receipt className="h-8 w-8 text-muted-foreground" />
      <p className="text-xs text-muted-foreground">
        No transactions yet
      </p>
    </div>
  );
}
