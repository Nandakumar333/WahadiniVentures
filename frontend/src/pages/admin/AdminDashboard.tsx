import { Users, DollarSign, ClipboardCheck, Crown } from 'lucide-react';
import KPICard from '@/components/admin/KPICard';
import TrendChart from '@/components/admin/TrendChart';
import { useAdminStats } from '@/hooks/useAdminStats';

/**
 * AdminDashboard Page - Platform Health Overview
 * T036: Displays KPI cards and analytics charts for platform metrics (US1)
 */
const AdminDashboard = () => {
  const { data: stats, isLoading, error } = useAdminStats();

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-6">
        <h3 className="text-red-800 font-semibold mb-2">Error Loading Dashboard</h3>
        <p className="text-red-600 text-sm">{error.message}</p>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Platform Overview</h2>
        <p className="text-gray-600 text-sm mt-1">Real-time metrics and 30-day trends</p>
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <KPICard
          title="Total Users"
          value={stats?.totalUsers.toLocaleString() ?? '--'}
          icon={Users}
          loading={isLoading}
        />
        <KPICard
          title="Active Subscribers"
          value={stats?.activeSubscribers.toLocaleString() ?? '--'}
          icon={Crown}
          loading={isLoading}
        />
        <KPICard
          title="Monthly Recurring Revenue"
          value={stats ? `$${stats.monthlyRecurringRevenue.toLocaleString()}` : '--'}
          icon={DollarSign}
          loading={isLoading}
        />
        <KPICard
          title="Pending Tasks"
          value={stats?.pendingTasks.toLocaleString() ?? '--'}
          icon={ClipboardCheck}
          loading={isLoading}
        />
      </div>

      {/* Trend Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <TrendChart
          title="User Growth (30 Days)"
          data={stats?.userGrowthTrend ?? []}
          color="#8b5cf6"
          loading={isLoading}
        />
        <TrendChart
          title="Revenue Trend (30 Days)"
          data={stats?.revenueTrend ?? []}
          color="#10b981"
          valuePrefix="$"
          loading={isLoading}
        />
      </div>
    </div>
  );
};

export default AdminDashboard;
