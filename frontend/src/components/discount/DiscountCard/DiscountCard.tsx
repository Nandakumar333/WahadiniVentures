import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import type { DiscountType } from '../../../types/discount.types';

interface DiscountCardProps {
  discount: DiscountType;
  onRedeem?: (discountId: string) => void;
  isLoading?: boolean;
}

/**
 * Formats a date as a relative time string (e.g., "in 2 days")
 */
const formatRelativeTime = (date: Date): string => {
  const now = new Date();
  const diff = date.getTime() - now.getTime();
  const days = Math.floor(diff / (1000 * 60 * 60 * 24));
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

  if (days > 0) {
    return `in ${days} day${days !== 1 ? 's' : ''}`;
  } else if (hours > 0) {
    return `in ${hours} hour${hours !== 1 ? 's' : ''}`;
  } else {
    return 'soon';
  }
};

/**
 * Card component displaying a single discount code with redemption button
 */
export const DiscountCard = ({ discount, onRedeem, isLoading = false }: DiscountCardProps) => {
  const isExpired = discount.expiryDate ? new Date(discount.expiryDate) < new Date() : false;
  const isMaxedOut = discount.maxRedemptions > 0 && discount.currentRedemptions >= discount.maxRedemptions;
  const canRedeem = discount.canRedeem && !isExpired && !isMaxedOut;

  const handleRedeem = () => {
    if (canRedeem && onRedeem) {
      onRedeem(discount.id);
    }
  };

  return (
    <Card 
      className="flex flex-col h-full"
      role="article"
      aria-label={`${discount.discountPercentage}% discount, ${discount.requiredPoints} points required, ${canRedeem ? 'available' : 'unavailable'}`}
    >
      <CardHeader>
        <div className="flex items-start justify-between">
          <div>
            <CardTitle className="text-2xl font-bold" aria-label={`${discount.discountPercentage} percent off`}>
              {discount.discountPercentage}% OFF
            </CardTitle>
            <CardDescription className="text-xs mt-1" aria-label={`Code: ${discount.code}`}>
              Code: {discount.code}
            </CardDescription>
          </div>
          <Badge 
            variant={canRedeem ? 'default' : 'secondary'} 
            className="shrink-0"
            aria-label={`Status: ${isExpired ? 'Expired' : isMaxedOut ? 'Sold Out' : discount.isActive ? 'Active' : 'Inactive'}`}
          >
            {isExpired ? 'Expired' : isMaxedOut ? 'Sold Out' : discount.isActive ? 'Active' : 'Inactive'}
          </Badge>
        </div>
      </CardHeader>

      <CardContent className="flex-grow space-y-3">
        <div className="flex items-center justify-between text-sm" role="group" aria-label="Required points information">
          <span className="text-muted-foreground">Required Points:</span>
          <span 
            className={`font-semibold ${discount.canAfford ? 'text-green-600' : 'text-red-600'}`}
            aria-label={`${discount.requiredPoints.toLocaleString()} points ${discount.canAfford ? 'affordable' : 'insufficient balance'}`}
          >
            {discount.requiredPoints.toLocaleString()} pts
          </span>
        </div>

        {discount.maxRedemptions > 0 && (
          <div 
            className="flex items-center justify-between text-sm" 
            role="group" 
            aria-label="Redemptions remaining"
          >
            <span className="text-muted-foreground">Remaining:</span>
            <span 
              className="font-semibold"
              aria-label={`${discount.maxRedemptions - discount.currentRedemptions} out of ${discount.maxRedemptions} remaining`}
            >
              {discount.maxRedemptions - discount.currentRedemptions} / {discount.maxRedemptions}
            </span>
          </div>
        )}

        {discount.expiryDate && (
          <div 
            className="flex items-center justify-between text-sm" 
            role="group" 
            aria-label="Expiry information"
          >
            <span className="text-muted-foreground">Expires:</span>
            <span 
              className="font-semibold text-orange-600"
              aria-label={`Expires ${formatRelativeTime(new Date(discount.expiryDate))}`}
            >
              {formatRelativeTime(new Date(discount.expiryDate))}
            </span>
          </div>
        )}

        {!discount.canAfford && (
          <p 
            className="text-xs text-amber-600 bg-amber-50 p-2 rounded"
            role="alert"
            aria-label={`Warning: Insufficient points. You need ${(discount.requiredPoints - (discount.canAfford ? 0 : discount.requiredPoints)).toLocaleString()} more points`}
          >
            ⚠️ Insufficient points. You need {(discount.requiredPoints - (discount.canAfford ? 0 : discount.requiredPoints)).toLocaleString()} more points.
          </p>
        )}
      </CardContent>

      <CardFooter>
        <Button
          onClick={handleRedeem}
          disabled={!canRedeem || isLoading}
          className="w-full"
          variant={canRedeem ? 'default' : 'secondary'}
          aria-label={
            isLoading 
              ? 'Redeeming discount' 
              : canRedeem 
                ? `Redeem ${discount.discountPercentage}% discount for ${discount.requiredPoints.toLocaleString()} points`
                : isExpired
                  ? 'Discount expired, redemption unavailable'
                  : isMaxedOut
                    ? 'Discount sold out, redemption unavailable'
                    : !discount.canAfford
                      ? 'Insufficient points, redemption unavailable'
                      : 'Discount unavailable'
          }
          aria-disabled={!canRedeem || isLoading}
          aria-busy={isLoading}
        >
          {isLoading ? 'Redeeming...' : canRedeem ? 'Redeem Now' : 'Unavailable'}
        </Button>
      </CardFooter>
    </Card>
  );
};
