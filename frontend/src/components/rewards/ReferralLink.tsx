import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { Copy, Check, Users, Gift, Clock, Loader2, AlertCircle, ExternalLink } from 'lucide-react';
import type { ReferralCodeDto } from '@/types/reward.types';

interface ReferralLinkProps {
  /** Referral code data from API */
  referralData: ReferralCodeDto;
  /** Base URL for referral links (e.g., 'https://app.example.com/register') */
  baseUrl?: string;
  /** Show detailed statistics */
  showStats?: boolean;
  /** Loading state */
  isLoading?: boolean;
  /** Error state */
  error?: Error | null;
}

/**
 * ReferralLink Component
 * 
 * Displays user's referral link with copy functionality and statistics
 * Features:
 * - One-click copy to clipboard with feedback
 * - Success/pending referral counts
 * - Bonus points information
 * - Shareable referral link
 * - Recent referrals list
 * - Social sharing buttons (optional)
 * - Loading and error states
 * 
 * @example
 * ```tsx
 * function ReferralPage() {
 *   const { data: referralData, isLoading, error } = useReferralInfo();
 *   
 *   return (
 *     <ReferralLink 
 *       referralData={referralData}
 *       baseUrl="https://wahadinicryptoquest.com/register"
 *       showStats={true}
 *     />
 *   );
 * }
 * ```
 */
