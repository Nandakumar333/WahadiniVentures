import { useState } from 'react';
import { Plus, DollarSign } from 'lucide-react';
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
import {
  useAdminCurrencyPricings,
  useCreateCurrencyPricing,
} from '@/hooks/admin/useAdminCurrency';
import { CurrencyPricingTable } from '@/components/admin/CurrencyPricingTable';
import { CurrencyPricingForm } from '@/components/admin/CurrencyPricingForm';
import type { CurrencyPricingInput } from '@/services/validation/currency.validation';

/**
 * Admin page for managing multi-currency pricing
 * Phase 7: Admin Currency Configuration
 * Provides CRUD operations for currency pricing with validation
 */
export const AdminCurrencyManagementPage = () => {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  const { data: currencies, isLoading, isError, error } = useAdminCurrencyPricings();
  const { mutate: createCurrency, isPending: isCreating } = useCreateCurrencyPricing();

  const handleCreateSubmit = (data: CurrencyPricingInput) => {
    createCurrency(
      {
        currencyCode: data.currencyCode,
        monthlyPrice: data.monthlyPrice,
        yearlyPrice: data.yearlyPrice,
        stripePriceIdMonthly: data.stripePriceIdMonthly,
        stripePriceIdYearly: data.stripePriceIdYearly,
        isActive: data.isActive,
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
            <CardTitle className="text-3xl font-bold flex items-center gap-2">
              <DollarSign className="h-8 w-8" />
              Currency Pricing Management
            </CardTitle>
            <CardDescription className="text-base mt-2">
              Configure multi-currency subscription pricing for global markets
            </CardDescription>
          </div>
          <Button onClick={() => setIsCreateDialogOpen(true)}>
            <Plus className="h-4 w-4 mr-2" />
            Add Currency
          </Button>
        </CardHeader>

        <CardContent>
          {isError && (
            <Alert variant="destructive" className="mb-6">
              <AlertDescription>
                {error?.message || 'Failed to load currency pricings. Please try again later.'}
              </AlertDescription>
            </Alert>
          )}

          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="text-sm text-muted-foreground">
                {currencies && (
                  <>
                    Total: {currencies.length} currenc{currencies.length !== 1 ? 'ies' : 'y'} •{' '}
                    Active: {currencies.filter((c) => c.isActive).length} •{' '}
                    Inactive: {currencies.filter((c) => !c.isActive).length}
                  </>
                )}
              </div>
            </div>

            <CurrencyPricingTable
              currencies={currencies || []}
              isLoading={isLoading}
            />
          </div>

          <div className="mt-6 p-4 bg-muted rounded-lg">
            <h3 className="font-semibold mb-2">Validation Rules:</h3>
            <ul className="text-sm text-muted-foreground space-y-1">
              <li>• Currency code: 3 uppercase letters (ISO 4217)</li>
              <li>• Price range: 0 - 9999</li>
              <li>• Yearly price ≤ Monthly price × 12</li>
              <li>• Deviation warning: Yearly price within 50% of (monthly × 12)</li>
              <li>• Stripe price IDs must start with "price_"</li>
            </ul>
          </div>
        </CardContent>
      </Card>

      {/* Create Dialog */}
      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Add New Currency Pricing</DialogTitle>
            <DialogDescription>
              Configure subscription pricing for a new currency market
            </DialogDescription>
          </DialogHeader>
          <CurrencyPricingForm
            onSubmit={handleCreateSubmit}
            isSubmitting={isCreating}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default AdminCurrencyManagementPage;
