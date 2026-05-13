import { useQuery, useInfiniteQuery } from '@tanstack/react-query';
import type { UseQueryResult, UseInfiniteQueryResult, InfiniteData } from '@tanstack/react-query';
import { rewardService } from '@/services/api/reward.service';
import type {
  TransactionDto,
  PaginatedResult,
  GetTransactionHistoryRequest,
  TransactionType,
} from '@/types/reward.types';

/**
 * React Query hook for fetching transaction history with cursor-based pagination
 * 
 * @param request - Filter and pagination parameters
 * @returns UseQueryResult with paginated transaction data
 * 
 * @example
 * ```tsx
 * function TransactionList() {
 *   const { data, isLoading } = useTransactionHistory({ pageSize: 20 });
 *   
 *   return (
 *     <div>
 *       {data?.items.map(tx => <TransactionRow key={tx.id} transaction={tx} />)}
 *     </div>
 *   );
 * }
 * ```
 */
export const useTransactionHistory = (
  request: GetTransactionHistoryRequest = {}
): UseQueryResult<PaginatedResult<TransactionDto>, Error> => {
  return useQuery({
    queryKey: ['reward', 'transactions', request],
    queryFn: () => rewardService.getTransactionHistory(request),
    staleTime: 1 * 60 * 1000, // 1 minute
    gcTime: 5 * 60 * 1000,
    retry: 2,
  });
};

/**
 * Hook for infinite scrolling transaction history
 * Automatically fetches next pages using cursor pagination
 * 
 * @param pageSize - Number of items per page
 * @param transactionType - Optional filter by transaction type
 * @returns UseInfiniteQueryResult with infinite scroll support
 * 
 * @example
 * ```tsx
 * function InfiniteTransactionList() {
 *   const {
 *     data,
 *     fetchNextPage,
 *     hasNextPage,
 *     isFetchingNextPage,
 *   } = useInfiniteTransactionHistory(20);
 *   
 *   return (
 *     <div>
 *       {data?.pages.map(page =>
 *         page.items.map(tx => <TransactionRow key={tx.id} transaction={tx} />)
 *       )}
 *       {hasNextPage && (
 *         <button onClick={() => fetchNextPage()} disabled={isFetchingNextPage}>
 *           Load More
 *         </button>
 *       )}
 *     </div>
 *   );
 * }
 * ```
 */
export const useInfiniteTransactionHistory = (
  pageSize: number = 20,
  transactionType?: TransactionType
): UseInfiniteQueryResult<InfiniteData<PaginatedResult<TransactionDto>, string | undefined>, Error> => {
  return useInfiniteQuery<PaginatedResult<TransactionDto>, Error, InfiniteData<PaginatedResult<TransactionDto>, string | undefined>, any[], string | undefined>({
    queryKey: ['reward', 'transactions', 'infinite', { pageSize, transactionType }],
    queryFn: async ({ pageParam }) =>
      await rewardService.getTransactionHistory({
        pageSize,
        cursor: pageParam,
        transactionType,
      }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    getPreviousPageParam: (firstPage) => firstPage.previousCursor ?? undefined,
    staleTime: 1 * 60 * 1000,
    gcTime: 5 * 60 * 1000,
    retry: 2,
  });
};

/**
 * Hook for fetching transactions filtered by type
 * 
 * @param transactionType - Transaction type to filter by
 * @param pageSize - Number of items per page
 * @returns UseQueryResult with filtered transactions
 * 
 * @example
 * ```tsx
 * function LessonCompletionHistory() {
 *   const { data } = useTransactionsByType('LessonCompletion', 10);
 *   return <TransactionList transactions={data?.items ?? []} />;
 * }
 * ```
 */
export const useTransactionsByType = (
  transactionType: TransactionType,
  pageSize: number = 20
): UseQueryResult<PaginatedResult<TransactionDto>, Error> => {
  return useTransactionHistory({
    transactionType,
    pageSize,
  });
};
