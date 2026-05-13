import * as React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { CheckCircle, Sparkles, Award, Star } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface SuccessAnimationProps {
  show: boolean;
  title?: string;
  message?: string;
  icon?: 'check' | 'sparkles' | 'award' | 'star';
  duration?: number;
  onComplete?: () => void;
  variant?: 'default' | 'celebration' | 'subtle';
  className?: string;
}

const iconComponents = {
  check: CheckCircle,
  sparkles: Sparkles,
  award: Award,
  star: Star,
};

const confettiColors = [
  'bg-red-500',
  'bg-blue-500',
  'bg-green-500',
  'bg-yellow-500',
  'bg-purple-500',
  'bg-pink-500',
];

interface ConfettiPiece {
  id: number;
  x: number;
  y: number;
  rotation: number;
  scale: number;
  color: string;
}

export const SuccessAnimation: React.FC<SuccessAnimationProps> = ({
  show,
  title = 'Success!',
  message,
  icon = 'check',
  duration = 3000,
  onComplete,
  variant = 'default',
  className,
}) => {
  const [confetti, setConfetti] = React.useState<ConfettiPiece[]>([]);
  const Icon = iconComponents[icon];

  React.useEffect(() => {
    if (show && variant === 'celebration') {
      // Generate confetti pieces
      const pieces: ConfettiPiece[] = Array.from({ length: 20 }, (_, i) => ({
        id: i,
        x: Math.random() * 100,
        y: -10,
        rotation: Math.random() * 360,
        scale: Math.random() * 0.5 + 0.5,
        color: confettiColors[Math.floor(Math.random() * confettiColors.length)],
      }));
      setConfetti(pieces);
    }

    if (show && onComplete) {
      const timer = setTimeout(onComplete, duration);
      return () => clearTimeout(timer);
    }
  }, [show, variant, onComplete, duration]);

  return (
    <AnimatePresence>
      {show && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.3 }}
          className={cn(
            'fixed inset-0 z-50 flex items-center justify-center',
            variant !== 'subtle' && 'bg-black/50 backdrop-blur-sm',
            className
          )}
        >
          {/* Confetti Animation */}
          {variant === 'celebration' && (
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
              {confetti.map((piece) => (
                <motion.div
                  key={piece.id}
                  initial={{
                    x: `${piece.x}%`,
                    y: piece.y,
                    rotate: piece.rotation,
                    scale: piece.scale,
                  }}
                  animate={{
                    y: '120vh',
                    rotate: piece.rotation + 360,
                  }}
                  transition={{
                    duration: 2 + Math.random(),
                    ease: 'easeIn',
                  }}
                  className={cn(
                    'absolute h-3 w-3 rounded-sm',
                    piece.color
                  )}
                />
              ))}
            </div>
          )}

          {/* Success Card */}
          <motion.div
            initial={{ scale: 0, rotate: -10 }}
            animate={{ scale: 1, rotate: 0 }}
            exit={{ scale: 0, rotate: 10 }}
            transition={{
              type: 'spring',
              stiffness: 260,
              damping: 20,
            }}
            className="relative z-10 flex flex-col items-center gap-4 rounded-lg bg-background p-8 shadow-2xl"
          >
            {/* Icon with pulse animation */}
            <motion.div
              initial={{ scale: 0 }}
              animate={{ scale: 1 }}
              transition={{
                delay: 0.2,
                type: 'spring',
                stiffness: 200,
                damping: 10,
              }}
            >
              <motion.div
                animate={{
                  scale: [1, 1.2, 1],
                }}
                transition={{
                  duration: 0.6,
                  repeat: variant === 'celebration' ? 2 : 0,
                  repeatDelay: 0.3,
                }}
                className={cn(
                  'flex h-16 w-16 items-center justify-center rounded-full',
                  'bg-green-100 dark:bg-green-900/30'
                )}
              >
                <Icon className="h-8 w-8 text-green-600 dark:text-green-500" />
              </motion.div>
            </motion.div>

            {/* Text content */}
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
              className="text-center"
            >
              <h2 className="text-2xl font-bold text-foreground">{title}</h2>
              {message && (
                <p className="mt-2 text-muted-foreground">{message}</p>
              )}
            </motion.div>

            {/* Decorative elements for celebration variant */}
            {variant === 'celebration' && (
              <>
                <motion.div
                  animate={{
                    rotate: [0, 360],
                  }}
                  transition={{
                    duration: 3,
                    repeat: Infinity,
                    ease: 'linear',
                  }}
                  className="absolute -top-6 -left-6"
                >
                  <Sparkles className="h-8 w-8 text-yellow-500" />
                </motion.div>
                <motion.div
                  animate={{
                    rotate: [360, 0],
                  }}
                  transition={{
                    duration: 3,
                    repeat: Infinity,
                    ease: 'linear',
                  }}
                  className="absolute -bottom-6 -right-6"
                >
                  <Star className="h-8 w-8 text-purple-500" />
                </motion.div>
              </>
            )}
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};
