import * as React from 'react';
import { cn } from '@/lib/utils';

export interface OptimizedImageProps extends Omit<React.ImgHTMLAttributes<HTMLImageElement>, 'src'> {
  /**
   * Image source URL
   */
  src: string;
  /**
   * Alt text for accessibility
   */
  alt: string;
  /**
   * Responsive image sources
   */
  srcSet?: string;
  /**
   * Image sizes for responsive loading
   */
  sizes?: string;
  /**
   * Whether to use lazy loading
   */
  lazy?: boolean;
  /**
   * Aspect ratio to maintain (e.g., '16/9', '4/3', '1/1')
   */
  aspectRatio?: string;
  /**
   * Placeholder type while loading
   */
  placeholder?: 'blur' | 'empty' | 'shimmer';
  /**
   * Blur data URL for placeholder
   */
  blurDataURL?: string;
  /**
   * Callback when image loads
   */
  onLoad?: () => void;
  /**
   * Callback when image fails to load
   */
  onError?: () => void;
  /**
   * Priority loading (disables lazy loading)
   */
  priority?: boolean;
  /**
   * Object fit behavior
   */
  objectFit?: 'contain' | 'cover' | 'fill' | 'none' | 'scale-down';
}

export const OptimizedImage: React.FC<OptimizedImageProps> = ({
  src,
  alt,
  srcSet,
  sizes,
  lazy = true,
  aspectRatio,
  placeholder = 'shimmer',
  blurDataURL,
  onLoad: onLoadProp,
  onError: onErrorProp,
  priority = false,
  objectFit = 'cover',
  className,
  ...props
}) => {
  const [isLoaded, setIsLoaded] = React.useState(false);
  const [hasError, setHasError] = React.useState(false);
  const imgRef = React.useRef<HTMLImageElement>(null);

  const handleLoad = React.useCallback(() => {
    setIsLoaded(true);
    onLoadProp?.();
  }, [onLoadProp]);

  const handleError = React.useCallback(() => {
    setHasError(true);
    onErrorProp?.();
  }, [onErrorProp]);

  // Intersection Observer for lazy loading
  React.useEffect(() => {
    if (!lazy || priority || !imgRef.current) return;

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting && imgRef.current) {
            const img = imgRef.current;
            if (img.dataset.src) {
              img.src = img.dataset.src;
              img.removeAttribute('data-src');
            }
            if (img.dataset.srcset) {
              img.srcset = img.dataset.srcset;
              img.removeAttribute('data-srcset');
            }
            observer.unobserve(img);
          }
        });
      },
      {
        rootMargin: '50px',
      }
    );

    observer.observe(imgRef.current);

    return () => {
      if (imgRef.current) {
        observer.unobserve(imgRef.current);
      }
    };
  }, [lazy, priority]);

  const objectFitClass = {
    contain: 'object-contain',
    cover: 'object-cover',
    fill: 'object-fill',
    none: 'object-none',
    'scale-down': 'object-scale-down',
  }[objectFit];

  return (
    <div
      className={cn(
        'relative overflow-hidden bg-muted',
        aspectRatio && 'aspect-[var(--aspect-ratio)]',
        className
      )}
      style={
        aspectRatio
          ? ({ '--aspect-ratio': aspectRatio } as React.CSSProperties)
          : undefined
      }
    >
      {/* Placeholder */}
      {!isLoaded && !hasError && (
        <>
          {placeholder === 'blur' && blurDataURL && (
            <img
              src={blurDataURL}
              alt=""
              aria-hidden="true"
              className={cn('absolute inset-0 h-full w-full', objectFitClass)}
            />
          )}
          {placeholder === 'shimmer' && (
            <div className="absolute inset-0 animate-pulse bg-gradient-to-r from-muted via-muted/50 to-muted" />
          )}
        </>
      )}

      {/* Main Image */}
      {!hasError && (
        <img
          ref={imgRef}
          src={priority || !lazy ? src : undefined}
          data-src={lazy && !priority ? src : undefined}
          srcSet={priority || !lazy ? srcSet : undefined}
          data-srcset={lazy && !priority ? srcSet : undefined}
          sizes={sizes}
          alt={alt}
          loading={lazy && !priority ? 'lazy' : undefined}
          onLoad={handleLoad}
          onError={handleError}
          className={cn(
            'h-full w-full transition-opacity duration-300',
            objectFitClass,
            isLoaded ? 'opacity-100' : 'opacity-0'
          )}
          {...props}
        />
      )}

      {/* Error Fallback */}
      {hasError && (
        <div className="absolute inset-0 flex items-center justify-center bg-muted">
          <div className="text-center text-muted-foreground">
            <svg
              className="mx-auto h-12 w-12 mb-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
            <p className="text-sm">Failed to load image</p>
          </div>
        </div>
      )}
    </div>
  );
};

/**
 * Avatar-specific optimized image
 */
export const OptimizedAvatar: React.FC<Omit<OptimizedImageProps, 'aspectRatio' | 'objectFit'>> = (
  props
) => {
  return (
    <OptimizedImage
      {...props}
      aspectRatio="1/1"
      objectFit="cover"
      className={cn('rounded-full', props.className)}
    />
  );
};
