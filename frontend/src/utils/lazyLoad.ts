import { lazy, type ComponentType, type LazyExoticComponent } from 'react';

/**
 * Options for lazy loading components
 */
export interface LazyLoadOptions {
  /**
   * Delay in milliseconds before loading the component (for testing)
   */
  delay?: number;
  /**
   * Preload the component after specified delay (in ms)
   */
  preloadAfter?: number;
}

/**
 * Enhanced lazy loading with optional delay and preloading
 */
export function lazyLoad<T extends ComponentType<any>>(
  importFunc: () => Promise<{ default: T }>,
  options: LazyLoadOptions = {}
): LazyExoticComponent<T> {
  const { delay = 0, preloadAfter } = options;

  // Create the lazy component
  const LazyComponent = lazy(() => {
    const promise = importFunc();

    // Add artificial delay if specified (useful for testing loading states)
    if (delay > 0) {
      return new Promise<{ default: T }>((resolve) => {
        setTimeout(() => {
          promise.then(resolve);
        }, delay);
      });
    }

    return promise;
  });

  // Set up preloading if specified
  if (preloadAfter !== undefined) {
    setTimeout(() => {
      importFunc();
    }, preloadAfter);
  }

  return LazyComponent;
}

/**
 * Preload a lazy component
 */
export function preloadComponent<T extends ComponentType<any>>(
  importFunc: () => Promise<{ default: T }>
): void {
  importFunc();
}

/**
 * Create a map of lazy-loaded routes
 */
export function createLazyRoutes<T extends Record<string, () => Promise<any>>>(
  routes: T
): { [K in keyof T]: LazyExoticComponent<ComponentType<any>> } {
  const lazyRoutes: any = {};

  for (const [key, importFunc] of Object.entries(routes)) {
    lazyRoutes[key] = lazy(importFunc);
  }

  return lazyRoutes;
}

/**
 * Preload critical routes on idle
 */
export function preloadCriticalRoutes(
  importFuncs: Array<() => Promise<any>>
): void {
  if ('requestIdleCallback' in window) {
    requestIdleCallback(() => {
      importFuncs.forEach((importFunc) => importFunc());
    });
  } else {
    // Fallback for browsers that don't support requestIdleCallback
    setTimeout(() => {
      importFuncs.forEach((importFunc) => importFunc());
    }, 1000);
  }
}

/**
 * Retry failed lazy imports (useful for chunk loading failures)
 */
export function lazyWithRetry<T extends ComponentType<any>>(
  importFunc: () => Promise<{ default: T }>,
  retries: number = 3,
  interval: number = 1000
): LazyExoticComponent<T> {
  return lazy(() => {
    return new Promise<{ default: T }>((resolve, reject) => {
      const attemptImport = (retriesLeft: number) => {
        importFunc()
          .then(resolve)
          .catch((error) => {
            if (retriesLeft === 0) {
              reject(error);
              return;
            }

            console.warn(
              `Failed to load component, retrying... (${retriesLeft} attempts left)`
            );

            setTimeout(() => {
              attemptImport(retriesLeft - 1);
            }, interval);
          });
      };

      attemptImport(retries);
    });
  });
}
