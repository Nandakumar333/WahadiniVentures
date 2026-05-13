import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Lock, CheckCircle2, Star, TrendingUp, Award } from 'lucide-react';
import { cn } from '@/lib/utils';

interface PremiumGateProps {
  children: React.ReactNode;
  showPreview?: boolean;
  className?: string;
}

/**
 * PremiumGate Component
 * 
 * Gates premium content for free users with a compelling upgrade UI.
 * Renders children for premium/admin users without modification.
 * Shows upgrade prompt with blurred preview for free users.
 * 
 * Features:
 * - Blur effect with glassmorphism overlay
 * - Upgrade card with pricing and benefits
 * - Lock icon animation
 * - Social proof (member count)
 * - Clear call-to-action button
 * - Accessibility compliant (WCAG 2.1 AA)
 */
export const PremiumGate: React.FC<PremiumGateProps> = ({
  children,
  showPreview = true,
  className,
}) => {
  const navigate = useNavigate();
  const { isPremium, isAdmin } = useAuthStore();

  // Allow access for premium and admin users
  if (isPremium() || isAdmin()) {
    return <>{children}</>;
  }

  // Show upgrade prompt for free users
  return (
    <div className={cn('relative', className)}>
      {/* Blurred Content Preview */}
      {showPreview && (
        <div className="relative pointer-events-none">
          <div className="blur-md opacity-50">
            {children}
          </div>
          <div className="absolute inset-0 bg-gradient-to-b from-transparent via-background/50 to-background" />
        </div>
      )}

      {/* Premium Upgrade Card */}
      <div className={cn(
        'absolute inset-0 flex items-center justify-center p-4',
        showPreview && 'mt-8'
      )}>
        <Card className="max-w-md w-full shadow-2xl border-2 border-primary/20 bg-background/95 backdrop-blur">
          <CardHeader className="text-center space-y-4">
            {/* Lock Icon with Animation */}
            <div className="mx-auto w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
              <Lock className="h-8 w-8 text-primary animate-pulse" aria-hidden="true" />
            </div>

            <div>
              <Badge variant="secondary" className="mb-2">
                <Star className="h-3 w-3 mr-1" aria-hidden="true" />
                Premium Feature
              </Badge>
              <CardTitle className="text-2xl font-bold">
                Unlock Premium Content
              </CardTitle>
              <CardDescription className="text-base mt-2">
                Upgrade to Premium and get unlimited access to all courses, tasks, and rewards
              </CardDescription>
            </div>
          </CardHeader>

          <CardContent className="space-y-4">
            {/* Benefits List */}
            <div className="space-y-3">
              <BenefitItem icon={CheckCircle2} text="Access to all premium courses" />
              <BenefitItem icon={TrendingUp} text="Exclusive task opportunities" />
              <BenefitItem icon={Award} text="Higher reward multipliers" />
              <BenefitItem icon={Star} text="Priority support" />
            </div>

            {/* Social Proof */}
            <div className="bg-muted/50 rounded-lg p-4 text-center">
              <p className="text-sm text-muted-foreground">
                Join <span className="font-bold text-foreground">10,000+</span> premium members
              </p>
              <div className="flex justify-center mt-2 -space-x-2">
                {[1, 2, 3, 4].map((i) => (
                  <div
                    key={i}
                    className="w-8 h-8 rounded-full bg-gradient-to-br from-primary to-primary/60 border-2 border-background"
                    aria-hidden="true"
                  />
                ))}
              </div>
            </div>
          </CardContent>

          <CardFooter className="flex-col space-y-2">
            <Button
              className="w-full"
              size="lg"
              onClick={() => navigate('/pricing')}
              aria-label="Upgrade to Premium membership"
            >
              Upgrade to Premium
            </Button>
            <p className="text-xs text-muted-foreground text-center">
              Cancel anytime • 30-day money-back guarantee
            </p>
          </CardFooter>
        </Card>
      </div>
    </div>
  );
};

// Benefit Item Component
interface BenefitItemProps {
  icon: React.ComponentType<{ className?: string }>;
  text: string;
}

const BenefitItem: React.FC<BenefitItemProps> = ({ icon: Icon, text }) => (
  <div className="flex items-center gap-3">
    <div className="flex-shrink-0">
      <Icon className="h-5 w-5 text-primary" aria-hidden="true" />
    </div>
    <p className="text-sm text-foreground">{text}</p>
  </div>
);

export default PremiumGate;
