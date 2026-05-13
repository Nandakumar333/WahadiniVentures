import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';
import type {
  SubscriptionStatusDto,
  CurrencyPricingDto,
  CreateCheckoutSessionDto,
  CheckoutSessionResponseDto,
  CreatePortalSessionDto,
  PortalSessionResponseDto,
  CancelSubscriptionDto,
} from '../types/subscription';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

// Get user's subscription status
export function useSubscriptionStatus() {
  return useQuery<SubscriptionStatusDto | null>({
    queryKey: ['subscription', 'status'],
    queryFn: async () => {
      try {
        const response = await axios.get(`${API_BASE_URL}/subscriptions/status`, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          },
        });
        return response.data;
      } catch (error: any) {
        if (error.response?.status === 204) {
          return null; // No subscription
        }
        throw error;
      }
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 1,
  });
}

// Get all active currency pricings (public endpoint)
export function useCurrencyPricings() {
  return useQuery<CurrencyPricingDto[]>({
    queryKey: ['subscription', 'pricing'],
    queryFn: async () => {
      const response = await axios.get(`${API_BASE_URL}/subscriptions/pricing`);
      return response.data;
    },
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

// Create checkout session
export function useCreateCheckoutSession() {
  return useMutation<CheckoutSessionResponseDto, Error, CreateCheckoutSessionDto>({
    mutationFn: async (data: CreateCheckoutSessionDto) => {
      const response = await axios.post(
        `${API_BASE_URL}/subscriptions/checkout`,
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
  });
}

// Create billing portal session
export function useCreatePortalSession() {
  return useMutation<PortalSessionResponseDto, Error, CreatePortalSessionDto>({
    mutationFn: async (data: CreatePortalSessionDto) => {
      const response = await axios.post(
        `${API_BASE_URL}/subscriptions/portal`,
        data,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
            'Content-Type': 'application/json',
          },
        }
      );
      // Redirect to portal URL
      if (response.data?.portalUrl) {
        window.location.href = response.data.portalUrl;
      }
      return response.data;
    },
  });
}

// Cancel subscription
export function useCancelSubscription() {
  const queryClient = useQueryClient();
  
  return useMutation<{ message: string }, Error, CancelSubscriptionDto>({
    mutationFn: async (data: CancelSubscriptionDto) => {
      const response = await axios.post(
        `${API_BASE_URL}/subscriptions/cancel`,
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
      // Invalidate subscription status to refresh
      queryClient.invalidateQueries({ queryKey: ['subscription', 'status'] });
    },
  });
}

// Reactivate subscription
export function useReactivateSubscription() {
  const queryClient = useQueryClient();
  
  return useMutation<{ message: string }, Error, void>({
    mutationFn: async () => {
      const response = await axios.post(
        `${API_BASE_URL}/subscriptions/reactivate`,
        {},
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
      // Invalidate subscription status to refresh
      queryClient.invalidateQueries({ queryKey: ['subscription', 'status'] });
    },
  });
}

// Consolidated hook for subscription management
export function useSubscription() {
  const { data: subscription, isLoading } = useSubscriptionStatus();
  const createPortalSession = useCreatePortalSession();
  const cancelSubscription = useCancelSubscription();
  const reactivateSubscription = useReactivateSubscription();

  return {
    subscription,
    isLoading,
    createPortalSession,
    cancelSubscription,
    reactivateSubscription,
  };
}
