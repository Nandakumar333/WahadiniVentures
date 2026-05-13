import { AnalyticsCard } from '../AnalyticsCard';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Coins, TrendingUp, Users, Percent } from 'lucide-react';
import type { AnalyticsSummaryDto } from '@/types/discount.types';

export interface DiscountAnalyticsDashboardProps {
  analytics: AnalyticsSummaryDto;
}

export const DiscountAnalyticsDashboard: React.FC<DiscountAnalyticsDashboardProps> = ({
  analytics,
}) => {
  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <AnalyticsCard
          title="Total Discount Codes"
          value={analytics.totalDiscountCodes}
          description={`${analytics.activeDiscountCodes} active`}
          icon={Percent}
        />
        <AnalyticsCard
          title="Total Redemptions"
          value={analytics.totalRedemptions.toLocaleString()}
          description="All-time redemptions"
          icon={TrendingUp}
        />
        <AnalyticsCard
          title="Points Burned"
          value={analytics.totalPointsBurned.toLocaleString()}
          description="Total points redeemed"
          icon={Coins}
        />
        <AnalyticsCard
          title="Unique Users"
          value={analytics.uniqueRedeemingUsers.toLocaleString()}
          description="Users who redeemed"
          icon={Users}
        />
      </div>

      {/* Top Performing Discounts Table */}
      <Card>
        <CardHeader>
          <CardTitle>Top Performing Discount Codes</CardTitle>
        </CardHeader>
        <CardContent>
          {analytics.topPerformingDiscounts.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead className="text-right">Redemptions</TableHead>
                  <TableHead className="text-right">Points Burned</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {analytics.topPerformingDiscounts.map((discount) => (
                  <TableRow key={discount.id}>
                    <TableCell className="font-mono font-semibold">
                      {discount.code}
                    </TableCell>
                    <TableCell className="text-right">
                      {discount.redemptionCount.toLocaleString()}
                    </TableCell>
                    <TableCell className="text-right">
                      {discount.pointsBurned.toLocaleString()}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <p className="text-center text-muted-foreground py-8">
              No redemptions yet
            </p>
          )}
        </CardContent>
      </Card>

      {/* Date Range Info */}
      {analytics.earliestRedemptionDate && analytics.latestRedemptionDate && (
        <Card>
          <CardHeader>
            <CardTitle>Redemption Activity Period</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex justify-between text-sm">
              <div>
                <p className="text-muted-foreground">First Redemption</p>
                <p className="font-semibold">
                  {new Date(analytics.earliestRedemptionDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
                </p>
              </div>
              <div className="text-right">
                <p className="text-muted-foreground">Latest Redemption</p>
                <p className="font-semibold">
                  {new Date(analytics.latestRedemptionDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};
