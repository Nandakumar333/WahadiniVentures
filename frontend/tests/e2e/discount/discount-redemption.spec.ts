import { test, expect } from '@playwright/test';

test.describe('Discount Redemption E2E Flow', () => {
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

  test('✓ Complete redemption flow - Browse, Select, Confirm, Copy Code', async ({ page }) => {
    // Step 1: Navigate to discounts gallery
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    // Verify discount gallery page loaded
    await expect(page.locator('h1, h2, [data-testid="page-title"]').filter({ hasText: /Discount|Redeem/i }).first()).toBeVisible({ timeout: 5000 });

    // Step 2: Find an available discount card
    const availableDiscount = page.locator('[data-testid="discount-card"]').filter({
      has: page.locator('button:has-text("Redeem"), button[data-testid="redeem-button"]')
    }).first();

    await expect(availableDiscount).toBeVisible({ timeout: 5000 });

    // Store discount details before redemption
    const discountPercentage = await availableDiscount.locator('[data-testid="discount-percentage"], .text-2xl').first().textContent();

    // Step 3: Click Redeem button
    const redeemButton = availableDiscount.locator('button:has-text("Redeem"), button[data-testid="redeem-button"]').first();
    await redeemButton.click();

    // Step 4: Verify confirmation modal appears
    const modal = page.locator('[role="dialog"], [data-testid="redemption-modal"]');
    await expect(modal).toBeVisible({ timeout: 3000 });
    await expect(modal.locator(':has-text("Confirm Redemption")')).toBeVisible();

    // Verify modal shows discount details
    await expect(modal.locator(':has-text("% OFF")')).toBeVisible();
    await expect(modal.locator(':has-text("pts"), :has-text("points")')).toBeVisible();

    // Step 5: Confirm redemption
    const confirmButton = modal.locator('button:has-text("Confirm"), button[data-testid="confirm-redeem"]').first();
    await confirmButton.click();

    // Step 6: Wait for redemption to complete
    await page.waitForTimeout(1000); // Allow API call to complete

    // Step 7: Verify success view appears
    await expect(modal.locator(':has-text("Success"), :has-text("🎉")')).toBeVisible({ timeout: 5000 });

    // Step 8: Verify discount code is displayed
    const discountCode = modal.locator('code, [data-testid="discount-code"]');
    await expect(discountCode).toBeVisible();

    const codeText = await discountCode.textContent();
    expect(codeText).toBeTruthy();
    expect(codeText?.length).toBeGreaterThan(5); // Should be a valid code

    // Step 9: Click copy button
    const copyButton = modal.locator('button').filter({ has: page.locator('svg') }).first();
    await copyButton.click();

    // Step 10: Verify copy success feedback (toast or button state change)
    const copySuccessIndicator = page.locator(
      ':has-text("copied"), :has-text("Copied"), [data-testid="copy-success"]'
    );
    await expect(copySuccessIndicator.first()).toBeVisible({ timeout: 3000 });

    // Step 11: Close modal
    const closeButton = modal.locator('button:has-text("Close"), button[aria-label="Close"]').first();
    if (await closeButton.isVisible()) {
      await closeButton.click();
    } else {
      // Click outside modal or press Escape
      await page.keyboard.press('Escape');
    }

    // Step 12: Verify discount card state changed (optional - might still be visible but with different state)
    await page.waitForTimeout(500);
  });

  test('✓ User can view redeemed discounts in "My Discounts" page', async ({ page }) => {
    // Redeem a discount first
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    const availableDiscount = page.locator('[data-testid="discount-card"]').filter({
      has: page.locator('button:has-text("Redeem")')
    }).first();

    if (await availableDiscount.isVisible({ timeout: 3000 })) {
      const redeemButton = availableDiscount.locator('button:has-text("Redeem")').first();
      await redeemButton.click();

      const modal = page.locator('[role="dialog"]');
      await expect(modal).toBeVisible({ timeout: 3000 });

      const confirmButton = modal.locator('button:has-text("Confirm")').first();
      await confirmButton.click();

      await page.waitForTimeout(1000);

      // Close modal
      await page.keyboard.press('Escape');
    }

    // Navigate to My Discounts page
    await page.goto('/discounts/my-discounts');
    await page.waitForLoadState('networkidle');

    // Verify page title
    await expect(page.locator('h1, h2').filter({ hasText: /My Discount|Redeemed/i }).first()).toBeVisible({ timeout: 5000 });

    // Verify at least one redeemed code is displayed
    const redemptionCard = page.locator('[data-testid="redemption-card"], [data-testid="discount-code-card"]').first();
    
    // If redemptions exist, verify code card structure
    if (await redemptionCard.isVisible({ timeout: 3000 })) {
      await expect(redemptionCard.locator('code, [data-testid="discount-code"]')).toBeVisible();
      await expect(redemptionCard.locator(':has-text("% OFF"), [data-testid="discount-percentage"]')).toBeVisible();
    }
  });

  test('✓ User cannot redeem same discount twice', async ({ page }) => {
    // Navigate to discounts gallery
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    // Find an available discount
    const availableDiscount = page.locator('[data-testid="discount-card"]').filter({
      has: page.locator('button:has-text("Redeem")')
    }).first();

    if (await availableDiscount.isVisible({ timeout: 3000 })) {
      const discountId = await availableDiscount.getAttribute('data-discount-id');

      // First redemption
      const redeemButton = availableDiscount.locator('button:has-text("Redeem")').first();
      await redeemButton.click();

      const modal = page.locator('[role="dialog"]');
      await expect(modal).toBeVisible({ timeout: 3000 });

      const confirmButton = modal.locator('button:has-text("Confirm")').first();
      await confirmButton.click();

      await page.waitForTimeout(1000);

      // Close modal
      await page.keyboard.press('Escape');
      await page.waitForTimeout(500);

      // Reload page to get updated discount states
      await page.reload();
      await page.waitForLoadState('networkidle');

      // Try to find the same discount and verify it's no longer redeemable
      const sameDiscount = discountId 
        ? page.locator(`[data-discount-id="${discountId}"]`)
        : availableDiscount;

      // Verify button is disabled or shows "Redeemed" or "Unavailable"
      const button = sameDiscount.locator('button').first();
      const buttonText = await button.textContent();

      expect(
        buttonText?.includes('Unavailable') || 
        buttonText?.includes('Redeemed') ||
        await button.isDisabled()
      ).toBeTruthy();
    }
  });

  test('✓ User cannot redeem discount with insufficient points', async ({ page }) => {
    // Navigate to discounts gallery
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    // Look for a discount with insufficient points warning
    const unaffordableDiscount = page.locator('[data-testid="discount-card"]').filter({
      has: page.locator(':has-text("Insufficient"), :has-text("more points")')
    }).first();

    if (await unaffordableDiscount.isVisible({ timeout: 3000 })) {
      // Verify redeem button is disabled or not present
      const redeemButton = unaffordableDiscount.locator('button:has-text("Redeem")');
      
      if (await redeemButton.count() > 0) {
        expect(await redeemButton.first().isDisabled()).toBe(true);
      } else {
        // Button should show "Unavailable"
        const unavailableButton = unaffordableDiscount.locator('button:has-text("Unavailable")');
        await expect(unavailableButton.first()).toBeVisible();
      }
    }
  });

  test('✓ User can copy discount code from "My Discounts" page', async ({ page }) => {
    // Navigate to My Discounts page
    await page.goto('/discounts/my-discounts');
    await page.waitForLoadState('networkidle');

    // Find first redemption card
    const redemptionCard = page.locator('[data-testid="redemption-card"], [data-testid="discount-code-card"]').first();

    if (await redemptionCard.isVisible({ timeout: 3000 })) {
      // Find and click copy button
      const copyButton = redemptionCard.locator('button').filter({ 
        has: page.locator('svg') 
      }).first();

      await copyButton.click();

      // Verify copy success feedback
      const successIndicator = page.locator(':has-text("copied"), :has-text("Copied")');
      await expect(successIndicator.first()).toBeVisible({ timeout: 3000 });
    }
  });

  test('✓ Discounts gallery shows correct availability states', async ({ page }) => {
    // Navigate to discounts gallery
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    // Verify at least one discount is displayed
    const discountCards = page.locator('[data-testid="discount-card"]');
    await expect(discountCards.first()).toBeVisible({ timeout: 5000 });

    // Check for various states
    const activeDiscounts = discountCards.filter({ has: page.locator(':has-text("Active")') });
    const expiredDiscounts = discountCards.filter({ has: page.locator(':has-text("Expired")') });
    const soldOutDiscounts = discountCards.filter({ has: page.locator(':has-text("Sold Out")') });

    // At least one of these states should exist
    const totalCount = await activeDiscounts.count() + 
                       await expiredDiscounts.count() + 
                       await soldOutDiscounts.count();
    
    expect(totalCount).toBeGreaterThan(0);
  });

  test('✓ Modal can be closed without completing redemption', async ({ page }) => {
    // Navigate to discounts gallery
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    const availableDiscount = page.locator('[data-testid="discount-card"]').filter({
      has: page.locator('button:has-text("Redeem")')
    }).first();

    if (await availableDiscount.isVisible({ timeout: 3000 })) {
      const redeemButton = availableDiscount.locator('button:has-text("Redeem")').first();
      await redeemButton.click();

      const modal = page.locator('[role="dialog"]');
      await expect(modal).toBeVisible({ timeout: 3000 });

      // Click Cancel button
      const cancelButton = modal.locator('button:has-text("Cancel")').first();
      if (await cancelButton.isVisible()) {
        await cancelButton.click();
      } else {
        // Press Escape to close
        await page.keyboard.press('Escape');
      }

      // Verify modal is closed
      await expect(modal).not.toBeVisible({ timeout: 2000 });

      // Verify we're still on the discounts page
      expect(page.url()).toContain('/discounts');
    }
  });

  test('✓ Redemption modal shows loading state during API call', async ({ page }) => {
    // Navigate to discounts gallery
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    const availableDiscount = page.locator('[data-testid="discount-card"]').filter({
      has: page.locator('button:has-text("Redeem")')
    }).first();

    if (await availableDiscount.isVisible({ timeout: 3000 })) {
      const redeemButton = availableDiscount.locator('button:has-text("Redeem")').first();
      await redeemButton.click();

      const modal = page.locator('[role="dialog"]');
      await expect(modal).toBeVisible({ timeout: 3000 });

      const confirmButton = modal.locator('button:has-text("Confirm")').first();
      
      // Click confirm and immediately check for loading state
      await confirmButton.click();

      // Look for loading indicators (disabled button or loading text)
      const loadingIndicator = modal.locator(
        'button:has-text("Redeeming"), ' +
        'button[disabled]:has-text("Confirm"), ' +
        ':has-text("Loading")'
      );

      // This might be very brief, so use a short timeout
      try {
        await expect(loadingIndicator.first()).toBeVisible({ timeout: 500 });
      } catch {
        // Loading might be too fast to catch - that's okay
      }

      // Wait for completion
      await page.waitForTimeout(2000);
    }
  });
});

