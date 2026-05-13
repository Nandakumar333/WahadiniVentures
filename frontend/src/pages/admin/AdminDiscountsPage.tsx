import { useState } from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { useAdminDiscounts } from '@/hooks/admin/useAdminDiscounts';
import { useCreateDiscount } from '@/hooks/admin/useCreateDiscount';
import { DiscountManagementTable } from '@/components/admin/DiscountManagementTable';
import { DiscountForm } from '@/components/admin/DiscountForm';
import type { CreateDiscountInput } from '@/services/validation/discount.validation';

/**
 * Admin page for managing discount codes
 * Provides CRUD operations for discount codes
 */
export const AdminDiscountsPage = () => {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  const { data: discounts, isLoading, isError, error } = useAdminDiscounts();
  const { mutate: createDiscount, isPending: isCreating } = useCreateDiscount();

  const handleCreateSubmit = (data: CreateDiscountInput) => {
    createDiscount(
      {
        code: data.code,
        discountPercentage: data.discountPercentage,
        requiredPoints: data.requiredPoints,
        maxRedemptions: data.maxRedemptions,
        expiryDate: data.expiryDate ? data.expiryDate.toISOString() : null,
      },
      {
        onSuccess: () => {
          setIsCreateDialogOpen(false);
        },
      }
    );
  };

  return (
    <div className="container mx-auto px-4 py-8 max-w-7xl">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <div>
            <CardTitle className="text-3xl font-bold">Discount Code Management</CardTitle>
            <CardDescription className="text-base mt-2">
              Create and manage discount codes for subscription rewards
            </CardDescription>
          </div>
          <Button onClick={() => setIsCreateDialogOpen(true)}>
            <Plus className="h-4 w-4 mr-2" />
            Create Discount
          </Button>
        </CardHeader>

        <CardContent>
          {isError && (
            <Alert variant="destructive" className="mb-6">
              <AlertDescription>
                {error?.message || 'Failed to load discount codes. Please try again later.'}
              </AlertDescription>
            </Alert>
          )}

          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="text-sm text-muted-foreground">
                {discounts && `Total: ${discounts.length} discount code${discounts.length !== 1 ? 's' : ''}`}
              </div>
            </div>

            <DiscountManagementTable
              discounts={discounts || []}
              isLoading={isLoading}
            />
          </div>
        </CardContent>
      </Card>

      {/* Create Dialog */}
      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Create New Discount Code</DialogTitle>
            <DialogDescription>
              Add a new discount code for users to redeem with their reward points
            </DialogDescription>
          </DialogHeader>
          <DiscountForm
            mode="create"
            onSubmit={handleCreateSubmit}
            isSubmitting={isCreating}
            onCancel={() => setIsCreateDialogOpen(false)}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
};
