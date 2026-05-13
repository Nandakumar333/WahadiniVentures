import type { DiscountType } from '../../../types/discount.types';
import { DiscountCard } from '../DiscountCard/DiscountCard';
import { DiscountListSkeleton } from '../DiscountListSkeleton/DiscountListSkeleton';

interface DiscountListProps {
  discounts: DiscountType[];
  onRedeem?: (discountId: string) => void;
  isLoading?: boolean;
}

/**
 * Grid layout component displaying a list of discount cards
 */
export const DiscountList = ({ discounts, onRedeem, isLoading = false }: DiscountListProps) => {
  // Show loading skeleton while data is being fetched
  if (isLoading) {
    return <DiscountListSkeleton count={6} />;
  }

  if (!discounts || discounts.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center">
        <div className="text-6xl mb-4">🎁</div>
        <h3 className="text-xl font-semibold mb-2">No Discounts Available</h3>
        <p className="text-muted-foreground">
          Check back later for new discount opportunities or earn more points to unlock existing discounts.
        </p>
      </div>
    );
  }

  return (
    <div 
      className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6 px-4 sm:px-0"
      data-testid="discount-gallery"
      role="list"
      aria-label="Available discount codes"
    >
      {discounts.map((discount) => (
        <DiscountCard
          key={discount.id}
          discount={discount}
          onRedeem={onRedeem}
          isLoading={isLoading}
        />
      ))}
    </div>
  );
};
