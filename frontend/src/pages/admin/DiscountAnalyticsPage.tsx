import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { DiscountAnalyticsDashboard } from '@/components/admin/DiscountAnalyticsDashboard';
import { useAnalyticsSummary } from '@/hooks/admin/useDiscountAnalytics';
import { Loader2, AlertCircle } from 'lucide-react';

export const DiscountAnalyticsPage: React.FC = () => {
  const { data: analytics, isLoading, error } = useAnalyticsSummary();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (error) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          Failed to load analytics: {error.message}
        </AlertDescription>
      </Alert>
    );
  }

  if (!analytics) {
    return (
      <Alert>
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>No analytics data available</AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Discount Redemption Analytics</CardTitle>
        </CardHeader>
        <CardContent>
          <DiscountAnalyticsDashboard analytics={analytics} />
        </CardContent>
      </Card>
    </div>
  );
};
