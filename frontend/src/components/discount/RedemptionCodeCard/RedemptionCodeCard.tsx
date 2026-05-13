import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Check, Copy, Calendar, Percent } from 'lucide-react';
import type { UserRedemption } from '../../../types/discount.types';
import { toast } from 'sonner';

interface RedemptionCodeCardProps {
  redemption: UserRedemption;
}

/**
 * Card component displaying a redeemed discount code with copy functionality
 */
export const RedemptionCodeCard = ({ redemption }: RedemptionCodeCardProps) => {
  const [copied, setCopied] = useState(false);

  const handleCopyCode = async () => {
    try {
      await navigator.clipboard.writeText(redemption.code);
      setCopied(true);
      toast.success('Code copied to clipboard!');
      
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      toast.error('Failed to copy code');
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  // Determine if code is expired
  const isExpired = redemption.expiryDate 
    ? new Date(redemption.expiryDate) < new Date() 
    : false;

  // Determine if code is used
  const isUsed = redemption.usedInSubscription;

  return (
    <Card className={`flex flex-col h-full ${isExpired ? 'opacity-60' : ''}`}>
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <CardTitle className="text-xl font-bold flex items-center gap-2">
              <Percent className="h-5 w-5 text-primary" />
              {redemption.discountPercentage}% OFF
            </CardTitle>
            <CardDescription className="text-xs mt-1">
              Redeemed on {formatDate(redemption.redeemedAt)}
            </CardDescription>
          </div>
          <div className="flex flex-col gap-1">
            {isUsed && (
              <Badge variant="secondary" className="shrink-0">
                Used
              </Badge>
            )}
            {isExpired ? (
              <Badge variant="destructive" className="shrink-0">
                Expired
              </Badge>
            ) : (
              <Badge variant="default" className="shrink-0">
                Active
              </Badge>
            )}
          </div>
        </div>
      </CardHeader>

      <CardContent className="flex-grow space-y-3">
        {/* Discount Code Display */}
        <div className="p-4 bg-muted rounded-lg border-2 border-dashed border-border">
          <p className="text-xs text-muted-foreground mb-1">Your Discount Code:</p>
          <div className="flex items-center justify-between gap-2">
            <code className="text-lg font-mono font-bold tracking-wider">
              {redemption.code}
            </code>
            <Button
              size="icon"
              variant="ghost"
              onClick={handleCopyCode}
              disabled={isExpired}
              className="shrink-0"
              title="Copy code"
            >
              {copied ? (
                <Check className="h-4 w-4 text-green-600" />
              ) : (
                <Copy className="h-4 w-4" />
              )}
            </Button>
          </div>
        </div>

        {/* Expiry Information */}
        {redemption.expiryDate && (
          <div className="flex items-center gap-2 text-sm">
            <Calendar className="h-4 w-4 text-muted-foreground" />
            <span className="text-muted-foreground">Expires:</span>
            <span className={`font-semibold ${isExpired ? 'text-destructive' : 'text-foreground'}`}>
              {formatDate(redemption.expiryDate)}
            </span>
          </div>
        )}

        {/* Usage Instructions */}
        {!isExpired && !isUsed && (
          <p className="text-xs text-muted-foreground bg-blue-50 dark:bg-blue-950 p-2 rounded">
            💡 Use this code during checkout to apply your discount to your subscription.
          </p>
        )}

        {isExpired && (
          <p className="text-xs text-destructive bg-red-50 dark:bg-red-950 p-2 rounded">
            ⚠️ This code has expired and can no longer be used.
          </p>
        )}

        {isUsed && (
          <p className="text-xs text-green-700 bg-green-50 dark:bg-green-950 p-2 rounded">
            ✓ This code has been successfully applied to your subscription.
          </p>
        )}
      </CardContent>

      <CardFooter>
        <Button
          onClick={handleCopyCode}
          disabled={isExpired}
          variant={isExpired ? 'secondary' : 'default'}
          className="w-full"
        >
          {copied ? (
            <>
              <Check className="mr-2 h-4 w-4" />
              Copied!
            </>
          ) : (
            <>
              <Copy className="mr-2 h-4 w-4" />
              Copy Code
            </>
          )}
        </Button>
      </CardFooter>
    </Card>
  );
};
