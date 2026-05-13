import { useState } from 'react';
import { Edit, Trash2, AlertCircle } from 'lucide-react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Skeleton } from '@/components/ui/skeleton';
import type { CurrencyPricingDto } from '@/types/subscription';
import { CurrencyPricingForm } from './CurrencyPricingForm';
import { useUpdateCurrencyPricing, useDeleteCurrencyPricing } from '@/hooks/admin/useAdminCurrency';
import type { CurrencyPricingInput } from '@/services/validation/currency.validation';

interface CurrencyPricingTableProps {
  currencies: CurrencyPricingDto[];
  isLoading: boolean;
}

export const CurrencyPricingTable = ({ currencies, isLoading }: CurrencyPricingTableProps) => {
  const [editingCurrency, setEditingCurrency] = useState<CurrencyPricingDto | null>(null);
  const [deletingCurrency, setDeletingCurrency] = useState<CurrencyPricingDto | null>(null);

  const { mutate: updateCurrency, isPending: isUpdating } = useUpdateCurrencyPricing();
  const { mutate: deleteCurrency, isPending: isDeleting } = useDeleteCurrencyPricing();

  const handleUpdateSubmit = (data: CurrencyPricingInput) => {
    if (!editingCurrency) return;

    updateCurrency(
      {
        id: editingCurrency.id,
        currencyCode: data.currencyCode,
        monthlyPrice: data.monthlyPrice,
        yearlyPrice: data.yearlyPrice,
        stripePriceIdMonthly: data.stripePriceIdMonthly,
        stripePriceIdYearly: data.stripePriceIdYearly,
        isActive: data.isActive,
      },
      {
        onSuccess: () => {
          setEditingCurrency(null);
        },
      }
    );
  };

  const handleDelete = () => {
    if (!deletingCurrency) return;

    deleteCurrency(deletingCurrency.id, {
      onSuccess: () => {
        setDeletingCurrency(null);
      },
    });
  };

  const formatPrice = (price: number, currencySymbol: string, showDecimal: boolean, decimalPlaces: number) => {
    if (!showDecimal) {
      return `${currencySymbol}${Math.round(price)}`;
    }
    return `${currencySymbol}${price.toFixed(decimalPlaces)}`;
  };

  if (isLoading) {
    return (
      <div className="space-y-2">
        {[...Array(5)].map((_, i) => (
          <Skeleton key={i} className="h-16 w-full" />
        ))}
      </div>
    );
  }

  if (currencies.length === 0) {
    return (
      <Alert>
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          No currency pricings configured. Create your first currency pricing to get started.
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Currency</TableHead>
              <TableHead>Monthly Price</TableHead>
              <TableHead>Yearly Price</TableHead>
              <TableHead>Discount</TableHead>
              <TableHead>Stripe IDs</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {currencies.map((currency) => {
              const monthlyTotal = currency.monthlyPrice * 12;
              const discount = monthlyTotal > 0
                ? Math.round(((monthlyTotal - currency.yearlyPrice) / monthlyTotal) * 100)
                : 0;

              return (
                <TableRow key={currency.id}>
                  <TableCell className="font-medium">
                    <div>
                      <div className="font-bold">{currency.currencyCode}</div>
                      <div className="text-xs text-muted-foreground">
                        {currency.currencySymbol}
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    {formatPrice(
                      currency.monthlyPrice,
                      currency.currencySymbol,
                      currency.showDecimal,
                      currency.decimalPlaces
                    )}
                    /mo
                  </TableCell>
                  <TableCell>
                    {formatPrice(
                      currency.yearlyPrice,
                      currency.currencySymbol,
                      currency.showDecimal,
                      currency.decimalPlaces
                    )}
                    /yr
                  </TableCell>
                  <TableCell>
                    <Badge variant={discount > 0 ? 'default' : 'secondary'}>
                      {discount > 0 ? `Save ${discount}%` : 'No discount'}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="text-xs space-y-1">
                      <div className="truncate max-w-[120px]" title={currency.stripePriceId}>
                        Monthly: {currency.stripePriceId.slice(0, 15)}...
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant={currency.isActive ? 'default' : 'secondary'}>
                      {currency.isActive ? 'Active' : 'Inactive'}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setEditingCurrency(currency)}
                      >
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setDeletingCurrency(currency)}
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>

      {/* Edit Dialog */}
      <Dialog open={!!editingCurrency} onOpenChange={(open) => !open && setEditingCurrency(null)}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit Currency Pricing</DialogTitle>
            <DialogDescription>
              Update pricing for {editingCurrency?.currencyCode}
            </DialogDescription>
          </DialogHeader>
          {editingCurrency && (
            <CurrencyPricingForm
              defaultValues={{
                currencyCode: editingCurrency.currencyCode,
                monthlyPrice: editingCurrency.monthlyPrice,
                yearlyPrice: editingCurrency.yearlyPrice,
                stripePriceIdMonthly: editingCurrency.stripePriceId,
                stripePriceIdYearly: editingCurrency.stripePriceId,
                isActive: editingCurrency.isActive,
              }}
              onSubmit={handleUpdateSubmit}
              isSubmitting={isUpdating}
              isEdit
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={!!deletingCurrency} onOpenChange={(open) => !open && setDeletingCurrency(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Currency Pricing</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete the pricing for {deletingCurrency?.currencyCode}?
              This will soft-delete the record and it will no longer be available for subscriptions.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeletingCurrency(null)}
              disabled={isDeleting}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={isDeleting}
            >
              {isDeleting ? 'Deleting...' : 'Delete'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
};
