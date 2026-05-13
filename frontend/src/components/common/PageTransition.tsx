import * as React from 'react';
import { motion, AnimatePresence } from 'framer-motion';

export interface PageTransitionProps {
  children: React.ReactNode;
  /**
   * Unique key for the page transition (typically the current route path)
   */
  transitionKey: string;
  /**
   * Animation variant to use
   */
  variant?: 'fade' | 'slide' | 'scale' | 'slideUp';
  /**
   * Duration of the animation in seconds
   */
  duration?: number;
  className?: string;
}

const variants = {
  fade: {
    initial: { opacity: 0 },
    animate: { opacity: 1 },
    exit: { opacity: 0 },
  },
  slide: {
    initial: { opacity: 0, x: -20 },
    animate: { opacity: 1, x: 0 },
    exit: { opacity: 0, x: 20 },
  },
  slideUp: {
    initial: { opacity: 0, y: 20 },
    animate: { opacity: 1, y: 0 },
    exit: { opacity: 0, y: -20 },
  },
  scale: {
    initial: { opacity: 0, scale: 0.95 },
    animate: { opacity: 1, scale: 1 },
    exit: { opacity: 0, scale: 0.95 },
  },
};

export const PageTransition: React.FC<PageTransitionProps> = ({
  children,
  transitionKey,
  variant = 'fade',
  duration = 0.3,
  className,
}) => {
  const selectedVariant = variants[variant];

  return (
    <AnimatePresence mode="wait">
      <motion.div
        key={transitionKey}
        initial={selectedVariant.initial}
        animate={selectedVariant.animate}
        exit={selectedVariant.exit}
        transition={{
          duration,
          ease: 'easeInOut',
        }}
        className={className}
      >
        {children}
      </motion.div>
    </AnimatePresence>
  );
};

/**
 * Hook to enable page transitions in router
 * Usage: const location = usePageTransition();
 */
export const usePageTransition = () => {
  const [displayLocation, setDisplayLocation] = React.useState<string>('');

  React.useEffect(() => {
    setDisplayLocation(window.location.pathname);
  }, []);

  return displayLocation;
};
