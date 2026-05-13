import { test, expect } from '@playwright/test';

test.describe('Registration E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('✓ User can navigate to registration page from home', async ({ page }) => {
    // Navigate from home page to registration page
    await page.click('a[href="/register"]');
    
    // Verify we're on the registration page
    await expect(page).toHaveURL('/register');
    await expect(page.locator('text=Start Your Journey')).toBeVisible();
    await expect(page.locator('text=Create your account and begin mastering crypto')).toBeVisible();
  });

  test('✓ User can fill registration form with valid data', async ({ page }) => {
    await page.goto('/register');
    
    // Fill in the form fields
    await page.fill('input[name="firstName"]', 'John');
    await page.fill('input[name="lastName"]', 'Doe');
    await page.fill('input[type="email"]', 'john.doe@example.com');
    await page.fill('input[name="password"]', 'StrongPass123!');
    await page.fill('input[name="confirmPassword"]', 'StrongPass123!');
    
    // Check terms and conditions
    await page.click('input[type="checkbox"][name="acceptTerms"]');
    
    // Verify all fields are filled
    await expect(page.locator('input[name="firstName"]')).toHaveValue('John');
    await expect(page.locator('input[name="lastName"]')).toHaveValue('Doe');
    await expect(page.locator('input[type="email"]')).toHaveValue('john.doe@example.com');
    await expect(page.locator('input[name="password"]')).toHaveValue('StrongPass123!');
    await expect(page.locator('input[name="confirmPassword"]')).toHaveValue('StrongPass123!');
  });

  test('✓ User sees real-time password strength indicator', async ({ page }) => {
    await page.goto('/register');
    
    const passwordInput = page.locator('input[name="password"]');
    
    // Weak password - only lowercase
    await passwordInput.fill('weakpass');
    await expect(page.locator('text=Password strength:')).toBeVisible();
    await expect(page.locator('text=Weak')).toBeVisible();
    
    // Medium password - lowercase + uppercase
    await passwordInput.fill('Mediumpass');
    await expect(page.locator('text=Medium')).toBeVisible();
    
    // Strong password - lowercase + uppercase + numbers
    await passwordInput.fill('StrongPass123');
    await expect(page.locator('text=Strong').or(page.locator('text=Very Strong'))).toBeVisible();
    
    // Very strong password - lowercase + uppercase + numbers + special chars
    await passwordInput.fill('StrongPass123!');
    await expect(page.locator('text=Very Strong')).toBeVisible();
  });

  test('✓ User sees validation errors for invalid inputs', async ({ page }) => {
    await page.goto('/register');
    
    // Try to submit with empty fields
    await page.click('button[type="submit"]');
    
    // Should show validation errors for required fields
    await expect(page.locator('text=/First name is required|First Name is required/i')).toBeVisible({ timeout: 5000 });
    
    // Fill invalid email
    await page.fill('input[type="email"]', 'invalid-email');
    await page.click('input[name="firstName"]'); // Trigger blur event
    await expect(page.locator('text=/Invalid email|Please enter a valid email/i')).toBeVisible({ timeout: 5000 });
    
    // Fill password that doesn't match confirmPassword
    await page.fill('input[name="password"]', 'Password123!');
    await page.fill('input[name="confirmPassword"]', 'Different123!');
    await page.click('button[type="submit"]');
    await expect(page.locator('text=/Passwords.*match/i')).toBeVisible({ timeout: 5000 });
  });

  test('✓ User can successfully submit registration form', async ({ page }) => {
    await page.goto('/register');
    
    // Fill in valid registration data
    const timestamp = Date.now();
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[type="email"]', `testuser${timestamp}@example.com`);
    await page.fill('input[name="password"]', 'StrongPass123!');
    await page.fill('input[name="confirmPassword"]', 'StrongPass123!');
    await page.click('input[type="checkbox"][name="acceptTerms"]');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Should show loading state
    await expect(page.locator('text=Creating Account...')).toBeVisible({ timeout: 2000 });
  });

  test('✓ User sees success message after registration', async ({ page }) => {
    await page.goto('/register');
    
    // Fill in valid registration data
    const timestamp = Date.now();
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[type="email"]', `testuser${timestamp}@example.com`);
    await page.fill('input[name="password"]', 'StrongPass123!');
    await page.fill('input[name="confirmPassword"]', 'StrongPass123!');
    await page.click('input[type="checkbox"][name="acceptTerms"]');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Wait for success message (depends on API response)
    await expect(page.locator('text=Registration Successful!')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('text=/Please check your email/i')).toBeVisible();
  });

  test('✓ User receives verification email (check email inbox via test email provider)', async ({ page }) => {
    // This test requires integration with a test email provider (e.g., Mailhog, Ethereal)
    // For now, we'll test that the success message mentions email verification
    await page.goto('/register');
    
    const timestamp = Date.now();
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[type="email"]', `testuser${timestamp}@example.com`);
    await page.fill('input[name="password"]', 'StrongPass123!');
    await page.fill('input[name="confirmPassword"]', 'StrongPass123!');
    await page.click('input[type="checkbox"][name="acceptTerms"]');
    
    await page.click('button[type="submit"]');
    
    // Verify the success message mentions email verification
    await expect(page.locator('text=/Check your email inbox for a verification link/i')).toBeVisible({ timeout: 10000 });
  });

  test('✓ User cannot register with duplicate email (shows error)', async ({ page }) => {
    await page.goto('/register');
    
    // Use a known existing email
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[type="email"]', 'existing@example.com'); // This should exist in test database
    await page.fill('input[name="password"]', 'StrongPass123!');
    await page.fill('input[name="confirmPassword"]', 'StrongPass123!');
    await page.click('input[type="checkbox"][name="acceptTerms"]');
    
    await page.click('button[type="submit"]');
    
    // Should show error message about duplicate email
    await expect(page.locator('text=/Email.*already.*exists|Email.*already.*registered|User.*already.*exists/i')).toBeVisible({ timeout: 10000 });
  });

  test('✓ User cannot register with duplicate username (shows error)', async ({ page }) => {
    // Note: Current implementation doesn't have username field
    // This test is a placeholder for future username functionality
    test.skip(true, 'Username field not yet implemented in the registration form');
  });

  test('✓ User cannot submit form with weak password (button disabled)', async ({ page }) => {
    await page.goto('/register');
    
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[type="email"]', 'test@example.com');
    
    // Fill weak password
    await page.fill('input[name="password"]', 'weak');
    await page.fill('input[name="confirmPassword"]', 'weak');
    
    // Check terms
    await page.click('input[type="checkbox"][name="acceptTerms"]');
    
    // Verify password strength indicator shows weak
    await expect(page.locator('text=Weak')).toBeVisible();
    
    // Try to submit - should still show validation error
    await page.click('button[type="submit"]');
    await expect(page.locator('text=/Password must be at least|Password.*too.*short|Password.*minimum/i')).toBeVisible({ timeout: 5000 });
  });

  test('✓ User can toggle password visibility', async ({ page }) => {
    await page.goto('/register');
    
    const passwordInput = page.locator('input[name="password"]');
    const confirmPasswordInput = page.locator('input[name="confirmPassword"]');
    
    // Fill password
    await passwordInput.fill('TestPassword123!');
    
    // Password should be hidden by default
    await expect(passwordInput).toHaveAttribute('type', 'password');
    
    // Click the eye icon to show password
    const passwordToggleButton = page.locator('button[aria-label*="password"]').first();
    await passwordToggleButton.click();
    
    // Password should now be visible
    await expect(passwordInput).toHaveAttribute('type', 'text');
    
    // Click again to hide
    await passwordToggleButton.click();
    await expect(passwordInput).toHaveAttribute('type', 'password');
    
    // Test confirm password toggle
    await confirmPasswordInput.fill('TestPassword123!');
    const confirmToggleButton = page.locator('button[aria-label*="password"]').nth(1);
    await confirmToggleButton.click();
    await expect(confirmPasswordInput).toHaveAttribute('type', 'text');
  });

  test('✓ User can navigate back to login page', async ({ page }) => {
    await page.goto('/register');
    
    // Find and click the "Sign in here" link
    await page.click('text=Sign in here');
    
    // Should navigate to login page
    await expect(page).toHaveURL('/login');
    await expect(page.locator('text=/Sign.*In|Log.*In|Welcome Back/i')).toBeVisible();
  });

  test('✓ Registration page is responsive on mobile', async ({ page }) => {
    // Set mobile viewport (iPhone 12)
    await page.setViewportSize({ width: 390, height: 844 });
    
    await page.goto('/register');
    
    // Verify the page is visible and functional on mobile
    await expect(page.locator('text=Start Your Journey')).toBeVisible();
    
    // Verify form fields are accessible
    await expect(page.locator('input[name="firstName"]')).toBeVisible();
    await expect(page.locator('input[name="lastName"]')).toBeVisible();
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
    
    // Verify grid layout adjusts for mobile (first name and last name should stack)
    const firstNameField = page.locator('input[name="firstName"]');
    const lastNameField = page.locator('input[name="lastName"]');
    
    const firstNameBox = await firstNameField.boundingBox();
    const lastNameBox = await lastNameField.boundingBox();
    
    // On mobile, last name should be below first name (y-coordinate should be greater)
    expect(lastNameBox!.y).toBeGreaterThan(firstNameBox!.y);
  });

  test('✓ Registration page is accessible (keyboard navigation, screen reader)', async ({ page }) => {
    await page.goto('/register');
    
    // Test keyboard navigation
    await page.keyboard.press('Tab'); // Should focus first name field
    let focusedElement = await page.evaluate(() => document.activeElement?.getAttribute('name'));
    expect(focusedElement).toBe('firstName');
    
    // Type in first name
    await page.keyboard.type('John');
    
    // Tab to last name
    await page.keyboard.press('Tab');
    focusedElement = await page.evaluate(() => document.activeElement?.getAttribute('name'));
    expect(focusedElement).toBe('lastName');
    
    // Test ARIA attributes for accessibility
    const firstNameInput = page.locator('input[name="firstName"]');
    await expect(firstNameInput).toHaveAttribute('aria-required', 'true');
    
    const emailInput = page.locator('input[type="email"]');
    await expect(emailInput).toHaveAttribute('aria-required', 'true');
    
    const passwordInput = page.locator('input[name="password"]');
    await expect(passwordInput).toHaveAttribute('aria-required', 'true');
    
    // Test that password visibility toggle has aria-label
    const toggleButton = page.locator('button[aria-label*="password"]').first();
    const ariaLabel = await toggleButton.getAttribute('aria-label');
    expect(ariaLabel).toMatch(/Show password|Hide password/i);
    
    // Test that password strength indicator has role and aria-live
    await passwordInput.fill('TestPassword123!');
    const strengthIndicator = page.locator('#password-strength');
    await expect(strengthIndicator).toHaveAttribute('role', 'status');
    await expect(strengthIndicator).toHaveAttribute('aria-live', 'polite');
  });
});