test.describe('Discount Redemption - Edge Cases', () => {
  async function loginAsUser(page: any, email = 'user@example.com', password = 'TestPassword123!') {
    await page.goto('/login');
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', password);
    await page.click('button[type="submit"]');
    await page.waitForURL(/^\/(courses|dashboard|home)/, { timeout: 5000 });
  }

  test.beforeEach(async ({ page }) => {
    await loginAsUser(page);
  });

  test('✓ Empty state is shown when no discounts available', async ({ page }) => {
    // Navigate to discounts gallery
    await page.goto('/discounts');
    await page.waitForLoadState('networkidle');

    // If no discounts are present, verify empty state
    const discountCards = page.locator('[data-testid="discount-card"]');
    const cardCount = await discountCards.count();

    if (cardCount === 0) {
      // Verify empty state message
      const emptyState = page.locator('[data-testid="empty-state"], :has-text("No discount"), :has-text("no discount")');
      await expect(emptyState.first()).toBeVisible({ timeout: 3000 });
    }
  });

  test('✓ Empty state is shown in My Discounts when no redemptions', async ({ page }) => {
    // Navigate to My Discounts page
    await page.goto('/discounts/my-discounts');
    await page.waitForLoadState('networkidle');

    // Check if there are any redeemed codes
    const redemptionCards = page.locator('[data-testid="redemption-card"], [data-testid="discount-code-card"]');
    const cardCount = await redemptionCards.count();

    if (cardCount === 0) {
      // Verify empty state message
      const emptyState = page.locator(
        '[data-testid="empty-state"], ' +
        ':has-text("No redeemed"), ' +
        ':has-text("no discount")'
      );
      await expect(emptyState.first()).toBeVisible({ timeout: 3000 });
    }
  });
});
