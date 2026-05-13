import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useMyRedemptions } from '../../hooks/discount/useMyRedemptions';
import { RedemptionCodeCard } from '../../components/discount/RedemptionCodeCard/RedemptionCodeCard';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Skeleton } from '@/components/ui/skeleton';
import { ChevronLeft, ChevronRight, Gift } from 'lucide-react';

/**
 * Page component displaying user's redeemed discount codes with pagination
 */
export const MyDiscountsPage = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 9; // 3x3 grid

  const { data, isLoading, isError, error } = useMyRedemptions({
    pageNumber: currentPage,
    pageSize,
  });

  const handlePreviousPage = () => {
    setCurrentPage((prev) => Math.max(1, prev - 1));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleNextPage = () => {
    if (data && currentPage < data.totalPages) {
      setCurrentPage((prev) => prev + 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  const handlePageClick = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Generate page numbers for pagination
  const getPageNumbers = () => {
    if (!data) return [];
    
    const pages: (number | string)[] = [];
    const totalPages = data.totalPages;
    const current = currentPage;

    if (totalPages <= 7) {
      // Show all pages if 7 or fewer
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Always show first page
      pages.push(1);

      if (current > 3) {
        pages.push('...');
      }

      // Show pages around current page
      const start = Math.max(2, current - 1);
      const end = Math.min(totalPages - 1, current + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (current < totalPages - 2) {
        pages.push('...');
      }

      // Always show last page
      pages.push(totalPages);
    }

    return pages;
  };

  return (
    <div className="container mx-auto px-4 py-8 max-w-7xl">
      <CardHeader className="px-0">
        <CardTitle className="text-3xl font-bold">My Discount Codes</CardTitle>
        <CardDescription className="text-base">
          View and manage your redeemed discount codes
        </CardDescription>
      </CardHeader>

      <CardContent className="px-0">
        {isError && (
          <Alert variant="destructive" className="mb-6">
            <AlertDescription>
              {error?.message || 'Failed to load your discount codes. Please try again later.'}
            </AlertDescription>
          </Alert>
        )}

        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[...Array(6)].map((_, index) => (
              <Card key={index} className="flex flex-col h-full">
                <CardHeader>
                  <Skeleton className="h-6 w-24 mb-2" />
                  <Skeleton className="h-4 w-32" />
                </CardHeader>
                <CardContent className="flex-grow space-y-3">
                  <Skeleton className="h-20 w-full" />
                  <Skeleton className="h-4 w-full" />
                  <Skeleton className="h-8 w-full" />
                </CardContent>
              </Card>
            ))}
          </div>
        ) : data && data.items.length > 0 ? (
          <>
            {/* Discount Codes Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
              {data.items.map((redemption) => (
                <RedemptionCodeCard key={redemption.id} redemption={redemption} />
              ))}
            </div>

            {/* Pagination */}
            {data.totalPages > 1 && (
              <div className="flex flex-col sm:flex-row items-center justify-between gap-4 mt-8">
                <div className="text-sm text-muted-foreground">
                  Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, data.totalCount)} of {data.totalCount} redemptions
                </div>

                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="icon"
                    onClick={handlePreviousPage}
                    disabled={currentPage === 1}
                  >
                    <ChevronLeft className="h-4 w-4" />
                  </Button>

                  <div className="flex items-center gap-1">
                    {getPageNumbers().map((page, index) => (
                      typeof page === 'number' ? (
                        <Button
                          key={index}
                          variant={currentPage === page ? 'default' : 'outline'}
                          size="icon"
                          onClick={() => handlePageClick(page)}
                          className="w-10"
                        >
                          {page}
                        </Button>
                      ) : (
                        <span key={index} className="px-2 text-muted-foreground">
                          {page}
                        </span>
                      )
                    ))}
                  </div>

                  <Button
                    variant="outline"
                    size="icon"
                    onClick={handleNextPage}
                    disabled={currentPage === data.totalPages}
                  >
                    <ChevronRight className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            )}
          </>
        ) : (
          // Empty State
          <div className="flex flex-col items-center justify-center py-16 text-center">
            <div className="rounded-full bg-muted p-6 mb-6">
              <Gift className="h-16 w-16 text-muted-foreground" />
            </div>
            <h3 className="text-2xl font-semibold mb-2">No Discount Codes Yet</h3>
            <p className="text-muted-foreground mb-6 max-w-md">
              You haven't redeemed any discount codes yet. Browse available discounts and start saving on your subscriptions!
            </p>
            <Link to="/discounts">
              <Button size="lg">
                Browse Available Discounts
              </Button>
            </Link>
          </div>
        )}
      </CardContent>
    </div>
  );
};
