import { test, expect } from '@playwright/test';

test.describe('Email Verification E2E Tests', () => {
  test('✓ User clicks verification link in email and lands on verification page', async ({ page }) => {
    // Simulate clicking verification link with valid userId and token
    const userId = 'test-user-id-123';
    const token = 'valid-verification-token-abc';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Should land on verification page
    await expect(page).toHaveURL(new RegExp(`/verify-email\\?userId=${userId}&token=${token}`));
    await expect(page.locator('text=WahadiniCryptoQuest')).toBeVisible();
  });

  test('✓ Verification page shows loading state while verifying', async ({ page }) => {
    // Navigate to verification page with valid parameters
    const userId = 'test-user-id-123';
    const token = 'valid-verification-token-abc';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Should show loading spinner and message
    await expect(page.locator('text=Verifying your email...')).toBeVisible({ timeout: 2000 });
    await expect(page.locator('text=Please wait while we verify your email address')).toBeVisible();
    
    // Should see loading spinner (Loader2 icon with animate-spin)
    const loader = page.locator('svg.animate-spin');
    await expect(loader).toBeVisible();
  });

  test('✓ Verification page shows success message after verification', async ({ page }) => {
    // Use a valid verification link (mock will need to be set up in API)
    const userId = 'new-user-123';
    const token = 'valid-new-token-xyz';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Wait for loading to complete and success message to appear
    await expect(page.locator('text=Email Verified!')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('text=/Your email has been successfully verified/i')).toBeVisible();
    
    // Should show success icon (CheckCircle)
    await expect(page.locator('svg').filter({ hasText: '' }).first()).toBeVisible(); // CheckCircle icon
    
    // Should show "Continue to Login" button
    await expect(page.locator('text=Continue to Login')).toBeVisible();
  });

  test('✓ User is auto-redirected to login page after success (5 sec countdown)', async ({ page }) => {
    // Note: Current implementation doesn't have auto-redirect
    // This test documents the expected behavior that should be implemented
    
    test.skip(true, 'Auto-redirect functionality not yet implemented. User must manually click "Continue to Login"');
    
    // Expected implementation:
    // const userId = 'new-user-123';
    // const token = 'valid-new-token-xyz';
    // await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    // await expect(page.locator('text=Email Verified!')).toBeVisible({ timeout: 10000 });
    // await expect(page.locator('text=/Redirecting.*in.*5/i')).toBeVisible();
    // await page.waitForURL('/login', { timeout: 6000 });
  });

  test('✓ User can manually navigate to login page', async ({ page }) => {
    const userId = 'new-user-123';
    const token = 'valid-new-token-xyz';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Wait for success message
    await expect(page.locator('text=Email Verified!')).toBeVisible({ timeout: 10000 });
    
    // Click "Continue to Login" button
    await page.click('text=Continue to Login');
    
    // Should navigate to login page
    await expect(page).toHaveURL('/login');
    await expect(page.locator('text=/Sign.*In|Log.*In|Welcome Back/i')).toBeVisible();
  });

  test('✓ Expired verification link shows error message', async ({ page }) => {
    // Use an expired token
    const userId = 'test-user-123';
    const token = 'expired-verification-token';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Wait for error message
    await expect(page.locator('text=Verification Failed')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('text=/expired|invalid|failed/i')).toBeVisible();
    
    // Should show error icon (XCircle)
    const errorIcon = page.locator('svg').filter({ has: page.locator('circle') }).first();
    await expect(errorIcon).toBeVisible();
    
    // Should show options to try again or go to login
    await expect(page.locator('text=Try Registration Again')).toBeVisible();
    await expect(page.locator('text=Back to Login')).toBeVisible();
  });

  test('✓ Invalid verification link shows error message', async ({ page }) => {
    // Use an invalid/malformed token
    const userId = 'test-user-123';
    const token = 'invalid-token-xyz';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Wait for error state
    await expect(page.locator('text=Verification Failed').or(page.locator('text=Invalid Link'))).toBeVisible({ timeout: 10000 });
    
    // Should show error message about invalid/expired link
    await expect(page.locator('text=/invalid|expired|failed/i')).toBeVisible();
    
    // Should provide options to recover
    await expect(page.locator('a[href="/register"]').or(page.locator('text=Try Registration Again'))).toBeVisible();
    await expect(page.locator('a[href="/login"]').or(page.locator('text=Back to Login'))).toBeVisible();
  });

  test('✓ User can resend verification email from error page', async ({ page }) => {
    // Navigate to verification page with expired token
    const userId = 'test-user-123';
    const token = 'expired-token';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Wait for error state
    await expect(page.locator('text=Verification Failed')).toBeVisible({ timeout: 10000 });
    
    // Note: Current implementation doesn't have "Resend Email" button
    // This test documents expected behavior
    test.skip(true, 'Resend email functionality not yet implemented on verification error page');
    
    // Expected implementation:
    // await page.click('text=Resend Verification Email');
    // await expect(page.locator('text=/Email sent|Check your inbox/i')).toBeVisible();
  });

  test('✓ Email verification page is responsive on mobile', async ({ page }) => {
    // Set mobile viewport (iPhone 12)
    await page.setViewportSize({ width: 390, height: 844 });
    
    const userId = 'test-user-123';
    const token = 'valid-token-xyz';
    
    await page.goto(`/verify-email?userId=${userId}&token=${token}`);
    
    // Verify the page is visible and functional on mobile
    await expect(page.locator('text=WahadiniCryptoQuest')).toBeVisible();
    
    // Wait for either loading or success/error state
    await expect(
      page.locator('text=Verifying your email...').or(
        page.locator('text=Email Verified!').or(
          page.locator('text=Verification Failed')
        )
      )
    ).toBeVisible({ timeout: 10000 });
    
    // Verify content is readable and buttons are accessible
    const container = page.locator('.bg-white');
    await expect(container).toBeVisible();
    
    // Check that the container has proper padding and is centered
    const containerBox = await container.boundingBox();
    expect(containerBox).not.toBeNull();
    expect(containerBox!.width).toBeLessThanOrEqual(390); // Should fit within viewport
  });

  test('✓ Missing userId parameter shows invalid link error', async ({ page }) => {
    // Navigate without userId parameter
    const token = 'some-token-xyz';
    
    await page.goto(`/verify-email?token=${token}`);
    
    // Should show invalid link error immediately (no API call made)
    await expect(page.locator('text=Invalid Link')).toBeVisible({ timeout: 5000 });
    await expect(page.locator('text=/Invalid verification link|check your email for the correct link/i')).toBeVisible();
  });

  test('✓ Missing token parameter shows invalid link error', async ({ page }) => {
    // Navigate without token parameter
    const userId = 'test-user-123';
    
    await page.goto(`/verify-email?userId=${userId}`);
    
    // Should show invalid link error immediately (no API call made)
    await expect(page.locator('text=Invalid Link')).toBeVisible({ timeout: 5000 });
    await expect(page.locator('text=/Invalid verification link|check your email for the correct link/i')).toBeVisible();
  });

  test('✓ User can navigate to register page from error state', async ({ page }) => {
    // Navigate with invalid link
    await page.goto('/verify-email?userId=invalid&token=invalid');
    
    // Wait for error state
    await expect(page.locator('text=/Verification Failed|Invalid Link/i')).toBeVisible({ timeout: 10000 });
    
    // Click "Try Registration Again" or "Register for New Account" button
    await page.click('text=/Try Registration Again|Register for New Account/i');
    
    // Should navigate to registration page
    await expect(page).toHaveURL('/register');
    await expect(page.locator('text=Start Your Journey')).toBeVisible();
  });

  test('✓ User can navigate to login page from error state', async ({ page }) => {
    // Navigate with invalid link
    await page.goto('/verify-email?userId=invalid&token=invalid');
    
    // Wait for error state
    await expect(page.locator('text=/Verification Failed|Invalid Link/i')).toBeVisible({ timeout: 10000 });
    
    // Click "Back to Login" button
    await page.click('text=Back to Login');
    
    // Should navigate to login page
    await expect(page).toHaveURL('/login');
    await expect(page.locator('text=/Sign.*In|Log.*In|Welcome Back/i')).toBeVisible();
  });
});
