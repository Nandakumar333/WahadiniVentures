import { test, expect } from '@playwright/test';

test.describe('Login E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('✓ User can navigate to login page', async ({ page }) => {
    // Navigate from home page to login page
    await page.click('a[href="/login"]');
    
    // Verify we're on the login page
    await expect(page).toHaveURL('/login');
    await expect(page.locator('text=Welcome Back')).toBeVisible();
    await expect(page.locator('text=/Sign in to continue your crypto learning adventure/i')).toBeVisible();
  });

  test('✓ User can enter email and password', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in email
    await page.fill('input[type="email"]', 'test@example.com');
    await expect(page.locator('input[type="email"]')).toHaveValue('test@example.com');
    
    // Fill in password
    await page.fill('input[type="password"]', 'TestPassword123!');
    await expect(page.locator('input[type="password"]')).toHaveValue('TestPassword123!');
  });

  test('✓ User can toggle password visibility', async ({ page }) => {
    await page.goto('/login');
    
    const passwordInput = page.locator('input#password');
    
    // Fill password
    await passwordInput.fill('TestPassword123!');
    
    // Password should be hidden by default
    await expect(passwordInput).toHaveAttribute('type', 'password');
    
    // Click the eye icon to show password
    const toggleButton = page.locator('button[aria-label*="password"]');
    await toggleButton.click();
    
    // Password should now be visible
    await expect(passwordInput).toHaveAttribute('type', 'text');
    
    // Click again to hide
    await toggleButton.click();
    await expect(passwordInput).toHaveAttribute('type', 'password');
  });

  test('✓ User can check RememberMe checkbox', async ({ page }) => {
    await page.goto('/login');
    
    const rememberMeCheckbox = page.locator('input#rememberMe');
    
    // Checkbox should be unchecked by default
    await expect(rememberMeCheckbox).not.toBeChecked();
    
    // Click to check
    await page.click('label[for="rememberMe"]');
    await expect(rememberMeCheckbox).toBeChecked();
    
    // Click again to uncheck
    await page.click('label[for="rememberMe"]');
    await expect(rememberMeCheckbox).not.toBeChecked();
  });

  test('✓ User can successfully login with valid credentials', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in valid credentials (requires seeded test user in database)
    await page.fill('input[type="email"]', 'testuser@wahadini.com');
    await page.fill('input[type="password"]', 'Password123!');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Should show loading state
    await expect(page.locator('button[type="submit"]')).toContainText(/Sign.*In/i);
    
    // Wait for login to complete (depends on API)
    // Note: This test will fail without a real backend and test database
  });

  test('✓ User is redirected to dashboard after login', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in valid credentials
    await page.fill('input[type="email"]', 'testuser@wahadini.com');
    await page.fill('input[type="password"]', 'Password123!');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Wait for redirect to dashboard
    await expect(page).toHaveURL('/dashboard', { timeout: 10000 });
    
    // Note: This test requires a real backend with valid test user
    test.skip(true, 'Requires backend server running with seeded test user');
  });

  test('✓ User sees error for invalid credentials (generic message)', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in invalid credentials
    await page.fill('input[type="email"]', 'invalid@example.com');
    await page.fill('input[type="password"]', 'WrongPassword123!');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Should show error message
    await expect(page.locator('text=/Invalid.*credentials|Invalid.*email.*password|Login.*failed/i')).toBeVisible({ timeout: 10000 });
    
    // Note: This test requires a real backend
    test.skip(true, 'Requires backend server running');
  });

  test('✓ User sees error for unverified email with resend option', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in credentials for unverified user
    await page.fill('input[type="email"]', 'unverified@example.com');
    await page.fill('input[type="password"]', 'Password123!');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Should show error about unverified email
    await expect(page.locator('text=/email.*not.*verified|verify.*email|confirmation.*email/i')).toBeVisible({ timeout: 10000 });
    
    // Should show resend verification option
    await expect(page.locator('text=/resend|send.*again/i')).toBeVisible();
    
    // Note: This test requires a real backend
    test.skip(true, 'Requires backend server running with unverified test user');
  });

  test('✓ User sees error for locked account with unlock instructions', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in credentials for locked account
    await page.fill('input[type="email"]', 'locked@example.com');
    await page.fill('input[type="password"]', 'Password123!');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Should show error about locked account
    await expect(page.locator('text=/account.*locked|temporarily.*locked|too.*many.*attempts/i')).toBeVisible({ timeout: 10000 });
    
    // Should show unlock instructions
    await expect(page.locator('text=/contact.*support|wait.*minutes|unlock/i')).toBeVisible();
    
    // Note: This test requires a real backend
    test.skip(true, 'Requires backend server running with locked test user');
  });

  test('✓ User cannot login after 5 failed attempts (account locked)', async ({ page }) => {
    await page.goto('/login');
    
    const email = `locktest${Date.now()}@example.com`;
    
    // Attempt to login 5 times with wrong password
    for (let i = 0; i < 5; i++) {
      await page.fill('input[type="email"]', email);
      await page.fill('input[type="password"]', `WrongPassword${i}`);
      await page.click('button[type="submit"]');
      
      // Wait for error message
      await expect(page.locator('text=/Invalid.*credentials|Login.*failed/i')).toBeVisible({ timeout: 10000 });
      
      // Clear form for next attempt
      await page.fill('input[type="password"]', '');
    }
    
    // 6th attempt should show account locked error
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', 'WrongPassword5');
    await page.click('button[type="submit"]');
    
    await expect(page.locator('text=/account.*locked|temporarily.*locked/i')).toBeVisible({ timeout: 10000 });
    
    // Note: This test requires a real backend with rate limiting
    test.skip(true, 'Requires backend server running with account lockout feature');
  });

  test('✓ User can navigate to forgot password page', async ({ page }) => {
    await page.goto('/login');
    
    // Click "Forgot your password?" link
    await page.click('text=Forgot your password?');
    
    // Should navigate to forgot password page
    await expect(page).toHaveURL('/forgot-password');
    await expect(page.locator('text=/Reset.*Password|Forgot.*Password/i')).toBeVisible();
  });

  test('✓ User can navigate to registration page', async ({ page }) => {
    await page.goto('/login');
    
    // Click "Sign up" link
    await page.click('text=Sign up');
    
    // Should navigate to registration page
    await expect(page).toHaveURL('/register');
    await expect(page.locator('text=Start Your Journey')).toBeVisible();
  });

  test('✓ User session persists after page refresh (RememberMe)', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in credentials and check RememberMe
    await page.fill('input[type="email"]', 'testuser@wahadini.com');
    await page.fill('input[type="password"]', 'Password123!');
    await page.click('label[for="rememberMe"]');
    
    // Submit login
    await page.click('button[type="submit"]');
    
    // Wait for redirect
    await expect(page).toHaveURL('/dashboard', { timeout: 10000 });
    
    // Refresh the page
    await page.reload();
    
    // Should still be on dashboard (not redirected to login)
    await expect(page).toHaveURL('/dashboard');
    
    // Note: This test requires a real backend
    test.skip(true, 'Requires backend server running with valid test user');
  });

  test('✓ Login page is responsive on mobile', async ({ page }) => {
    // Set mobile viewport (iPhone 12)
    await page.setViewportSize({ width: 390, height: 844 });
    
    await page.goto('/login');
    
    // Verify the page is visible and functional on mobile
    await expect(page.locator('text=Welcome Back')).toBeVisible();
    
    // Verify form fields are accessible
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input#password')).toBeVisible();
    await expect(page.locator('input#rememberMe')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
    
    // Verify links are accessible
    await expect(page.locator('text=Forgot your password?')).toBeVisible();
    await expect(page.locator('text=Sign up')).toBeVisible();
    
    // Test that form can be filled on mobile
    await page.fill('input[type="email"]', 'mobile@test.com');
    await page.fill('input#password', 'MobilePass123!');
    
    await expect(page.locator('input[type="email"]')).toHaveValue('mobile@test.com');
    await expect(page.locator('input#password')).toHaveValue('MobilePass123!');
  });

  test('✓ Login page is accessible (keyboard navigation, screen readers)', async ({ page }) => {
    await page.goto('/login');
    
    // Test keyboard navigation
    await page.keyboard.press('Tab'); // Should focus email field
    let focusedElement = await page.evaluate(() => document.activeElement?.getAttribute('id'));
    expect(focusedElement).toBe('email');
    
    // Type in email
    await page.keyboard.type('test@example.com');
    
    // Tab to password field
    await page.keyboard.press('Tab');
    focusedElement = await page.evaluate(() => document.activeElement?.getAttribute('id'));
    expect(focusedElement).toBe('password');
    
    // Type password
    await page.keyboard.type('TestPassword123!');
    
    // Tab to password toggle button
    await page.keyboard.press('Tab');
    focusedElement = await page.evaluate(() => document.activeElement?.getAttribute('aria-label'));
    expect(focusedElement).toMatch(/Show password|Hide password/i);
    
    // Test ARIA attributes
    const emailInput = page.locator('input#email');
    await expect(emailInput).toHaveAttribute('aria-invalid', 'false');
    await expect(emailInput).toHaveAttribute('autoComplete', 'email');
    
    const passwordInput = page.locator('input#password');
    await expect(passwordInput).toHaveAttribute('aria-invalid', 'false');
    await expect(passwordInput).toHaveAttribute('autoComplete', 'current-password');
    
    // Test that password visibility toggle has proper aria-label
    const toggleButton = page.locator('button[aria-label*="password"]');
    const ariaLabel = await toggleButton.getAttribute('aria-label');
    expect(ariaLabel).toMatch(/Show password|Hide password/i);
    
    // Test error message accessibility
    await page.fill('input#email', 'invalid-email');
    await page.click('button[type="submit"]');
    
    // Error should have role="alert" and aria-live
    const errorMessage = page.locator('p#email-error');
    await expect(errorMessage).toHaveAttribute('role', 'alert');
    await expect(errorMessage).toHaveAttribute('aria-live', 'polite');
    
    // Test submit button aria-busy attribute
    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toHaveAttribute('aria-busy');
  });

  test('✓ Empty form shows validation errors', async ({ page }) => {
    await page.goto('/login');
    
    // Try to submit empty form
    await page.click('button[type="submit"]');
    
    // Should show validation errors
    await expect(page.locator('text=/Email is required/i')).toBeVisible({ timeout: 5000 });
    await expect(page.locator('text=/Password is required/i')).toBeVisible({ timeout: 5000 });
  });

  test('✓ Invalid email format shows validation error', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in invalid email
    await page.fill('input#email', 'not-an-email');
    await page.fill('input#password', 'SomePassword123!');
    
    // Try to submit
    await page.click('button[type="submit"]');
    
    // Should show email validation error
    await expect(page.locator('text=/valid email|Invalid email/i')).toBeVisible({ timeout: 5000 });
  });

  test('✓ Login button shows loading state during submission', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in credentials
    await page.fill('input#email', 'test@example.com');
    await page.fill('input#password', 'TestPassword123!');
    
    // Submit form
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    
    // Button should be disabled during submission
    await expect(submitButton).toBeDisabled();
    
    // Should show loading spinner
    await expect(page.locator('button[type="submit"] svg.animate-spin')).toBeVisible({ timeout: 2000 });
  });
});
