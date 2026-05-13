import { Skeleton } from '../../ui/skeleton';

/**
 * Loading skeleton for discount card
 * Displays a placeholder while discount data is being fetched
 */
export const DiscountCardSkeleton = () => {
  return (
    <div className="rounded-lg border bg-card p-6 space-y-4">
      {/* Header with badge */}
      <div className="flex items-start justify-between">
        <Skeleton className="h-6 w-20" />
        <Skeleton className="h-5 w-16" />
      </div>

      {/* Percentage */}
      <div className="space-y-2">
        <Skeleton className="h-10 w-24" />
        <Skeleton className="h-4 w-32" />
      </div>

      {/* Details */}
      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-4 w-16" />
        </div>
        <div className="flex items-center justify-between">
          <Skeleton className="h-4 w-28" />
          <Skeleton className="h-4 w-20" />
        </div>
        <div className="flex items-center justify-between">
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-4 w-24" />
        </div>
      </div>

      {/* Button */}
      <Skeleton className="h-10 w-full" />
    </div>
  );
};

/**
 * Loading skeleton for discount list
 * Shows multiple card skeletons in a grid layout
 */
interface DiscountListSkeletonProps {
  count?: number;
}

export const DiscountListSkeleton = ({ count = 6 }: DiscountListSkeletonProps) => {
  return (
    <div 
      className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
      role="status"
      aria-label="Loading discounts"
    >
      {Array.from({ length: count }).map((_, index) => (
        <DiscountCardSkeleton key={index} />
      ))}
      <span className="sr-only">Loading discount codes...</span>
    </div>
  );
};
