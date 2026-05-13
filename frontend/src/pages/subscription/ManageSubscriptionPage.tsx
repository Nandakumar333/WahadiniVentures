import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/auth/useAuth';
import { useSubscription } from '@/hooks/useSubscription';
import { SubscriptionStatus, SubscriptionTier } from '@/types/subscription';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { CircleAlert, CreditCard, Calendar, CheckCircle2 } from 'lucide-react';

/**
 * Subscription Management Page - US3
 * Allows users to:
 * - View current subscription status and details
 * - Access Stripe billing portal for payment method updates
 * - Cancel subscription (scheduled at period end)
 * - Reactivate cancelled subscription
 * - See grace period warnings for past due subscriptions
 */
export const ManageSubscriptionPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { 
    subscription, 
    isLoading, 
    createPortalSession, 
    cancelSubscription, 
    reactivateSubscription 
  } = useSubscription();

  const [showCancelDialog, setShowCancelDialog] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);

  // Redirect if not authenticated
  React.useEffect(() => {
    if (!user) {
      navigate('/login');
    }
  }, [user, navigate]);

  // Handle billing portal access
  const handleBillingPortal = async () => {
    try {
      setIsProcessing(true);
      const returnUrl = `${window.location.origin}/subscription/manage`;
      await createPortalSession.mutateAsync({ returnUrl });
    } catch (error) {
      console.error('Failed to open billing portal:', error);
    } finally {
      setIsProcessing(false);
    }
  };

  // Handle subscription cancellation
  const handleCancelConfirm = async () => {
    try {
      setIsProcessing(true);
      await cancelSubscription.mutateAsync({ reason: cancelReason });
      setShowCancelDialog(false);
      setCancelReason('');
    } catch (error) {
      console.error('Failed to cancel subscription:', error);
    } finally {
      setIsProcessing(false);
    }
  };

  // Handle subscription reactivation
  const handleReactivate = async () => {
    try {
      setIsProcessing(true);
      await reactivateSubscription.mutateAsync();
    } catch (error) {
      console.error('Failed to reactivate subscription:', error);
    } finally {
      setIsProcessing(false);
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!subscription) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card>
          <CardHeader>
            <CardTitle>No Active Subscription</CardTitle>
            <CardDescription>You don't have an active subscription.</CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={() => navigate('/pricing')}>
              View Pricing Plans
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const isPastDue = subscription.status === SubscriptionStatus.PastDue;
  const isCancelled = subscription.isCancelledAtPeriodEnd;
  const isActive = subscription.status === SubscriptionStatus.Active;

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Manage Subscription</h1>
        <p className="text-muted-foreground">View and manage your WahadiniCryptoQuest subscription</p>
      </div>

      {/* Grace Period Warning for Past Due */}
      {isPastDue && (
        <Alert variant="destructive" className="mb-6">
          <CircleAlert className="h-4 w-4" />
          <AlertDescription>
            <strong>Payment Past Due</strong>
            <br />
            Your payment method was declined. Please update your payment information to avoid service interruption. 
            You have until {new Date(subscription.currentPeriodEnd).toLocaleDateString()} before your access is suspended.
          </AlertDescription>
        </Alert>
      )}

      {/* Cancellation Notice */}
      {isCancelled && (
        <Alert className="mb-6 border-yellow-500 bg-yellow-50 dark:bg-yellow-950">
          <CircleAlert className="h-4 w-4 text-yellow-600" />
          <AlertDescription className="text-yellow-800 dark:text-yellow-200">
            <strong>Subscription Cancelled</strong>
            <br />
            Your subscription will end on {new Date(subscription.currentPeriodEnd).toLocaleDateString()}. 
            You'll retain access until then. You can reactivate your subscription at any time before this date.
          </AlertDescription>
        </Alert>
      )}

      {/* Subscription Details Card */}
      <Card className="mb-6">
        <CardHeader>
          <CardTitle>Subscription Details</CardTitle>
          <CardDescription>Current plan and billing information</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Status */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              {isActive && <CheckCircle2 className="h-5 w-5 text-green-500" />}
              {isPastDue && <CircleAlert className="h-5 w-5 text-red-500" />}
              {isCancelled && <CircleAlert className="h-5 w-5 text-yellow-500" />}
              <span className="font-medium">Status</span>
            </div>
            <span className={`font-semibold ${
              isActive ? 'text-green-600' : 
              isPastDue ? 'text-red-600' : 
              'text-yellow-600'
            }`}>
              {subscription.status}
              {isCancelled && ' (Cancels at period end)'}
            </span>
          </div>

          {/* Plan */}
          <div className="flex items-center justify-between">
            <span className="font-medium">Plan</span>
            <span className="text-muted-foreground">
              {subscription.tier === SubscriptionTier.YearlyPremium ? 'Premium Annual' : 'Premium Monthly'}
            </span>
          </div>

          {/* Billing Cycle */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Calendar className="h-5 w-5 text-muted-foreground" />
              <span className="font-medium">Billing Cycle</span>
            </div>
            <span className="text-muted-foreground">
              {subscription.tier === SubscriptionTier.YearlyPremium ? 'Yearly' : 'Monthly'}
            </span>
          </div>

          {/* Next Billing Date */}
          {!isCancelled && (
            <div className="flex items-center justify-between">
              <span className="font-medium">Next Billing Date</span>
              <span className="text-muted-foreground">
                {new Date(subscription.currentPeriodEnd).toLocaleDateString()}
              </span>
            </div>
          )}

          {/* End Date (if cancelled) */}
          {isCancelled && (
            <div className="flex items-center justify-between">
              <span className="font-medium">Access Ends</span>
              <span className="text-muted-foreground">
                {new Date(subscription.currentPeriodEnd).toLocaleDateString()}
              </span>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Actions Card */}
      <Card>
        <CardHeader>
          <CardTitle>Subscription Actions</CardTitle>
          <CardDescription>Manage your subscription and payment methods</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Billing Portal Button */}
          <div className="flex items-start justify-between gap-4">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-1">
                <CreditCard className="h-5 w-5 text-muted-foreground" />
                <h3 className="font-semibold">Update Payment Method</h3>
              </div>
              <p className="text-sm text-muted-foreground">
                Access the billing portal to update your payment information, view invoices, and manage billing details.
              </p>
            </div>
            <Button 
              onClick={handleBillingPortal}
              disabled={isProcessing || createPortalSession.isPending}
              variant="outline"
            >
              {createPortalSession.isPending ? 'Opening...' : 'Billing Portal'}
            </Button>
          </div>

          <div className="border-t pt-4" />

          {/* Cancel/Reactivate Button */}
          {!isCancelled ? (
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1">
                <h3 className="font-semibold mb-1">Cancel Subscription</h3>
                <p className="text-sm text-muted-foreground">
                  You'll retain access until the end of your billing period. No refunds for partial periods.
                </p>
              </div>
              <Button 
                onClick={() => setShowCancelDialog(true)}
                disabled={isProcessing}
                variant="destructive"
              >
                Cancel Subscription
              </Button>
            </div>
          ) : (
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1">
                <h3 className="font-semibold mb-1">Reactivate Subscription</h3>
                <p className="text-sm text-muted-foreground">
                  Resume your subscription and continue enjoying premium access. You won't be charged until the next billing period.
                </p>
              </div>
              <Button 
                onClick={handleReactivate}
                disabled={isProcessing || reactivateSubscription.isPending}
                variant="default"
              >
                {reactivateSubscription.isPending ? 'Reactivating...' : 'Reactivate'}
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Cancellation Dialog */}
      <Dialog open={showCancelDialog} onOpenChange={setShowCancelDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel Subscription</DialogTitle>
            <DialogDescription>
              Are you sure you want to cancel your subscription? You'll retain access until the end of your current billing period.
            </DialogDescription>
          </DialogHeader>
          
          <div className="py-4">
            <label htmlFor="cancel-reason" className="text-sm font-medium mb-2 block">
              Help us improve (optional)
            </label>
            <Textarea
              id="cancel-reason"
              placeholder="Please tell us why you're cancelling..."
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              rows={4}
            />
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setShowCancelDialog(false);
                setCancelReason('');
              }}
              disabled={isProcessing}
            >
              Keep Subscription
            </Button>
            <Button
              variant="destructive"
              onClick={handleCancelConfirm}
              disabled={isProcessing || cancelSubscription.isPending}
            >
              {cancelSubscription.isPending ? 'Cancelling...' : 'Confirm Cancellation'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};
