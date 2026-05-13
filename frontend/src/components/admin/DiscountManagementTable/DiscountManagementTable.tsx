import { useState } from 'react';
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
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Pencil, Trash2, Power, PowerOff } from 'lucide-react';
import type { AdminDiscountType } from '@/types/discount.types';
import {
  useActivateDiscount,
  useDeactivateDiscount,
  useDeleteDiscount,
} from '@/hooks/admin/useDiscountActions';
import { DiscountForm } from '../DiscountForm';
import { useUpdateDiscount } from '@/hooks/admin/useUpdateDiscount';
import type { UpdateDiscountInput } from '@/services/validation/discount.validation';

interface DiscountManagementTableProps {
  discounts: AdminDiscountType[];
  isLoading?: boolean;
}

/**
 * Table component for managing discount codes with CRUD operations
 */
export const DiscountManagementTable = ({
  discounts,
  isLoading = false,
}: DiscountManagementTableProps) => {
  const [editingDiscount, setEditingDiscount] = useState<AdminDiscountType | null>(null);
  const [deletingDiscount, setDeletingDiscount] = useState<AdminDiscountType | null>(null);

  const { mutate: activateDiscount, isPending: isActivating } = useActivateDiscount();
  const { mutate: deactivateDiscount, isPending: isDeactivating } = useDeactivateDiscount();
  const { mutate: deleteDiscount, isPending: isDeleting } = useDeleteDiscount();
  const { mutate: updateDiscount, isPending: isUpdating } = useUpdateDiscount();

  const handleEdit = (discount: AdminDiscountType) => {
    setEditingDiscount(discount);
  };

  const handleDelete = (discount: AdminDiscountType) => {
    setDeletingDiscount(discount);
  };

  const confirmDelete = () => {
    if (deletingDiscount) {
      deleteDiscount(deletingDiscount.id);
      setDeletingDiscount(null);
    }
  };

  const handleToggleActive = (discount: AdminDiscountType) => {
    if (discount.isActive) {
      deactivateDiscount(discount.id);
    } else {
      activateDiscount(discount.id);
    }
  };

  const handleUpdateSubmit = (data: UpdateDiscountInput) => {
    if (editingDiscount) {
      updateDiscount(
        {
          discountCodeId: editingDiscount.id,
          data: {
            discountPercentage: data.discountPercentage,
            requiredPoints: data.requiredPoints,
            maxRedemptions: data.maxRedemptions,
            expiryDate: data.expiryDate ? data.expiryDate.toISOString() : undefined,
          },
        },
        {
          onSuccess: () => {
            setEditingDiscount(null);
          },
        }
      );
    }
  };

  const isExpired = (expiryDate: string | null): boolean => {
    if (!expiryDate) return false;
    return new Date(expiryDate) < new Date();
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <div className="text-muted-foreground">Loading discounts...</div>
      </div>
    );
  }

  if (discounts.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center">
        <p className="text-muted-foreground mb-2">No discount codes found</p>
        <p className="text-sm text-muted-foreground">Create your first discount code to get started</p>
      </div>
    );
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Code</TableHead>
              <TableHead>Discount</TableHead>
              <TableHead>Points</TableHead>
              <TableHead>Redemptions</TableHead>
              <TableHead>Expiry</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {discounts.map((discount) => (
              <TableRow key={discount.id}>
                <TableCell className="font-mono font-semibold">{discount.code}</TableCell>
                <TableCell>{discount.discountPercentage}%</TableCell>
                <TableCell>{discount.requiredPoints.toLocaleString()}</TableCell>
                <TableCell>
                  {discount.currentRedemptions}
                  {discount.maxRedemptions > 0 && ` / ${discount.maxRedemptions}`}
                  {discount.maxRedemptions === 0 && ' / ∞'}
                </TableCell>
                <TableCell>
                  {discount.expiryDate ? (
                    <span className={isExpired(discount.expiryDate) ? 'text-destructive' : ''}>
                      {new Date(discount.expiryDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
                    </span>
                  ) : (
                    <span className="text-muted-foreground">No expiry</span>
                  )}
                </TableCell>
                <TableCell>
                  <Badge variant={isExpired(discount.expiryDate) ? 'destructive' : discount.isActive ? 'default' : 'secondary'}>
                    {isExpired(discount.expiryDate)
                      ? 'Expired'
                      : discount.isActive
                      ? 'Active'
                      : 'Inactive'}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <div className="flex justify-end gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleEdit(discount)}
                      disabled={isUpdating}
                      title="Edit"
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleToggleActive(discount)}
                      disabled={isActivating || isDeactivating || isExpired(discount.expiryDate)}
                      title={discount.isActive ? 'Deactivate' : 'Activate'}
                    >
                      {discount.isActive ? (
                        <PowerOff className="h-4 w-4" />
                      ) : (
                        <Power className="h-4 w-4" />
                      )}
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleDelete(discount)}
                      disabled={isDeleting}
                      title="Delete"
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Edit Dialog */}
      <Dialog open={!!editingDiscount} onOpenChange={(open) => !open && setEditingDiscount(null)}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit Discount Code</DialogTitle>
            <DialogDescription>
              Update discount code: {editingDiscount?.code}
            </DialogDescription>
          </DialogHeader>
          {editingDiscount && (
            <DiscountForm
              mode="edit"
              defaultValues={editingDiscount}
              onSubmit={handleUpdateSubmit}
              isSubmitting={isUpdating}
              onCancel={() => setEditingDiscount(null)}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={!!deletingDiscount} onOpenChange={(open: boolean) => !open && setDeletingDiscount(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Discount Code</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete discount code <strong>{deletingDiscount?.code}</strong>?
              <br />
              <br />
              This action will soft delete the discount code. Historical redemptions will be preserved,
              but the code will no longer be available for new redemptions.
            </DialogDescription>
          </DialogHeader>
          <div className="flex justify-end gap-4 mt-4">
            <Button variant="outline" onClick={() => setDeletingDiscount(null)} disabled={isDeleting}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={confirmDelete} disabled={isDeleting}>
              {isDeleting ? 'Deleting...' : 'Delete'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
};
