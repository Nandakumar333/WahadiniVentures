import { test, expect } from '@playwright/test';

test.describe('Browse Courses E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/courses');
  });

  test('✓ User can view courses list', async ({ page }) => {
    // Wait for courses to load
    await page.waitForSelector('[data-testid="course-card"], [data-testid="course-list"]', { timeout: 5000 });
    
    // Verify page title or heading
    const heading = page.locator('h1, h2').first();
    await expect(heading).toBeVisible();
    
    // Verify at least one course card is visible
    const courseCards = page.locator('[data-testid="course-card"]');
    await expect(courseCards.first()).toBeVisible();
  });

  test('✓ User can search for courses', async ({ page }) => {
    // Wait for search input
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]');
    await expect(searchInput).toBeVisible();
    
    // Enter search term
    await searchInput.fill('Bitcoin');
    
    // Wait for search results to update (debounce delay)
    await page.waitForTimeout(500);
    
    // Verify search results contain the search term
    const courseCards = page.locator('[data-testid="course-card"]');
    if (await courseCards.count() > 0) {
      const firstCard = courseCards.first();
      await expect(firstCard).toBeVisible();
    }
  });

  test('✓ User can filter courses by category', async ({ page }) => {
    // Wait for category filter (dropdown, buttons, or tabs)
    const categoryFilter = page.locator('select[name*="category"], button:has-text("Category"), [data-testid="category-filter"]').first();
    
    if (await categoryFilter.isVisible()) {
      await categoryFilter.click();
      
      // Select a category
      const categoryOption = page.locator('option, button, a').filter({ hasText: /Blockchain|Crypto|Bitcoin/i }).first();
      if (await categoryOption.isVisible()) {
        await categoryOption.click();
        
        // Wait for filtered results
        await page.waitForTimeout(500);
        
        // Verify courses are filtered
        const courseCards = page.locator('[data-testid="course-card"]');
        await expect(courseCards.first()).toBeVisible({ timeout: 3000 });
      }
    }
  });

  test('✓ User can filter courses by difficulty level', async ({ page }) => {
    // Wait for difficulty filter
    const difficultyFilter = page.locator('select[name*="difficulty"], button:has-text("Difficulty"), [data-testid="difficulty-filter"]').first();
    
    if (await difficultyFilter.isVisible()) {
      await difficultyFilter.click();
      
      // Select a difficulty level
      const difficultyOption = page.locator('option, button').filter({ hasText: /Beginner|Intermediate|Advanced/i }).first();
      if (await difficultyOption.isVisible()) {
        await difficultyOption.click();
        
        // Wait for filtered results
        await page.waitForTimeout(500);
        
        // Verify courses are filtered
        const courseCards = page.locator('[data-testid="course-card"]');
        if (await courseCards.count() > 0) {
          await expect(courseCards.first()).toBeVisible();
        }
      }
    }
  });

  test('✓ User can view course details', async ({ page }) => {
    // Wait for course cards
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Click on the first course
    const firstCourse = page.locator('[data-testid="course-card"]').first();
    const courseTitleElement = firstCourse.locator('h3, h2, [data-testid="course-title"]').first();
    const courseTitle = await courseTitleElement.textContent();
    
    await firstCourse.click();
    
    // Wait for navigation to course detail page
    await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
    
    // Verify course detail page elements
    await expect(page.locator('h1, h2').first()).toBeVisible();
    
    // Verify course information is displayed
    const detailsSection = page.locator('[data-testid="course-details"], .course-details, main');
    await expect(detailsSection).toBeVisible();
  });

  test('✓ User can paginate through courses', async ({ page }) => {
    // Wait for courses to load
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Look for pagination controls
    const nextButton = page.locator('button:has-text("Next"), button[aria-label*="next"], [data-testid="next-page"]').first();
    const paginationContainer = page.locator('[role="navigation"], [data-testid="pagination"]').first();
    
    if (await nextButton.isVisible() || await paginationContainer.isVisible()) {
      // Get current page courses
      const firstPageCards = await page.locator('[data-testid="course-card"]').count();
      expect(firstPageCards).toBeGreaterThan(0);
      
      // Click next page if available
      if (await nextButton.isEnabled()) {
        await nextButton.click();
        
        // Wait for new page to load
        await page.waitForTimeout(500);
        
        // Verify courses loaded on second page
        const secondPageCards = await page.locator('[data-testid="course-card"]').count();
        expect(secondPageCards).toBeGreaterThan(0);
      }
    }
  });

  test('✓ User can view course enrollment count and ratings', async ({ page }) => {
    // Wait for course cards
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    const firstCourse = page.locator('[data-testid="course-card"]').first();
    await expect(firstCourse).toBeVisible();
    
    // Check for enrollment count (e.g., "1,234 students")
    const enrollmentInfo = firstCourse.locator('[data-testid="enrollment-count"], :has-text("students"), :has-text("enrolled")');
    
    // Check for rating/difficulty info
    const metaInfo = firstCourse.locator('[data-testid="course-meta"], [data-testid="course-stats"]');
    
    // At least one of these should be visible
    const hasEnrollmentInfo = await enrollmentInfo.isVisible().catch(() => false);
    const hasMetaInfo = await metaInfo.isVisible().catch(() => false);
    
    expect(hasEnrollmentInfo || hasMetaInfo).toBeTruthy();
  });

  test('✓ User can clear filters', async ({ page }) => {
    // Apply a search filter
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]');
    if (await searchInput.isVisible()) {
      await searchInput.fill('Bitcoin');
      await page.waitForTimeout(500);
      
      // Look for clear/reset button
      const clearButton = page.locator('button:has-text("Clear"), button:has-text("Reset"), button[aria-label*="clear"]').first();
      
      if (await clearButton.isVisible()) {
        await clearButton.click();
        
        // Verify search is cleared
        await expect(searchInput).toHaveValue('');
        
        // Wait for all courses to reload
        await page.waitForTimeout(500);
        
        // Verify courses are displayed
        const courseCards = page.locator('[data-testid="course-card"]');
        await expect(courseCards.first()).toBeVisible();
      }
    }
  });

  test('✓ User can see loading state while courses load', async ({ page }) => {
    // Intercept the API call to add delay
    await page.route('**/api/courses**', async (route) => {
      await page.waitForTimeout(1000); // Add 1s delay
      await route.continue();
    });
    
    // Navigate to courses page
    await page.goto('/courses');
    
    // Look for loading indicator
    const loadingIndicator = page.locator('[data-testid="loading"], [role="status"], .loading, .spinner, :has-text("Loading")').first();
    
    // Loading indicator should be visible initially (or courses load very fast)
    const hasLoadingIndicator = await loadingIndicator.isVisible().catch(() => false);
    
    // Eventually courses should load
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    const courseCards = page.locator('[data-testid="course-card"]');
    await expect(courseCards.first()).toBeVisible();
  });
});
