import React from 'react';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import {
  BookOpen,
  CheckCircle,
  Award,
  Flame,
  Users,
  Trophy,
  Gift,
  AlertCircle,
  ShoppingCart,
  Circle,
  ArrowUp,
  ArrowDown,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { TransactionDto, TransactionType } from '@/types/reward.types';
import { formatRelativeTime } from '@/services/api/reward.service';

/**
 * Get icon component for transaction type
 */
function getTransactionIcon(type: TransactionType): React.ReactNode {
  const iconClass = "h-5 w-5";
  
  const iconMap: Record<TransactionType, React.ReactNode> = {
    LessonCompletion: <BookOpen className={cn(iconClass, "text-blue-500")} />,
    TaskApproval: <CheckCircle className={cn(iconClass, "text-green-500")} />,
    CourseCompletion: <Award className={cn(iconClass, "text-purple-500")} />,
    DailyStreak: <Flame className={cn(iconClass, "text-orange-500")} />,
    ReferralBonus: <Users className={cn(iconClass, "text-pink-500")} />,
    AchievementBonus: <Trophy className={cn(iconClass, "text-yellow-500")} />,
    AdminBonus: <Gift className={cn(iconClass, "text-emerald-500")} />,
    AdminPenalty: <AlertCircle className={cn(iconClass, "text-red-500")} />,
    Redemption: <ShoppingCart className={cn(iconClass, "text-gray-500")} />,
  };
  
  return iconMap[type] || <Circle className={iconClass} />;
}

/**
 * Get display text for transaction type
 */
function getTransactionTypeDisplay(type: TransactionType): string {
  const displayMap: Record<TransactionType, string> = {
    LessonCompletion: 'Lesson Completed',
    TaskApproval: 'Task Approved',
    CourseCompletion: 'Course Completed',
    DailyStreak: 'Daily Streak Bonus',
    ReferralBonus: 'Referral Bonus',
    AchievementBonus: 'Achievement Unlocked',
    AdminBonus: 'Admin Bonus',
    AdminPenalty: 'Admin Penalty',
    Redemption: 'Points Redeemed',
  };
  
  return displayMap[type] || type;
}

interface TransactionRowProps {
  transaction: TransactionDto;
  showBalance?: boolean;
  className?: string;
}

/**
 * TransactionRow Component
 * 
 * Displays a single transaction in the transaction history
 * Features:
 * - Icon based on transaction type
 * - Color-coded point amounts (green for positive, red for negative)
 * - Relative time display
 * - Optional balance after transaction
 * - Responsive design
 * 
 * @param transaction - Transaction data to display
 * @param showBalance - Whether to show balance after transaction
 * @param className - Additional CSS classes
 */
export function TransactionRow({ transaction, showBalance = false, className }: TransactionRowProps) {
  const isPositive = transaction.amount >= 0;
  const formattedAmount = Math.abs(transaction.amount).toLocaleString();
  const relativeTime = formatRelativeTime(transaction.createdAt);

  return (
    <div
      className={cn(
        "flex items-center gap-4 p-4 rounded-lg border bg-card hover:bg-accent/50 transition-colors",
        className
      )}
    >
      {/* Icon */}
      <div className="flex-shrink-0">
        {getTransactionIcon(transaction.type)}
      </div>

      {/* Transaction details */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <h4 className="text-sm font-semibold text-foreground truncate">
            {getTransactionTypeDisplay(transaction.type)}
          </h4>
        </div>
        <p className="text-xs text-muted-foreground truncate mb-1">
          {transaction.description}
        </p>
        <span className="text-xs text-muted-foreground">
          {relativeTime}
        </span>
      </div>

      {/* Amount and balance */}
      <div className="flex flex-col items-end gap-1">
        {/* Point amount */}
        <div className="flex items-center gap-1">
          {isPositive ? (
            <ArrowUp className="h-4 w-4 text-green-500" />
          ) : (
            <ArrowDown className="h-4 w-4 text-red-500" />
          )}
          <span
            className={cn(
              "text-lg font-bold",
              isPositive ? "text-green-600 dark:text-green-400" : "text-red-600 dark:text-red-400"
            )}
          >
            {isPositive ? '+' : '-'}{formattedAmount}
          </span>
        </div>

        {/* Balance after (if available) */}
        {showBalance && transaction.balanceAfter !== undefined && (
          <span className="text-xs text-muted-foreground">
            Balance: {transaction.balanceAfter.toLocaleString()}
          </span>
        )}
      </div>
    </div>
  );
}

/**
 * TransactionRowCompact Component
 * 
 * Compact version for mobile or sidebar
 */
export function TransactionRowCompact({ transaction, className }: TransactionRowProps) {
  const isPositive = transaction.amount >= 0;
  const formattedAmount = Math.abs(transaction.amount).toLocaleString();

  return (
    <div
      className={cn(
        "flex items-center justify-between gap-2 p-2 rounded-md hover:bg-accent/50 transition-colors",
        className
      )}
    >
      <div className="flex items-center gap-2 min-w-0">
        <div className="flex-shrink-0">
          {getTransactionIcon(transaction.type)}
        </div>
        <span className="text-sm truncate">{getTransactionTypeDisplay(transaction.type)}</span>
      </div>
      <span
        className={cn(
          "text-sm font-semibold whitespace-nowrap",
          isPositive ? "text-green-600" : "text-red-600"
        )}
      >
        {isPositive ? '+' : '-'}{formattedAmount}
      </span>
    </div>
  );
}

/**
 * TransactionCard Component
 * 
 * Card-based version for grid layouts
 */
export function TransactionCard({ transaction, showBalance = false, className }: TransactionRowProps) {
  const isPositive = transaction.amount >= 0;
  const formattedAmount = Math.abs(transaction.amount).toLocaleString();
  const relativeTime = formatRelativeTime(transaction.createdAt);

  return (
    <Card className={cn("hover:shadow-md transition-shadow", className)}>
      <CardContent className="p-4">
        <div className="flex items-start gap-3">
          {/* Icon */}
          <div className="flex-shrink-0 mt-1">
            {getTransactionIcon(transaction.type)}
          </div>

          {/* Content */}
          <div className="flex-1 min-w-0">
            <h4 className="text-sm font-semibold text-foreground mb-1">
              {getTransactionTypeDisplay(transaction.type)}
            </h4>
            <p className="text-xs text-muted-foreground mb-2">
              {transaction.description}
            </p>
            <span className="text-xs text-muted-foreground">{relativeTime}</span>
          </div>

          {/* Amount */}
          <div className="flex flex-col items-end gap-1">
            <span
              className={cn(
                "text-xl font-bold",
                isPositive ? "text-green-600" : "text-red-600"
              )}
            >
              {isPositive ? '+' : '-'}{formattedAmount}
            </span>
            {showBalance && transaction.balanceAfter !== undefined && (
              <Badge variant="outline" className="text-xs">
                {transaction.balanceAfter.toLocaleString()}
              </Badge>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
