import { useState } from 'react';
import { Search, Filter, ChevronLeft, ChevronRight, XCircle } from 'lucide-react';
import UserTable from '../../components/admin/UserTable';
import { useUserManagement } from '../../hooks/useUserManagement';

/**
 * User management page
 * T072-T075: US3 - User Account Management
 */
const UserManagement = () => {
  const [filters, setFilters] = useState({
    searchTerm: '',
    role: undefined as number | undefined,
    isActive: undefined as boolean | undefined,
    isBanned: undefined as boolean | undefined,
    emailConfirmed: undefined as boolean | undefined,
    hasActiveSubscription: undefined as boolean | undefined,
    pageNumber: 1,
    pageSize: 20,
    sortBy: 'CreatedAt',
    sortDescending: true
  });

  const [showRoleModal, setShowRoleModal] = useState(false);
  const [showBanModal, setShowBanModal] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [roleData, setRoleData] = useState({ role: 0, reason: '' });
  const [banReason, setBanReason] = useState('');

  const { users, pagination, isLoading, error, updateRole, banUser, unbanUser, isUpdating } = useUserManagement(filters);

  const handleViewUser = (userId: string) => {
    // Navigate to user detail page
    window.location.href = `/admin/users/${userId}`;
  };

  const handleUpdateRole = (userId: string) => {
    setSelectedUserId(userId);
    setRoleData({ role: 0, reason: '' });
    setShowRoleModal(true);
  };

  const handleBanUser = (userId: string) => {
    const user = users.find(u => u.id === userId);
    if (user?.isBanned) {
      // Unban directly
      handleUnban(userId);
    } else {
      setSelectedUserId(userId);
      setBanReason('');
      setShowBanModal(true);
    }
  };

  const submitRoleUpdate = async () => {
    if (!selectedUserId) return;

    try {
      await updateRole({ userId: selectedUserId, data: roleData });
      setShowRoleModal(false);
      setSelectedUserId(null);
    } catch (err) {
      console.error('Failed to update role:', err);
      alert('Failed to update user role. Please try again.');
    }
  };

  const submitBan = async () => {
    if (!selectedUserId || !banReason.trim()) {
      alert('Ban reason is required');
      return;
    }

    try {
      await banUser({ userId: selectedUserId, data: { reason: banReason } });
      setShowBanModal(false);
      setSelectedUserId(null);
      setBanReason('');
    } catch (err) {
      console.error('Failed to ban user:', err);
      alert('Failed to ban user. Please try again.');
    }
  };

  const handleUnban = async (userId: string) => {
    if (!confirm('Are you sure you want to unban this user?')) return;

    try {
      await unbanUser(userId);
    } catch (err) {
      console.error('Failed to unban user:', err);
      alert('Failed to unban user. Please try again.');
    }
  };

  const handlePageChange = (newPage: number) => {
    setFilters({ ...filters, pageNumber: newPage });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">User Management</h1>
        <p className="text-gray-600 mt-2">
          Manage user accounts, roles, and access
        </p>
      </div>

      {/* Filters */}
      <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
        <div className="flex items-center gap-2 mb-4">
          <Filter className="w-5 h-5 text-gray-500" />
          <h2 className="text-lg font-semibold text-gray-900">Filters</h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Search
            </label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                value={filters.searchTerm}
                onChange={(e) => setFilters({ ...filters, searchTerm: e.target.value, pageNumber: 1 })}
                placeholder="Search by email or name..."
                className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Role
            </label>
            <select
              value={filters.role ?? ''}
              onChange={(e) => setFilters({ ...filters, role: e.target.value ? Number(e.target.value) : undefined, pageNumber: 1 })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
              <option value="">All Roles</option>
              <option value="0">Free</option>
              <option value="1">Premium</option>
              <option value="2">Admin</option>
              <option value="3">SuperAdmin</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <select
              value={filters.isBanned === undefined ? '' : filters.isBanned ? 'banned' : 'active'}
              onChange={(e) => setFilters({ 
                ...filters, 
                isBanned: e.target.value === '' ? undefined : e.target.value === 'banned',
                pageNumber: 1 
              })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
              <option value="">All Status</option>
              <option value="active">Active</option>
              <option value="banned">Banned</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <select
              value={filters.emailConfirmed === undefined ? '' : filters.emailConfirmed ? 'confirmed' : 'unconfirmed'}
              onChange={(e) => setFilters({ 
                ...filters, 
                emailConfirmed: e.target.value === '' ? undefined : e.target.value === 'confirmed',
                pageNumber: 1 
              })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
              <option value="">All</option>
              <option value="confirmed">Confirmed</option>
              <option value="unconfirmed">Unconfirmed</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Page Size
            </label>
            <select
              value={filters.pageSize}
              onChange={(e) => setFilters({ ...filters, pageSize: Number(e.target.value), pageNumber: 1 })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
              <option value={100}>100</option>
            </select>
          </div>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">
            Failed to load users. {(error as Error).message}
          </p>
        </div>
      )}

      {/* User Table */}
      <UserTable
        users={users}
        isLoading={isLoading}
        onViewUser={handleViewUser}
        onBanUser={handleBanUser}
        onUpdateRole={handleUpdateRole}
      />

      {/* Pagination */}
      {pagination && pagination.totalPages > 1 && (
        <div className="bg-white rounded-lg shadow p-4 flex items-center justify-between">
          <div className="text-sm text-gray-700">
            Showing {((pagination.pageNumber - 1) * pagination.pageSize) + 1} to{' '}
            {Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalCount)} of{' '}
            {pagination.totalCount} users
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => handlePageChange(pagination.pageNumber - 1)}
              disabled={!pagination.hasPreviousPage}
              className="px-3 py-1 border border-gray-300 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
            >
              <ChevronLeft className="w-5 h-5" />
            </button>
            <span className="px-3 py-1 border border-gray-300 rounded-md bg-purple-50 text-purple-700">
              {pagination.pageNumber} / {pagination.totalPages}
            </span>
            <button
              onClick={() => handlePageChange(pagination.pageNumber + 1)}
              disabled={!pagination.hasNextPage}
              className="px-3 py-1 border border-gray-300 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
            >
              <ChevronRight className="w-5 h-5" />
            </button>
          </div>
        </div>
      )}

      {/* Role Update Modal */}
      {showRoleModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="flex justify-between items-center p-6 border-b border-gray-200">
              <h2 className="text-xl font-bold text-gray-900">Update User Role</h2>
              <button
                onClick={() => setShowRoleModal(false)}
                disabled={isUpdating}
                className="text-gray-500 hover:text-gray-700"
              >
                <XCircle className="w-6 h-6" />
              </button>
            </div>

            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  New Role
                </label>
                <select
                  value={roleData.role}
                  onChange={(e) => setRoleData({ ...roleData, role: Number(e.target.value) })}
                  disabled={isUpdating}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
                >
                  <option value={0}>Free</option>
                  <option value={1}>Premium</option>
                  <option value={2}>Admin</option>
                  <option value={3}>SuperAdmin</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Reason (Optional)
                </label>
                <textarea
                  value={roleData.reason}
                  onChange={(e) => setRoleData({ ...roleData, reason: e.target.value })}
                  placeholder="Explain why you're changing this user's role..."
                  rows={3}
                  disabled={isUpdating}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
                />
              </div>
            </div>

            <div className="flex gap-3 p-6 border-t border-gray-200">
              <button
                onClick={() => setShowRoleModal(false)}
                disabled={isUpdating}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={submitRoleUpdate}
                disabled={isUpdating}
                className="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-md disabled:opacity-50"
              >
                {isUpdating ? 'Updating...' : 'Update Role'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Ban Modal */}
      {showBanModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="flex justify-between items-center p-6 border-b border-gray-200">
              <h2 className="text-xl font-bold text-gray-900">Ban User</h2>
              <button
                onClick={() => setShowBanModal(false)}
                disabled={isUpdating}
                className="text-gray-500 hover:text-gray-700"
              >
                <XCircle className="w-6 h-6" />
              </button>
            </div>

            <div className="p-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Ban Reason (Required)
              </label>
              <textarea
                value={banReason}
                onChange={(e) => setBanReason(e.target.value)}
                placeholder="Explain why you're banning this user..."
                rows={4}
                disabled={isUpdating}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
              />
              <p className="text-xs text-gray-500 mt-1">Max 500 characters</p>
            </div>

            <div className="flex gap-3 p-6 border-t border-gray-200">
              <button
                onClick={() => setShowBanModal(false)}
                disabled={isUpdating}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={submitBan}
                disabled={isUpdating}
                className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md disabled:opacity-50"
              >
                {isUpdating ? 'Banning...' : 'Ban User'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default UserManagement;
