import * as React from 'react';
import { motion } from 'framer-motion';
import { Button, type ButtonProps } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export interface AnimatedButtonProps extends Omit<ButtonProps, 'asChild'> {
  /**
   * Animation preset to use
   */
  animation?: 'scale' | 'bounce' | 'pulse' | 'shake' | 'lift' | 'none';
  /**
   * Whether to show ripple effect on click
   */
  ripple?: boolean;
  /**
   * Loading state
   */
  isLoading?: boolean;
  /**
   * Icon to show on the left
   */
  leftIcon?: React.ReactNode;
  /**
   * Icon to show on the right
   */
  rightIcon?: React.ReactNode;
}

const animationVariants = {
  scale: {
    whileHover: { scale: 1.05 },
    whileTap: { scale: 0.95 },
    transition: { type: 'spring' as const, stiffness: 400, damping: 17 },
  },
  bounce: {
    whileHover: { y: -2 },
    whileTap: { y: 0 },
    transition: { type: 'spring' as const, stiffness: 400, damping: 10 },
  },
  pulse: {
    whileHover: { scale: [1, 1.05, 1] },
    transition: { duration: 0.3, repeat: Infinity, repeatType: 'reverse' as const },
  },
  shake: {
    whileHover: { x: [-2, 2, -2, 2, 0] },
    transition: { duration: 0.4 },
  },
  lift: {
    whileHover: { y: -4, boxShadow: '0 10px 20px rgba(0,0,0,0.2)' },
    whileTap: { y: 0, boxShadow: '0 5px 10px rgba(0,0,0,0.1)' },
    transition: { type: 'spring' as const, stiffness: 400, damping: 17 },
  },
  none: {},
};

export const AnimatedButton = React.forwardRef<HTMLButtonElement, AnimatedButtonProps>(
  (
    {
      className,
      animation = 'scale',
      ripple = false,
      isLoading = false,
      leftIcon,
      rightIcon,
      children,
      onClick,
      disabled,
      ...props
    },
    ref
  ) => {
    const [ripples, setRipples] = React.useState<{ x: number; y: number; id: number }[]>([]);
    const buttonRef = React.useRef<HTMLButtonElement>(null);

    React.useImperativeHandle(ref, () => buttonRef.current!);

    const handleClick = (e: React.MouseEvent<HTMLButtonElement>) => {
      if (ripple && buttonRef.current) {
        const rect = buttonRef.current.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        const id = Date.now();

        setRipples((prev) => [...prev, { x, y, id }]);

        // Remove ripple after animation
        setTimeout(() => {
          setRipples((prev) => prev.filter((r) => r.id !== id));
        }, 600);
      }

      onClick?.(e);
    };

    const animationProps = animationVariants[animation];

    return (
      <motion.div
        {...(animation !== 'none' && animationProps)}
        className="inline-block"
      >
        <Button
          ref={buttonRef}
          className={cn('relative overflow-hidden', className)}
          onClick={handleClick}
          disabled={disabled || isLoading}
          {...props}
        >
          {/* Ripple Effect */}
          {ripple && (
            <span className="absolute inset-0 pointer-events-none">
              {ripples.map((ripple) => (
                <motion.span
                  key={ripple.id}
                  className="absolute rounded-full bg-white/30"
                  style={{
                    left: ripple.x,
                    top: ripple.y,
                  }}
                  initial={{ width: 0, height: 0, x: 0, y: 0 }}
                  animate={{
                    width: 200,
                    height: 200,
                    x: -100,
                    y: -100,
                    opacity: [1, 0],
                  }}
                  transition={{ duration: 0.6 }}
                />
              ))}
            </span>
          )}

          {/* Loading Spinner */}
          {isLoading && (
            <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
          )}

          {/* Left Icon */}
          {leftIcon && !isLoading && (
            <span className="mr-2 inline-flex items-center">{leftIcon}</span>
          )}

          {/* Button Content */}
          <span className="relative z-10">{children}</span>

          {/* Right Icon */}
          {rightIcon && (
            <span className="ml-2 inline-flex items-center">{rightIcon}</span>
          )}
        </Button>
      </motion.div>
    );
  }
);

AnimatedButton.displayName = 'AnimatedButton';
