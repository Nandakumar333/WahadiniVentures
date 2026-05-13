import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';
import type { CurrencyPricingDto } from '@/types/subscription';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

// DTOs matching backend
export interface CreateCurrencyPricingDto {
  currencyCode: string;
  monthlyPrice: number;
  yearlyPrice: number;
  stripePriceIdMonthly: string;
  stripePriceIdYearly: string;
  isActive: boolean;
}

export interface UpdateCurrencyPricingDto extends CreateCurrencyPricingDto {
  id: number;
}

// Get all currency pricings (including inactive) - Admin only
export function useAdminCurrencyPricings() {
  return useQuery<CurrencyPricingDto[]>({
    queryKey: ['admin', 'currency-pricing'],
    queryFn: async () => {
      const response = await axios.get(`${API_BASE_URL}/api/admin/currency-pricing`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
        },
      });
      return response.data;
    },
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

// Get single currency pricing by ID - Admin only
export function useAdminCurrencyPricing(id: number) {
  return useQuery<CurrencyPricingDto>({
    queryKey: ['admin', 'currency-pricing', id],
    queryFn: async () => {
      const response = await axios.get(`${API_BASE_URL}/api/admin/currency-pricing/${id}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
        },
      });
      return response.data;
    },
    enabled: !!id,
    staleTime: 2 * 60 * 1000,
  });
}

// Create currency pricing - Admin only
export function useCreateCurrencyPricing() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateCurrencyPricingDto) => {
      const response = await axios.post(
        `${API_BASE_URL}/api/admin/currency-pricing`,
        data,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
            'Content-Type': 'application/json',
          },
        }
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'currency-pricing'] });
      queryClient.invalidateQueries({ queryKey: ['subscription', 'pricing'] });
    },
  });
}

// Update currency pricing - Admin only
export function useUpdateCurrencyPricing() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: UpdateCurrencyPricingDto) => {
      const response = await axios.put(
        `${API_BASE_URL}/api/admin/currency-pricing/${data.id}`,
        data,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
            'Content-Type': 'application/json',
          },
        }
      );
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'currency-pricing'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'currency-pricing', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['subscription', 'pricing'] });
    },
  });
}

// Delete currency pricing (soft delete) - Admin only
export function useDeleteCurrencyPricing() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await axios.delete(`${API_BASE_URL}/api/admin/currency-pricing/${id}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
        },
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'currency-pricing'] });
      queryClient.invalidateQueries({ queryKey: ['subscription', 'pricing'] });
    },
  });
}
