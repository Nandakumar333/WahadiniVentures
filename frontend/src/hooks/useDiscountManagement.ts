/**
 * Hook for discount code management operations
 * T150: US5 - Reward System Management
 */
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../services/adminService';
import type { CreateDiscountCodeDto } from '../types/admin.types';

export function useDiscountManagement() {
  const queryClient = useQueryClient();

  // Query for fetching discount codes
  const { data: discountCodes = [], isLoading, error } = useQuery({
    queryKey: ['admin', 'discounts'],
    queryFn: () => adminService.getDiscountCodes(),
    staleTime: 2 * 60 * 1000, // 2 minutes
    gcTime: 5 * 60 * 1000, // 5 minutes
  });

  // Mutation for creating discount code
  const createDiscountMutation = useMutation({
    mutationFn: (data: CreateDiscountCodeDto) => adminService.createDiscountCode(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'discounts'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'stats'] });
    },
  });

  // Query for redemption logs
  const useRedemptionLogs = (code: string) =>
    useQuery({
      queryKey: ['admin', 'redemptions', code],
      queryFn: () => adminService.getRedemptions(code),
      enabled: !!code,
      staleTime: 1 * 60 * 1000,
    });

  return {
    discountCodes,
    isLoading,
    error,
    createDiscount: createDiscountMutation.mutateAsync,
    isCreating: createDiscountMutation.isPending,
    useRedemptionLogs,
  };
}
