import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import adminService from '../services/adminService';
import type { PaginatedUsersDto, UserDetailDto, UpdateUserRoleDto, BanUserDto } from '../types/admin.types';

/**
 * Hook for user management operations
 * T076: US3 - User Account Management
 */
function useUserManagement(filters?: {
  searchTerm?: string;
  role?: number;
  isActive?: boolean;
  isBanned?: boolean;
  emailConfirmed?: boolean;
  hasActiveSubscription?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}) {
  const queryClient = useQueryClient();

  // Fetch users list
  const {
    data: usersData,
    isLoading,
    error,
    refetch
  } = useQuery<PaginatedUsersDto>({
    queryKey: ['admin', 'users', filters],
    queryFn: () => adminService.getUsers(filters),
    staleTime: 1000 * 60 * 2, // 2 minutes
    gcTime: 1000 * 60 * 5, // 5 minutes
  });

  // Update role mutation
  const updateRoleMutation = useMutation({
    mutationFn: ({ userId, data }: { userId: string; data: UpdateUserRoleDto }) =>
      adminService.updateUserRole(userId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'users'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'stats'] });
    }
  });

  // Ban user mutation
  const banUserMutation = useMutation({
    mutationFn: ({ userId, data }: { userId: string; data: BanUserDto }) =>
      adminService.banUser(userId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'users'] });
    }
  });

  // Unban user mutation
  const unbanUserMutation = useMutation({
    mutationFn: (userId: string) => adminService.unbanUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'users'] });
    }
  });

  return {
    users: usersData?.users || [],
    pagination: usersData ? {
      totalCount: usersData.totalCount,
      pageNumber: usersData.pageNumber,
      pageSize: usersData.pageSize,
      totalPages: usersData.totalPages,
      hasPreviousPage: usersData.hasPreviousPage,
      hasNextPage: usersData.hasNextPage
    } : null,
    isLoading,
    error,
    refetch,
    updateRole: updateRoleMutation.mutateAsync,
    banUser: banUserMutation.mutateAsync,
    unbanUser: unbanUserMutation.mutateAsync,
    isUpdating: updateRoleMutation.isPending || banUserMutation.isPending || unbanUserMutation.isPending
  };
}

/**
 * Hook for fetching individual user details
 */
function useUserDetail(userId?: string) {
  return useQuery<UserDetailDto>({
    queryKey: ['admin', 'users', userId],
    queryFn: () => adminService.getUserById(userId!),
    enabled: !!userId,
    staleTime: 1000 * 60, // 1 minute
  });
}

export { useUserManagement, useUserDetail };
