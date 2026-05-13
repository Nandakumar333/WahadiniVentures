import React, { useState } from 'react';
import { useInfiniteTransactionHistory } from '@/hooks/reward';
import { TransactionRow } from '@/components/rewards/TransactionRow';
import { EmptyTransactionState, EmptyFilteredTransactionState } from '@/components/rewards/EmptyTransactionState';
import { RewardBalanceDetailed } from '@/components/rewards/RewardBalance';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Loader2, RefreshCw, ChevronDown } from 'lucide-react';
import { type TransactionType, TransactionTypeValues } from '@/types/reward.types';
import { useNavigate } from 'react-router-dom';

/**
 * Transaction type filter options
 */
const TRANSACTION_FILTERS: Array<{ value: TransactionType | 'all'; label: string }> = [
  { value: 'all', label: 'All Transactions' },
  { value: TransactionTypeValues.LessonCompletion, label: 'Lesson Completions' },
  { value: TransactionTypeValues.TaskApproval, label: 'Task Approvals' },
  { value: TransactionTypeValues.CourseCompletion, label: 'Course Completions' },
  { value: TransactionTypeValues.DailyStreak, label: 'Daily Streaks' },
  { value: TransactionTypeValues.ReferralBonus, label: 'Referral Bonuses' },
  { value: TransactionTypeValues.AchievementBonus, label: 'Achievement Bonuses' },
  { value: TransactionTypeValues.AdminBonus, label: 'Admin Bonuses' },
  { value: TransactionTypeValues.AdminPenalty, label: 'Admin Penalties' },
  { value: TransactionTypeValues.Redemption, label: 'Redemptions' },
];

/**
 * TransactionHistory Page
 * 
 * Displays user's complete reward transaction history with:
 * - Infinite scroll pagination using cursor-based pagination
 * - Filter by transaction type
 * - Balance summary card
 * - Loading states and error handling
 * - Empty states for no transactions
 * - Responsive design
 * 
 * Route: /rewards/transactions
 */
export function TransactionHistory() {
  const navigate = useNavigate();
  const [selectedFilter, setSelectedFilter] = useState<TransactionType | 'all'>('all');

  const {
    data,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    error,
    refetch,
    isRefetching,
  } = useInfiniteTransactionHistory(
    20, // Page size
    selectedFilter === 'all' ? undefined : selectedFilter
  );

  // Calculate total transactions loaded
  const totalTransactions = data?.pages?.reduce(
    (total: number, page) => total + page.items.length,
    0
  ) ?? 0;

  // Handle filter change
  const handleFilterChange = (value: string) => {
    setSelectedFilter(value as TransactionType | 'all');
  };

  // Handle clear filter
  const handleClearFilter = () => {
    setSelectedFilter('all');
  };

  // Handle explore courses click
  const handleExploreCourses = () => {
    navigate('/courses');
  };

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      {/* Page header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-foreground mb-2">Transaction History</h1>
        <p className="text-muted-foreground">
          View your complete reward points transaction history
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left column: Balance card (sticky on desktop) */}
        <div className="lg:col-span-1">
          <div className="lg:sticky lg:top-6">
            <RewardBalanceDetailed />
          </div>
        </div>

        {/* Right column: Transaction list */}
        <div className="lg:col-span-2 space-y-4">
          {/* Filter controls card */}
          <Card>
            <CardHeader className="pb-4">
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="text-lg">Transactions</CardTitle>
                  <CardDescription>
                    {totalTransactions > 0 && (
                      <>
                        Showing {totalTransactions} transaction{totalTransactions !== 1 ? 's' : ''}
                      </>
                    )}
                  </CardDescription>
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => refetch()}
                  disabled={isRefetching}
                >
                  {isRefetching ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <RefreshCw className="h-4 w-4" />
                  )}
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              {/* Filter dropdown */}
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
                  Filter by:
                </span>
                <Select value={selectedFilter} onValueChange={handleFilterChange}>
                  <SelectTrigger className="w-full max-w-xs">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {TRANSACTION_FILTERS.map((filter) => (
                      <SelectItem key={filter.value} value={filter.value}>
                        {filter.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {selectedFilter !== 'all' && (
                  <Button variant="ghost" size="sm" onClick={handleClearFilter}>
                    Clear
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Transaction list */}
          {isLoading ? (
            // Initial loading state
            <div className="flex flex-col items-center justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-primary mb-4" />
              <p className="text-sm text-muted-foreground">Loading transactions...</p>
            </div>
          ) : error ? (
            // Error state
            <Card className="border-destructive/50">
              <CardContent className="flex flex-col items-center justify-center py-12">
                <p className="text-sm text-destructive mb-4">
                  Failed to load transactions
                </p>
                <Button onClick={() => refetch()} variant="outline" size="sm">
                  Try Again
                </Button>
              </CardContent>
            </Card>
          ) : totalTransactions === 0 ? (
            // Empty state
            selectedFilter !== 'all' ? (
              <EmptyFilteredTransactionState
                filterType={TRANSACTION_FILTERS.find(f => f.value === selectedFilter)?.label}
                onClearFilter={handleClearFilter}
              />
            ) : (
              <EmptyTransactionState onExploreClick={handleExploreCourses} />
            )
          ) : (
            // Transaction list
            <>
              <div className="space-y-3">
                {data?.pages?.map((page, pageIndex) => (
                  <React.Fragment key={pageIndex}>
                    {page.items.map((transaction) => (
                      <TransactionRow
                        key={transaction.id}
                        transaction={transaction}
                        showBalance={false}
                      />
                    ))}
                  </React.Fragment>
                ))}
              </div>

              {/* Load more button */}
              {hasNextPage && (
                <div className="flex justify-center pt-4">
                  <Button
                    onClick={() => fetchNextPage()}
                    disabled={isFetchingNextPage}
                    variant="outline"
                    size="lg"
                  >
                    {isFetchingNextPage ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Loading more...
                      </>
                    ) : (
                      <>
                        <ChevronDown className="mr-2 h-4 w-4" />
                        Load More
                      </>
                    )}
                  </Button>
                </div>
              )}

              {/* End of list indicator */}
              {!hasNextPage && totalTransactions > 5 && (
                <div className="flex justify-center pt-4">
                  <Badge variant="secondary" className="text-xs">
                    You've reached the end
                  </Badge>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default TransactionHistory;