export function ReferralLink({
  referralData,
  baseUrl = `${window.location.origin}/register`,
  showStats = true,
  isLoading = false,
  error = null,
}: ReferralLinkProps) {
  const [copied, setCopied] = useState(false);

  // Loading state
  if (isLoading) {
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-center gap-2 py-8">
            <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
            <span className="text-sm text-muted-foreground">Loading referral data...</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Error state
  if (error) {
    return (
      <Card className="border-destructive/50">
        <CardContent className="pt-6">
          <div className="flex items-center gap-2 py-8">
            <AlertCircle className="h-5 w-5 text-destructive" />
            <span className="text-sm text-destructive">Failed to load referral information</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!referralData) return null;

  const { 
    referralCode, 
    referralLink, 
    totalReferrals, 
    successfulReferrals,
    totalPointsEarned, 
    recentReferrals 
  } = referralData;
  
  const referralUrl = referralLink || `${baseUrl}?ref=${referralCode}`;
  
  // Use successfulReferrals from backend if available, fallback to totalReferrals or 0
  const completedCount = successfulReferrals ?? totalReferrals ?? 0;
  
  // Calculate successful and pending from recentReferrals (with safe fallback)
  const successfulReferrals_calculated = recentReferrals?.filter(r => r.status === 'Completed').length || 0;
  const pendingReferrals = recentReferrals?.filter(r => r.status === 'Pending').length || 0;
  
  // Use calculated value if recentReferrals exists, otherwise use backend value
  const finalSuccessfulCount = recentReferrals ? successfulReferrals_calculated : completedCount;

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(referralUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="flex items-center gap-2">
              <Users className="w-5 h-5 text-purple-500" />
              Your Referral Link
            </CardTitle>
            <CardDescription className="mt-1.5">
              Share this link and earn 100 bonus points when someone completes their first course!
            </CardDescription>
          </div>
          <Badge variant="secondary" className="text-base px-4 py-2">
            <Gift className="w-4 h-4 mr-2 text-amber-500" />
            +100 pts
          </Badge>
        </div>
      </CardHeader>

      <CardContent className="space-y-6">
        {/* Referral code and link */}
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <label className="text-sm font-medium">Your Referral Code</label>
            <Badge variant="outline">{referralCode}</Badge>
          </div>
          
          <div className="flex gap-2">
            <Input
              readOnly
              value={referralUrl}
              className="font-mono text-sm"
              onClick={(e) => e.currentTarget.select()}
            />
            <Button
              variant="outline"
              size="icon"
              onClick={handleCopy}
              className="flex-shrink-0"
            >
              {copied ? (
                <Check className="h-4 w-4 text-green-500" />
              ) : (
                <Copy className="h-4 w-4" />
              )}
            </Button>
          </div>

          <p className="text-xs text-muted-foreground">
            Click the link to select all, or use the copy button
          </p>
        </div>

        {/* Statistics */}
        {showStats && (
          <>
            <Separator />
            
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              {/* Total referrals */}
              <div className="space-y-1">
                <p className="text-sm text-muted-foreground">Total Referrals</p>
                <div className="flex items-baseline gap-2">
                  <p className="text-3xl font-bold">{completedCount + pendingReferrals}</p>
                  <Users className="w-4 h-4 text-muted-foreground" />
                </div>
              </div>

              {/* Successful referrals */}
              <div className="space-y-1">
                <p className="text-sm text-muted-foreground">Completed</p>
                <div className="flex items-baseline gap-2">
                  <p className="text-3xl font-bold text-green-600">{finalSuccessfulCount}</p>
                  <Check className="w-4 h-4 text-green-600" />
                </div>
                <p className="text-xs text-muted-foreground">
                  {totalPointsEarned} points earned
                </p>
              </div>

              {/* Pending referrals */}
              <div className="space-y-1">
                <p className="text-sm text-muted-foreground">Pending</p>
                <div className="flex items-baseline gap-2">
                  <p className="text-3xl font-bold text-amber-600">{pendingReferrals}</p>
                  <Clock className="w-4 h-4 text-amber-600" />
                </div>
                <p className="text-xs text-muted-foreground">
                  Finish first course
                </p>
              </div>
            </div>
          </>
        )}

        {/* Recent referrals list */}
        {recentReferrals && recentReferrals.length > 0 && (
          <>
            <Separator />
            
            <div className="space-y-3">
              <h4 className="text-sm font-medium">Recent Referrals</h4>
              <div className="space-y-2">
                {recentReferrals.slice(0, 5).map((referral) => (
                  <div
                    key={referral.id}
                    className="flex items-center justify-between p-3 rounded-lg bg-muted/50"
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-gradient-to-br from-purple-400 to-purple-600 flex items-center justify-center text-white text-sm font-medium">
                        {referral.inviteeUsername.charAt(0).toUpperCase()}
                      </div>
                      <div>
                        <p className="text-sm font-medium">{referral.inviteeUsername}</p>
                        <p className="text-xs text-muted-foreground">
                          Joined {new Date(referral.createdAt).toLocaleDateString()}
                        </p>
                      </div>
                    </div>
                    <Badge
                      variant={referral.status === 'Completed' ? 'default' : 'secondary'}
                      className="text-xs"
                    >
                      {referral.status === 'Completed' ? (
                        <>
                          <Check className="w-3 h-3 mr-1" />
                          +{referral.pointsAwarded} pts
                        </>
                      ) : (
                        <>
                          <Clock className="w-3 h-3 mr-1" />
                          Pending
                        </>
                      )}
                    </Badge>
                  </div>
                ))}
              </div>
              
              {recentReferrals.length > 5 && (
                <Button variant="ghost" size="sm" className="w-full">
                  View all {recentReferrals.length} referrals
                  <ExternalLink className="w-3 h-3 ml-2" />
                </Button>
              )}
            </div>
          </>
        )}

        {/* How it works info */}
        <div className="rounded-lg bg-muted/50 p-4 space-y-2">
          <h4 className="text-sm font-medium flex items-center gap-2">
            <Gift className="w-4 h-4 text-purple-500" />
            How Referrals Work
          </h4>
          <ul className="text-sm text-muted-foreground space-y-1 ml-6 list-disc">
            <li>Share your unique referral link with friends</li>
            <li>They sign up using your link</li>
            <li>When they complete their first course, you both get 100 bonus points!</li>
            <li>No limit on the number of referrals</li>
          </ul>
        </div>
      </CardContent>
    </Card>
  );
}
