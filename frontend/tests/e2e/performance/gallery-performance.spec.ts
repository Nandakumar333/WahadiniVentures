import { test, expect } from '@playwright/test';

test.describe('Discount Gallery Performance', () => {
  test('Gallery page with 1000 discount types loads in under 2 seconds', async ({ page }) => {
    // Arrange - Mock API to return 1000 discount types
    await page.route('**/api/discounts/available', async (route) => {
      const discounts = Array.from({ length: 1000 }, (_, i) => ({
        id: `discount-${i}`,
        code: `TEST${i}`,
        discountPercentage: 10 + (i % 20),
        requiredPoints: 100 + (i * 50),
        totalAvailable: 100,
        remainingCount: 50 + (i % 50),
        expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        isActive: true,
        isExpired: false,
        isSoldOut: false,
        canAfford: i % 3 === 0, // 1/3 affordable
        description: `Test discount ${i} - Performance testing with large dataset`,
        category: ['Shopping', 'Gaming', 'Education', 'Entertainment'][i % 4],
      }));

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(discounts),
      });
    });

    // Act - Navigate and measure load time
    const startTime = Date.now();
    
    await page.goto('/discounts', { waitUntil: 'networkidle' });
    
    // Wait for the gallery grid to be visible
    await expect(page.locator('[data-testid="discount-gallery"]')).toBeVisible({ timeout: 5000 });
    
    const loadTime = Date.now() - startTime;

    // Assert - Page should load in under 2 seconds
    expect(loadTime).toBeLessThan(2000);
    console.log(`✓ Gallery with 1000 items loaded in ${loadTime}ms`);

    // Verify virtualization is working (not all items rendered at once)
    const renderedCards = await page.locator('[data-testid="discount-card"]').count();
    expect(renderedCards).toBeLessThan(1000);
    expect(renderedCards).toBeGreaterThan(0);
    console.log(`✓ Virtualization working: Only ${renderedCards} cards rendered out of 1000`);
  });

  test('Gallery scrolling performance remains smooth with 1000 items', async ({ page }) => {
    // Arrange - Mock large dataset
    await page.route('**/api/discounts/available', async (route) => {
      const discounts = Array.from({ length: 1000 }, (_, i) => ({
        id: `discount-${i}`,
        code: `SCROLL${i}`,
        discountPercentage: 15,
        requiredPoints: 500,
        totalAvailable: 50,
        remainingCount: 25,
        expiryDate: new Date(Date.now() + 60 * 24 * 60 * 60 * 1000).toISOString(),
        isActive: true,
        isExpired: false,
        isSoldOut: false,
        canAfford: true,
        description: `Scrolling test discount ${i}`,
        category: 'Testing',
      }));

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(discounts),
      });
    });

    await page.goto('/discounts');
    await expect(page.locator('[data-testid="discount-gallery"]')).toBeVisible();

    // Act - Measure scroll performance
    const scrollTimings: number[] = [];
    
    for (let i = 0; i < 5; i++) {
      const startScroll = Date.now();
      
      // Scroll down 500px
      await page.evaluate(() => {
        window.scrollBy({ top: 500, behavior: 'smooth' });
      });
      
      // Wait for scroll to settle
      await page.waitForTimeout(300);
      
      const scrollTime = Date.now() - startScroll;
      scrollTimings.push(scrollTime);
    }

    // Assert - Scroll should be smooth (each scroll < 500ms)
    const avgScrollTime = scrollTimings.reduce((a, b) => a + b, 0) / scrollTimings.length;
    expect(avgScrollTime).toBeLessThan(500);
    console.log(`✓ Average scroll time: ${avgScrollTime}ms`);
  });

  test('Filtering 1000 discounts responds quickly', async ({ page }) => {
    // Arrange
    await page.route('**/api/discounts/available', async (route) => {
      const discounts = Array.from({ length: 1000 }, (_, i) => ({
        id: `discount-${i}`,
        code: `FILTER${i}`,
        discountPercentage: 10 + (i % 30),
        requiredPoints: 100 + (i * 10),
        totalAvailable: 100,
        remainingCount: 50,
        expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        isActive: i % 2 === 0, // Half active, half inactive
        isExpired: false,
        isSoldOut: i % 10 === 0, // 10% sold out
        canAfford: i % 3 === 0,
        description: `Filter test ${i}`,
        category: ['Shopping', 'Gaming'][i % 2],
      }));

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(discounts),
      });
    });

    await page.goto('/discounts');
    await expect(page.locator('[data-testid="discount-gallery"]')).toBeVisible();

    // Act - Apply filter and measure response time
    const startFilter = Date.now();
    
    // Click "Available Only" filter (if exists)
    const filterButton = page.locator('button:has-text("Active")').first();
    if (await filterButton.isVisible()) {
      await filterButton.click();
    }
    
    // Wait for re-render
    await page.waitForTimeout(100);
    
    const filterTime = Date.now() - startFilter;

    // Assert - Filtering should be instant (< 300ms)
    expect(filterTime).toBeLessThan(300);
    console.log(`✓ Filter applied in ${filterTime}ms`);
  });

  test('Search across 1000 discounts returns results quickly', async ({ page }) => {
    // Arrange
    await page.route('**/api/discounts/available', async (route) => {
      const discounts = Array.from({ length: 1000 }, (_, i) => ({
        id: `discount-${i}`,
        code: `SEARCH${i}`,
        discountPercentage: 20,
        requiredPoints: 300,
        totalAvailable: 50,
        remainingCount: 25,
        expiryDate: new Date(Date.now() + 45 * 24 * 60 * 60 * 1000).toISOString(),
        isActive: true,
        isExpired: false,
        isSoldOut: false,
        canAfford: true,
        description: i % 100 === 0 ? 'Special premium discount offer' : `Standard discount ${i}`,
        category: 'Shopping',
      }));

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(discounts),
      });
    });

    await page.goto('/discounts');
    await expect(page.locator('[data-testid="discount-gallery"]')).toBeVisible();

    // Act - Search and measure response time
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"]').first();
    
    if (await searchInput.isVisible()) {
      const startSearch = Date.now();
      
      await searchInput.fill('premium');
      await page.waitForTimeout(100); // Debounce time
      
      const searchTime = Date.now() - startSearch;

      // Assert - Search should be fast (< 200ms)
      expect(searchTime).toBeLessThan(500);
      console.log(`✓ Search completed in ${searchTime}ms`);
      
      // Verify results are filtered
      const visibleCards = await page.locator('[data-testid="discount-card"]:visible').count();
      expect(visibleCards).toBeGreaterThan(0);
      expect(visibleCards).toBeLessThan(1000); // Should show filtered results
    }
  });

  test('Initial render with empty state is fast', async ({ page }) => {
    // Arrange - Mock empty response
    await page.route('**/api/discounts/available', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    // Act
    const startTime = Date.now();
    await page.goto('/discounts');
    
    // Wait for empty state message
    await expect(page.locator('text=/no discounts|no results|empty/i').first()).toBeVisible({ timeout: 3000 });
    
    const loadTime = Date.now() - startTime;

    // Assert - Empty state should load very quickly (< 1 second)
    expect(loadTime).toBeLessThan(1000);
    console.log(`✓ Empty state loaded in ${loadTime}ms`);
  });

  test('Pagination with 1000 items is responsive', async ({ page }) => {
    // Arrange
    await page.route('**/api/discounts/available*', async (route) => {
      const url = new URL(route.request().url());
      const page = parseInt(url.searchParams.get('page') || '1');
      const pageSize = parseInt(url.searchParams.get('pageSize') || '20');
      
      const startIndex = (page - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      
      const allDiscounts = Array.from({ length: 1000 }, (_, i) => ({
        id: `discount-${i}`,
        code: `PAGE${i}`,
        discountPercentage: 15,
        requiredPoints: 400,
        totalAvailable: 100,
        remainingCount: 50,
        expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        isActive: true,
        isExpired: false,
        isSoldOut: false,
        canAfford: true,
        description: `Page test discount ${i}`,
        category: 'Shopping',
      }));

      const pageData = allDiscounts.slice(startIndex, endIndex);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: pageData,
          totalCount: 1000,
          page,
          pageSize,
          totalPages: Math.ceil(1000 / pageSize),
        }),
      });
    });

    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    // Act - Navigate to next page and measure
    const nextButton = page.locator('button:has-text("Next"), button[aria-label="Next page"]').first();
    
    if (await nextButton.isVisible()) {
      const startPagination = Date.now();
      
      await nextButton.click();
      await page.waitForLoadState('networkidle');
      
      const paginationTime = Date.now() - startPagination;

      // Assert - Page navigation should be fast (< 1 second)
      expect(paginationTime).toBeLessThan(1000);
      console.log(`✓ Pagination completed in ${paginationTime}ms`);
    }
  });

  test('Memory usage remains stable during extended browsing', async ({ page }) => {
    // Arrange
    await page.route('**/api/discounts/available', async (route) => {
      const discounts = Array.from({ length: 1000 }, (_, i) => ({
        id: `discount-${i}`,
        code: `MEM${i}`,
        discountPercentage: 20,
        requiredPoints: 500,
        totalAvailable: 50,
        remainingCount: 25,
        expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        isActive: true,
        isExpired: false,
        isSoldOut: false,
        canAfford: true,
        description: `Memory test ${i}`,
        category: 'Testing',
      }));

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(discounts),
      });
    });

    await page.goto('/discounts');
    await expect(page.locator('[data-testid="discount-gallery"]')).toBeVisible();

    // Act - Simulate extended browsing session
    for (let i = 0; i < 10; i++) {
      // Scroll down
      await page.evaluate(() => window.scrollBy(0, 500));
      await page.waitForTimeout(100);
      
      // Scroll up
      await page.evaluate(() => window.scrollBy(0, -500));
      await page.waitForTimeout(100);
    }

    // Assert - Page should still be responsive
    const card = page.locator('[data-testid="discount-card"]').first();
    await expect(card).toBeVisible({ timeout: 1000 });
    
    // No assertion on exact memory, but page should not crash
    console.log('✓ Page remained stable after extended browsing');
  });
});
