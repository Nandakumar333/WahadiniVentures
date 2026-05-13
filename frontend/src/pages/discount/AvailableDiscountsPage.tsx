import { useState } from 'react';
import { useDiscounts } from '../../hooks/discount/useDiscounts';
import { DiscountList } from '../../components/discount/DiscountList/DiscountList';
import { RedemptionModal } from '../../components/discount/RedemptionModal/RedemptionModal';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Skeleton } from '@/components/ui/skeleton';
import type { DiscountType } from '../../types/discount.types';

/**
 * Page component displaying available discounts for the current user
 */
export const AvailableDiscountsPage = () => {
  const { data: discounts, isLoading, isError, error } = useDiscounts();
  const [selectedDiscount, setSelectedDiscount] = useState<DiscountType | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleRedeem = (discountId: string) => {
    const discount = discounts?.find((d) => d.id === discountId);
    if (discount) {
      setSelectedDiscount(discount);
      setIsModalOpen(true);
    }
  };

  return (
    <div className="container mx-auto px-4 py-8 max-w-7xl">
      <CardHeader className="px-0">
        <CardTitle className="text-3xl font-bold">Available Discounts</CardTitle>
        <CardDescription className="text-base">
          Redeem your points for exclusive subscription discounts
        </CardDescription>
      </CardHeader>

      <CardContent className="px-0">
        {isError && (
          <Alert variant="destructive" className="mb-6">
            <AlertDescription>
              {error?.message || 'Failed to load discounts. Please try again later.'}
            </AlertDescription>
          </Alert>
        )}

        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[...Array(6)].map((_, index) => (
              <Card key={index} className="flex flex-col h-full">
                <CardHeader>
                  <Skeleton className="h-8 w-24 mb-2" />
                  <Skeleton className="h-4 w-32" />
                </CardHeader>
                <CardContent className="flex-grow space-y-3">
                  <Skeleton className="h-4 w-full" />
                  <Skeleton className="h-4 w-full" />
                  <Skeleton className="h-4 w-3/4" />
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <DiscountList discounts={discounts || []} onRedeem={handleRedeem} />
        )}
      </CardContent>

      {/* Redemption Modal */}
      <RedemptionModal
        discount={selectedDiscount}
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
      />
    </div>
  );
};
