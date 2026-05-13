import * as React from 'react';
import { cn } from '@/lib/utils';

export type ContainerSize = 'narrow' | 'default' | 'wide' | 'full';

export interface ResponsiveContainerProps {
  children: React.ReactNode;
  size?: ContainerSize;
  className?: string;
  /**
   * Whether to apply padding
   */
  noPadding?: boolean;
  /**
   * Custom element type
   */
  as?: React.ElementType;
}

const containerSizeClasses: Record<ContainerSize, string> = {
  narrow: 'max-w-container-narrow',
  default: 'max-w-container-default',
  wide: 'max-w-container-wide',
  full: 'max-w-full',
};

export const ResponsiveContainer: React.FC<ResponsiveContainerProps> = ({
  children,
  size = 'default',
  className,
  noPadding = false,
  as: Component = 'div',
}) => {
  return (
    <Component
      className={cn(
        'mx-auto w-full',
        containerSizeClasses[size],
        !noPadding && 'px-4 sm:px-6 lg:px-8',
        className
      )}
    >
      {children}
    </Component>
  );
};
