import React from 'react';
import { Loader2 } from 'lucide-react';

export const PageLoader = () => (
  <div className="flex items-center justify-center min-h-screen">
    <div className="text-center">
      <Loader2 className="h-8 w-8 animate-spin mx-auto text-blue-600" />
      <p className="text-gray-600 mt-2">Loading...</p>
    </div>
  </div>
);

export const LazyWrapper = ({ children }: { children: React.ReactNode }) => (
  <React.Suspense fallback={<PageLoader />}>
    {children}
  </React.Suspense>
);