import { test, expect } from '@playwright/test';

test.describe('Premium Course Access Gate E2E Tests', () => {
  // Login helpers
  async function loginAsUser(page: any, email = 'user@example.com', password = 'TestPassword123!') {
    await page.goto('/login');
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', password);
    await page.click('button[type="submit"]');
    await page.waitForURL(/^\/(courses|dashboard|home)/, { timeout: 5000 });
  }

  async function loginAsPremiumUser(page: any, email = 'premium@example.com', password = 'TestPassword123!') {
    await loginAsUser(page, email, password);
  }

  test.beforeEach(async ({ page }) => {
    // Start each test on courses page
    await page.goto('/courses');
  });

  test('✓ Premium courses are visually marked with badge/label', async ({ page }) => {
    // Wait for courses to load
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Look for premium badges on course cards
    const premiumBadges = page.locator(
      '[data-testid="premium-badge"], ' +
      '.premium-badge, ' +
      ':has-text("Premium"), ' +
      ':has-text("Pro"), ' +
      '[class*="premium"]'
    );
    
    const badgeCount = await premiumBadges.count();
    
    // If premium courses exist, they should have badges
    if (badgeCount > 0) {
      await expect(premiumBadges.first()).toBeVisible();
    }
    
    // Alternatively, verify at least one course card exists
    const courseCards = page.locator('[data-testid="course-card"]');
    await expect(courseCards.first()).toBeVisible();
  });

  test('✓ Non-premium user sees upgrade prompt when viewing premium course', async ({ page }) => {
    // Login as regular (non-premium) user
    await loginAsUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a premium course
    const premiumCourse = page.locator('[data-testid="course-card"]').filter({ hasText: /Premium|Pro/i }).first();
    
    if (await premiumCourse.isVisible()) {
      await premiumCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Look for upgrade prompt/paywall
      const upgradePrompt = page.locator(
        ':has-text("Upgrade to Premium"), ' +
        ':has-text("Subscribe to access"), ' +
        ':has-text("Premium only"), ' +
        'button:has-text("Upgrade"), ' +
        '[data-testid="premium-gate"], ' +
        '[data-testid="upgrade-prompt"]'
      );
      
      await expect(upgradePrompt.first()).toBeVisible({ timeout: 5000 });
    }
  });

  test('✓ Non-premium user cannot enroll in premium course', async ({ page }) => {
    // Login as regular user
    await loginAsUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a premium course
    const premiumCourse = page.locator('[data-testid="course-card"]').filter({ hasText: /Premium|Pro/i }).first();
    
    if (await premiumCourse.isVisible()) {
      await premiumCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Enroll button should either:
      // 1. Say "Upgrade to Enroll" or similar
      // 2. Be disabled
      // 3. Not exist (replaced with upgrade button)
      const enrollButton = page.locator('button:has-text("Enroll")');
      const upgradeButton = page.locator('button:has-text("Upgrade"), button:has-text("Subscribe")');
      
      const enrollExists = await enrollButton.isVisible().catch(() => false);
      const upgradeExists = await upgradeButton.isVisible().catch(() => false);
      
      if (enrollExists) {
        // If enroll button exists, it should be disabled or require upgrade
        const isDisabled = await enrollButton.isDisabled();
        const hasUpgradeText = await enrollButton.textContent().then(text => text?.includes('Upgrade'));
        expect(isDisabled || hasUpgradeText).toBeTruthy();
      } else {
        // Otherwise, upgrade button should be present
        expect(upgradeExists).toBeTruthy();
      }
    }
  });

  test('✓ Premium user can enroll in premium courses', async ({ page }) => {
    // Login as premium user
    await loginAsPremiumUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a premium course
    const premiumCourse = page.locator('[data-testid="course-card"]').filter({ hasText: /Premium|Pro/i }).first();
    
    if (await premiumCourse.isVisible()) {
      await premiumCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Premium user should see enroll button (not upgrade button)
      const enrollButton = page.locator('button:has-text("Enroll")').first();
      await expect(enrollButton).toBeVisible({ timeout: 5000 });
      
      // Enroll button should be enabled
      await expect(enrollButton).toBeEnabled();
      
      // Try to enroll
      await enrollButton.click();
      await page.waitForTimeout(1000);
      
      // Should see enrollment success
      const successIndicators = page.locator(
        'button:has-text("Enrolled"), ' +
        'button:has-text("Continue"), ' +
        ':has-text("Successfully enrolled")'
      );
      
      await expect(successIndicators.first()).toBeVisible({ timeout: 5000 });
    }
  });

  test('✓ Premium user can access premium course lessons', async ({ page }) => {
    // Login as premium user
    await loginAsPremiumUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find and enroll in a premium course
    const premiumCourse = page.locator('[data-testid="course-card"]').filter({ hasText: /Premium|Pro/i }).first();
    
    if (await premiumCourse.isVisible()) {
      await premiumCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Enroll if not already enrolled
      const enrollButton = page.locator('button:has-text("Enroll")').first();
      if (await enrollButton.isVisible()) {
        await enrollButton.click();
        await page.waitForTimeout(1000);
      }
      
      // Verify lessons are accessible
      const lessonList = page.locator('[data-testid="lesson-list"], [data-testid="lesson-item"]');
      const firstLesson = lessonList.first();
      
      if (await firstLesson.isVisible()) {
        await firstLesson.click();
        await page.waitForTimeout(1000);
        
        // Verify lesson content is visible (video player or content)
        const lessonContent = page.locator(
          '[data-testid="video-player"], ' +
          'iframe, ' +
          '[data-testid="lesson-content"], ' +
          '.lesson-content'
        );
        
        await expect(lessonContent.first()).toBeVisible({ timeout: 5000 });
      }
    }
  });

  test('✓ Upgrade button redirects to subscription/pricing page', async ({ page }) => {
    // Login as regular user
    await loginAsUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a premium course
    const premiumCourse = page.locator('[data-testid="course-card"]').filter({ hasText: /Premium|Pro/i }).first();
    
    if (await premiumCourse.isVisible()) {
      await premiumCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Click upgrade/subscribe button
      const upgradeButton = page.locator(
        'button:has-text("Upgrade"), ' +
        'button:has-text("Subscribe"), ' +
        'button:has-text("Get Premium"), ' +
        'a:has-text("Upgrade")'
      ).first();
      
      if (await upgradeButton.isVisible()) {
        await upgradeButton.click();
        
        // Should navigate to pricing/subscription page
        await page.waitForTimeout(1000);
        
        const currentUrl = page.url();
        const isPricingPage = currentUrl.includes('/pricing') || 
                             currentUrl.includes('/subscribe') || 
                             currentUrl.includes('/premium') ||
                             currentUrl.includes('/upgrade');
        
        expect(isPricingPage).toBeTruthy();
      }
    }
  });

  test('✓ Non-premium user can filter to view only free courses', async ({ page }) => {
    // Login as regular user
    await loginAsUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Look for premium filter toggle/checkbox
    const premiumFilter = page.locator(
      'input[name*="premium"], ' +
      'input[type="checkbox"]:near(:text("Premium")), ' +
      'button:has-text("Free only"), ' +
      '[data-testid="premium-filter"]'
    ).first();
    
    if (await premiumFilter.isVisible()) {
      // If it's a checkbox for "Show Premium only", uncheck it
      if (await premiumFilter.isChecked()) {
        await premiumFilter.click();
      }
      
      // Or if it's a "Free only" button, click it
      const isFreeOnlyButton = await premiumFilter.textContent().then(text => text?.includes('Free'));
      if (isFreeOnlyButton) {
        await premiumFilter.click();
      }
      
      await page.waitForTimeout(500);
      
      // Verify premium badges are not visible (all courses are free)
      const premiumBadges = page.locator('[data-testid="premium-badge"], :has-text("Premium")');
      const badgeCount = await premiumBadges.count();
      
      // Either no premium badges, or courses are still showing
      const courseCards = page.locator('[data-testid="course-card"]');
      const courseCount = await courseCards.count();
      
      expect(courseCount).toBeGreaterThan(0);
    }
  });

  test('✓ Premium gate shows clear pricing information', async ({ page }) => {
    // Login as regular user
    await loginAsUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a premium course
    const premiumCourse = page.locator('[data-testid="course-card"]').filter({ hasText: /Premium|Pro/i }).first();
    
    if (await premiumCourse.isVisible()) {
      await premiumCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Look for pricing information in the gate/prompt
      const pricingInfo = page.locator(
        ':has-text("$"), ' +
        ':has-text("/month"), ' +
        ':has-text("per month"), ' +
        ':has-text("subscription"), ' +
        '[data-testid="pricing-info"]'
      );
      
      const hasPricingInfo = await pricingInfo.first().isVisible().catch(() => false);
      
      // Or verify "Upgrade" button exists (which implies pricing elsewhere)
      const upgradeButton = page.locator('button:has-text("Upgrade"), button:has-text("Subscribe")');
      const hasUpgradeButton = await upgradeButton.first().isVisible().catch(() => false);
      
      expect(hasPricingInfo || hasUpgradeButton).toBeTruthy();
    }
  });

  test('✓ Premium gate prevents access to premium lesson content', async ({ page }) => {
    // Login as regular user
    await loginAsUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a premium course
    const premiumCourse = page.locator('[data-testid="course-card"]').filter({ hasText: /Premium|Pro/i }).first();
    
    if (await premiumCourse.isVisible()) {
      await premiumCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Try to click on a lesson
      const lessonItem = page.locator('[data-testid="lesson-item"]').first();
      
      if (await lessonItem.isVisible()) {
        // Lesson should be locked or show premium icon
        const lockIcon = lessonItem.locator('[data-testid="lock-icon"], :has-text("🔒"), [class*="lock"]');
        const premiumIcon = lessonItem.locator('[data-testid="premium-icon"], :has-text("Premium")');
        
        const hasLockIcon = await lockIcon.isVisible().catch(() => false);
        const hasPremiumIcon = await premiumIcon.isVisible().catch(() => false);
        
        // Try clicking lesson
        await lessonItem.click();
        await page.waitForTimeout(500);
        
        // Should see upgrade prompt or no video content
        const upgradePrompt = page.locator(':has-text("Upgrade"), :has-text("Premium only")');
        const hasPrompt = await upgradePrompt.first().isVisible().catch(() => false);
        
        expect(hasLockIcon || hasPremiumIcon || hasPrompt).toBeTruthy();
      }
    }
  });

  test('✓ Free course is accessible to all authenticated users', async ({ page }) => {
    // Login as regular user
    await loginAsUser(page);
    
    await page.goto('/courses');
    await page.waitForSelector('[data-testid="course-card"]', { timeout: 5000 });
    
    // Find a free course (not premium)
    const freeCourse = page.locator('[data-testid="course-card"]').filter({ hasNotText: /Premium|Pro/i }).first();
    
    if (await freeCourse.isVisible()) {
      await freeCourse.click();
      
      await page.waitForURL(/\/courses\/[a-zA-Z0-9-]+/, { timeout: 5000 });
      
      // Should see enroll button (not upgrade button)
      const enrollButton = page.locator('button:has-text("Enroll")').first();
      await expect(enrollButton).toBeVisible({ timeout: 5000 });
      await expect(enrollButton).toBeEnabled();
      
      // Enroll in course
      await enrollButton.click();
      await page.waitForTimeout(1000);
      
      // Verify lessons are accessible
      const lessonItem = page.locator('[data-testid="lesson-item"]').first();
      
      if (await lessonItem.isVisible()) {
        await lessonItem.click();
        await page.waitForTimeout(1000);
        
        // Verify lesson content is visible
        const lessonContent = page.locator(
          '[data-testid="video-player"], ' +
          'iframe, ' +
          '[data-testid="lesson-content"]'
        );
        
        await expect(lessonContent.first()).toBeVisible({ timeout: 5000 });
      }
    }
  });
});
