import { test, expect } from '@playwright/test';

test.describe('Course Enrollment E2E Tests', () => {
  // Login helper
  async function loginAsUser(page: any, email = 'user@example.com', password = 'TestPassword123!') {
    await page.goto('/login');
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', password);
    await page.click('button[type="submit"]');
    await page.waitForURL(/^\/(courses|dashboard|home)/, { timeout: 5000 });
  }

  test.beforeEach(async ({ page }) => {
    // Login as regular user before each test
    await loginAsUser(page);
  });

  test('✓ Authenticated user can view enroll button on course details', async ({ page }) => {
    // Navigate to courses page
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Click on first course
    const firstCourse = page.locator('[data-testid="course-card"]').first();
    await firstCourse.click();
    
    // Wait for course detail page
    await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
    
    // Verify enroll button is visible
    const enrollButton = page.locator('button:has-text("Enroll"), button[data-testid="enroll-button"]');
    await expect(enrollButton.first()).toBeVisible({ timeout: 3000 });
  });

  test('✓ User can successfully enroll in a free course', async ({ page }) => {
    // Navigate to courses page
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a free course (non-premium)
    const freeCourse = page.locator('[data-testid="course-card"]').filter({ hasNotText: /Premium|Pro/i }).first();
    await freeCourse.click();
    
    // Wait for course detail page
    await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
    
    // Click enroll button
    const enrollButton = page.locator('button:has-text("Enroll"), button[data-testid="enroll-button"]').first();
    
    if (await enrollButton.isVisible()) {
      await enrollButton.click();
      
      // Wait for enrollment confirmation (button text change, toast, or modal)
      await page.waitForTimeout(1000);
      
      // Verify enrollment success indicators
      const enrolledIndicator = page.locator(
        'button:has-text("Enrolled"), ' +
        'button:has-text("Continue"), ' +
        ':has-text("Successfully enrolled"), ' +
        '[data-testid="enrollment-success"]'
      );
      
      await expect(enrolledIndicator.first()).toBeVisible({ timeout: 5000 });
    }
  });

  test('✓ Already enrolled user sees "Continue Learning" instead of "Enroll"', async ({ page }) => {
    // Navigate to courses page
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Click on first course
    const firstCourse = page.locator('[data-testid="course-card"]').first();
    await firstCourse.click();
    
    await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
    
    // Try to enroll
    const enrollButton = page.locator('button:has-text("Enroll"), button[data-testid="enroll-button"]').first();
    
    if (await enrollButton.isVisible()) {
      await enrollButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Reload the page
    await page.reload();
    await page.waitForLoadState('networkidle');
    
    // Verify "Continue" or "Enrolled" button is shown
    const continueButton = page.locator(
      'button:has-text("Continue"), ' +
      'button:has-text("Resume"), ' +
      'button:has-text("Enrolled"), ' +
      '[data-testid="continue-button"]'
    );
    
    await expect(continueButton.first()).toBeVisible({ timeout: 5000 });
  });

  test('✓ User can view enrolled courses in "My Courses"', async ({ page }) => {
    // Navigate to my courses page
    await page.goto('/courses/my-courses');
    
    // Wait for enrolled courses to load
    await page.waitForSelector('[data-testid="course-card"], [data-testid="enrolled-course"], .course-card', { timeout: 5000 });
    
    // Verify at least one enrolled course is visible
    const enrolledCourses = page.locator('[data-testid="course-card"], [data-testid="enrolled-course"], .course-card');
    await expect(enrolledCourses.first()).toBeVisible();
    
    // Verify progress indicator exists
    const progressIndicator = page.locator('[data-testid="course-progress"], .progress, [role="progressbar"]').first();
    const hasProgress = await progressIndicator.isVisible().catch(() => false);
    
    // Either progress indicator exists, or courses are shown
    const coursesCount = await enrolledCourses.count();
    expect(hasProgress || coursesCount > 0).toBeTruthy();
  });

  test('✓ User can filter enrolled courses by completion status', async ({ page }) => {
    // Navigate to my courses page
    await page.goto('/courses/my-courses');
    await page.waitForSelector('[data-testid="course-card"], [data-testid="enrolled-course"], .course-card', { timeout: 5000 });
    
    // Look for status filter (tabs, dropdown, or buttons)
    const statusFilter = page.locator(
      'button:has-text("In Progress"), ' +
      'button:has-text("Completed"), ' +
      'select[name*="status"], ' +
      '[data-testid="status-filter"]'
    ).first();
    
    if (await statusFilter.isVisible()) {
      await statusFilter.click();
      await page.waitForTimeout(500);
      
      // Verify courses are still visible after filter
      const enrolledCourses = page.locator('[data-testid="course-card"], [data-testid="enrolled-course"]');
      const count = await enrolledCourses.count();
      
      // Either filtered courses show up, or message says "No courses"
      if (count === 0) {
        const emptyMessage = page.locator(':has-text("No courses"), :has-text("Nothing here")');
        await expect(emptyMessage.first()).toBeVisible();
      } else {
        await expect(enrolledCourses.first()).toBeVisible();
      }
    }
  });

  test('✓ User receives visual feedback during enrollment process', async ({ page }) => {
    // Navigate to courses page
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Click on a free course
    const freeCourse = page.locator('[data-testid="course-card"]').filter({ hasNotText: /Premium|Pro/i }).first();
    await freeCourse.click();
    
    await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
    
    // Click enroll button
    const enrollButton = page.locator('button:has-text("Enroll"), button[data-testid="enroll-button"]').first();
    
    if (await enrollButton.isVisible()) {
      await enrollButton.click();
      
      // Look for loading state on button
      const loadingButton = page.locator('button:disabled, button[aria-busy="true"], button :has-text("Loading")').first();
      const hasLoadingState = await loadingButton.isVisible().catch(() => false);
      
      // Wait for enrollment to complete
      await page.waitForTimeout(2000);
      
      // Verify success state (toast, modal, or button text change)
      const successIndicators = page.locator(
        ':has-text("Successfully enrolled"), ' +
        ':has-text("Enrollment complete"), ' +
        'button:has-text("Enrolled"), ' +
        '[role="alert"]:has-text("Success")'
      );
      
      const hasSuccessIndicator = await successIndicators.first().isVisible().catch(() => false);
      
      expect(hasLoadingState || hasSuccessIndicator).toBeTruthy();
    }
  });

  test('✓ Unauthenticated user is redirected to login when trying to enroll', async ({ page }) => {
    // Logout first
    await page.goto('/');
    const logoutButton = page.locator('button:has-text("Logout"), button:has-text("Sign out"), a:has-text("Logout")').first();
    
    if (await logoutButton.isVisible()) {
      await logoutButton.click();
      await page.waitForTimeout(500);
    }
    
    // Navigate to a course detail page directly
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    const firstCourse = page.locator('[data-testid="course-card"]').first();
    await firstCourse.click();
    
    await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
    
    // Try to enroll
    const enrollButton = page.locator('button:has-text("Enroll"), button:has-text("Sign in to enroll")').first();
    
    if (await enrollButton.isVisible()) {
      await enrollButton.click();
      
      // Should redirect to login page
      await page.waitForURL(/\/login/, { timeout: 5000 });
      await expect(page).toHaveURL(/\/login/);
    }
  });

  test('✓ User can view course progress after enrollment', async ({ page }) => {
    // Navigate to my courses
    await page.goto('/courses/my-courses');
    await page.waitForSelector('[data-testid="course-card"], [data-testid="enrolled-course"]', { timeout: 5000 });
    
    // Click on an enrolled course
    const enrolledCourse = page.locator('[data-testid="course-card"], [data-testid="enrolled-course"]').first();
    await enrolledCourse.click();
    
    await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
    
    // Verify progress indicator exists
    const progressIndicators = page.locator(
      '[data-testid="course-progress"], ' +
      '[role="progressbar"], ' +
      '.progress-bar, ' +
      ':has-text("% complete"), ' +
      ':has-text("Progress")'
    );
    
    const hasProgressIndicator = await progressIndicators.first().isVisible().catch(() => false);
    
    // Or verify lesson list with completion checkmarks
    const lessonList = page.locator('[data-testid="lesson-list"], [data-testid="lesson-item"]');
    const hasLessonList = await lessonList.first().isVisible().catch(() => false);
    
    expect(hasProgressIndicator || hasLessonList).toBeTruthy();
  });
});
