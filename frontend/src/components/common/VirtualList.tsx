import * as React from 'react';
import { useVirtualizer } from '@tanstack/react-virtual';
import { cn } from '@/lib/utils';

export interface VirtualListProps<T> {
  /**
   * Array of items to render
   */
  items: T[];
  /**
   * Render function for each item
   */
  renderItem: (item: T, index: number) => React.ReactNode;
  /**
   * Estimated item height in pixels
   */
  estimateSize?: number;
  /**
   * Gap between items in pixels
   */
  gap?: number;
  /**
   * Container height (if not provided, will use parent height)
   */
  height?: number | string;
  /**
   * Container className
   */
  className?: string;
  /**
   * Item wrapper className
   */
  itemClassName?: string;
  /**
   * Callback when scrolling
   */
  onScroll?: (scrollTop: number) => void;
  /**
   * Overscan count (number of items to render outside viewport)
   */
  overscan?: number;
  /**
   * Loading indicator
   */
  loading?: boolean;
  /**
   * Empty state component
   */
  emptyState?: React.ReactNode;
}

export function VirtualList<T>({
  items,
  renderItem,
  estimateSize = 50,
  gap = 0,
  height = '100%',
  className,
  itemClassName,
  onScroll,
  overscan = 5,
  loading = false,
  emptyState,
}: VirtualListProps<T>) {
  const parentRef = React.useRef<HTMLDivElement>(null);

  const virtualizer = useVirtualizer({
    count: items.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => estimateSize,
    overscan,
    gap,
  });

  // Handle scroll callback
  React.useEffect(() => {
    if (!onScroll || !parentRef.current) return;

    const handleScroll = () => {
      if (parentRef.current) {
        onScroll(parentRef.current.scrollTop);
      }
    };

    const element = parentRef.current;
    element.addEventListener('scroll', handleScroll);
    return () => element.removeEventListener('scroll', handleScroll);
  }, [onScroll]);

  if (loading) {
    return (
      <div
        className={cn('flex items-center justify-center', className)}
        style={{ height }}
      >
        <div className="text-center text-muted-foreground">
          <div className="h-8 w-8 animate-spin rounded-full border-2 border-primary border-t-transparent mx-auto mb-2" />
          <p>Loading...</p>
        </div>
      </div>
    );
  }

  if (items.length === 0 && emptyState) {
    return (
      <div
        className={cn('flex items-center justify-center', className)}
        style={{ height }}
      >
        {emptyState}
      </div>
    );
  }

  return (
    <div
      ref={parentRef}
      className={cn('overflow-auto', className)}
      style={{ height }}
    >
      <div
        style={{
          height: `${virtualizer.getTotalSize()}px`,
          width: '100%',
          position: 'relative',
        }}
      >
        {virtualizer.getVirtualItems().map((virtualItem) => (
          <div
            key={virtualItem.key}
            className={itemClassName}
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              width: '100%',
              transform: `translateY(${virtualItem.start}px)`,
            }}
          >
            {renderItem(items[virtualItem.index], virtualItem.index)}
          </div>
        ))}
      </div>
    </div>
  );
}

/**
 * Virtual grid for 2D layouts
 */
export interface VirtualGridProps<T> {
  items: T[];
  renderItem: (item: T, index: number) => React.ReactNode;
  columns: number;
  estimateSize?: number;
  gap?: number;
  height?: number | string;
  className?: string;
  itemClassName?: string;
}

export function VirtualGrid<T>({
  items,
  renderItem,
  columns,
  estimateSize = 200,
  gap = 16,
  height = '100%',
  className,
  itemClassName,
}: VirtualGridProps<T>) {
  const parentRef = React.useRef<HTMLDivElement>(null);

  // Calculate rows
  const rows = Math.ceil(items.length / columns);

  const virtualizer = useVirtualizer({
    count: rows,
    getScrollElement: () => parentRef.current,
    estimateSize: () => estimateSize,
    overscan: 2,
  });

  if (items.length === 0) {
    return (
      <div
        className={cn('flex items-center justify-center text-muted-foreground', className)}
        style={{ height }}
      >
        <p>No items to display</p>
      </div>
    );
  }

  return (
    <div
      ref={parentRef}
      className={cn('overflow-auto', className)}
      style={{ height }}
    >
      <div
        style={{
          height: `${virtualizer.getTotalSize()}px`,
          width: '100%',
          position: 'relative',
        }}
      >
        {virtualizer.getVirtualItems().map((virtualRow) => {
          const rowStartIndex = virtualRow.index * columns;
          const rowItems = items.slice(rowStartIndex, rowStartIndex + columns);

          return (
            <div
              key={virtualRow.key}
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                transform: `translateY(${virtualRow.start}px)`,
              }}
            >
              <div
                className="grid"
                style={{
                  gridTemplateColumns: `repeat(${columns}, 1fr)`,
                  gap: `${gap}px`,
                }}
              >
                {rowItems.map((item, colIndex) => (
                  <div key={rowStartIndex + colIndex} className={itemClassName}>
                    {renderItem(item, rowStartIndex + colIndex)}
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
