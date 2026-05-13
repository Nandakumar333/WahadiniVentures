/**
 * Discount Code Management Page
 * T153-T154: US5 - Reward System Management
 */
import { useState } from 'react';
import { Plus, Eye, X } from 'lucide-react';
import { useDiscountManagement } from '../../hooks/useDiscountManagement';
import type { CreateDiscountCodeDto } from '../../types/admin.types';

export function DiscountManagement() {
  const { discountCodes, isLoading, createDiscount, isCreating, useRedemptionLogs } = useDiscountManagement();
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [viewingCode, setViewingCode] = useState<string | null>(null);
  const [discountForm, setDiscountForm] = useState<CreateDiscountCodeDto>({
    code: '',
    discountPercentage: 0,
    requiredPoints: 0,
    expirationDate: '',
    usageLimit: 0,
  });

  const { data: redemptions = [] } = useRedemptionLogs(viewingCode || '');

  const handleCreate = () => {
    setDiscountForm({
      code: '',
      discountPercentage: 0,
      requiredPoints: 0,
      expirationDate: '',
      usageLimit: 0,
    });
    setShowCreateModal(true);
  };

  const submitDiscount = async () => {
    // Validate required fields
    if (!discountForm.code || discountForm.discountPercentage < 0 || discountForm.discountPercentage > 100) {
      alert('Please fill in all required fields correctly');
      return;
    }

    try {
      await createDiscount(discountForm);
      setShowCreateModal(false);
    } catch (error) {
      console.error('Error creating discount code:', error);
      alert('Failed to create discount code');
    }
  };

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      Active: 'bg-green-100 text-green-800',
      Expired: 'bg-gray-100 text-gray-800',
      FullyRedeemed: 'bg-blue-100 text-blue-800',
      Inactive: 'bg-red-100 text-red-800',
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Discount Codes</h1>
          <p className="text-gray-600 mt-1">Manage discount codes and redemptions</p>
        </div>
        <button
          onClick={handleCreate}
          className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          <Plus className="w-4 h-4 mr-2" />
          Create Code
        </button>
      </div>

      {/* Discount Codes Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Code</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Discount</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Required Points</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Expiration</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Usage</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {isLoading ? (
              <tr>
                <td colSpan={7} className="px-6 py-4 text-center text-gray-500">
                  Loading...
                </td>
              </tr>
            ) : discountCodes.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-6 py-4 text-center text-gray-500">
                  No discount codes found
                </td>
              </tr>
            ) : (
              discountCodes.map((code: any) => (
                <tr key={code.id}>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="font-medium text-gray-900">{code.code}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm font-semibold text-gray-900">{code.discountPercentage}%</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm text-gray-900">{code.requiredPoints} pts</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm text-gray-500">
                      {code.expirationDate
                        ? new Date(code.expirationDate).toLocaleDateString()
                        : 'No expiration'}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm text-gray-900">
                      {code.usageCount} / {code.usageLimit === 0 ? '∞' : code.usageLimit}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusBadge(code.status)}`}>
                      {code.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <button
                      onClick={() => setViewingCode(code.code)}
                      className="text-blue-600 hover:text-blue-900"
                      title="View Redemptions"
                    >
                      <Eye className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Create Discount Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold">Create Discount Code</h2>
              <button onClick={() => setShowCreateModal(false)} className="text-gray-500 hover:text-gray-700">
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Code <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={discountForm.code}
                  onChange={(e) => setDiscountForm({ ...discountForm, code: e.target.value.toUpperCase() })}
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="SAVE20"
                  maxLength={20}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Discount Percentage (0-100) <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  value={discountForm.discountPercentage}
                  onChange={(e) => setDiscountForm({ ...discountForm, discountPercentage: parseInt(e.target.value) || 0 })}
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                  max="100"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Required Points</label>
                <input
                  type="number"
                  value={discountForm.requiredPoints}
                  onChange={(e) => setDiscountForm({ ...discountForm, requiredPoints: parseInt(e.target.value) || 0 })}
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Expiration Date (Optional)</label>
                <input
                  type="date"
                  value={discountForm.expirationDate || ''}
                  onChange={(e) => setDiscountForm({ ...discountForm, expirationDate: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                  min={new Date().toISOString().split('T')[0]}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Usage Limit (0 = unlimited)
                </label>
                <input
                  type="number"
                  value={discountForm.usageLimit}
                  onChange={(e) => setDiscountForm({ ...discountForm, usageLimit: parseInt(e.target.value) || 0 })}
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>
            </div>

            <div className="mt-6 flex gap-3">
              <button
                onClick={submitDiscount}
                disabled={isCreating}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400"
              >
                {isCreating ? 'Creating...' : 'Create Code'}
              </button>
              <button
                onClick={() => setShowCreateModal(false)}
                className="px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Redemptions Modal */}
      {viewingCode && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-2xl max-h-[80vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold">Redemptions for {viewingCode}</h2>
              <button onClick={() => setViewingCode(null)} className="text-gray-500 hover:text-gray-700">
                <X className="w-5 h-5" />
              </button>
            </div>

            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">User</th>
                  <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Discount</th>
                  <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Redeemed At</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {redemptions.length === 0 ? (
                  <tr>
                    <td colSpan={3} className="px-4 py-4 text-center text-gray-500">
                      No redemptions yet
                    </td>
                  </tr>
                ) : (
                  redemptions.map((redemption: any, index: number) => (
                    <tr key={index}>
                      <td className="px-4 py-2 text-sm text-gray-900">{redemption.username}</td>
                      <td className="px-4 py-2 text-sm text-gray-900">{redemption.discountAmount}%</td>
                      <td className="px-4 py-2 text-sm text-gray-500">
                        {new Date(redemption.redeemedAt).toLocaleString()}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>

            <div className="mt-4">
              <button
                onClick={() => setViewingCode(null)}
                className="w-full px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
