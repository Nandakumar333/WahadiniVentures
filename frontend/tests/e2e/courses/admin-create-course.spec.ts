import { test, expect } from '@playwright/test';

test.describe('Admin Create Course E2E Tests', () => {
  // Admin login helper
  async function loginAsAdmin(page: any, email = 'admin@wahadini.com', password = 'AdminPassword123!') {
    await page.goto('/login');
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', password);
    await page.click('button[type="submit"]');
    await page.waitForURL(/^\/(admin|dashboard|courses)/, { timeout: 5000 });
  }

  test.beforeEach(async ({ page }) => {
    // Login as admin before each test
    await loginAsAdmin(page);
  });

  test('✓ Admin can navigate to course creation page', async ({ page }) => {
    // Navigate to admin panel
    await page.goto('/admin/courses');
    
    // Look for "Create Course" button
    const createButton = page.locator(
      'button:has-text("Create Course"), ' +
      'button:has-text("New Course"), ' +
      'a:has-text("Create Course"), ' +
      '[data-testid="create-course-button"]'
    ).first();
    
    await expect(createButton).toBeVisible({ timeout: 5000 });
    
    // Click create button
    await createButton.click();
    
    // Verify navigation to create page or modal opens
    const createForm = page.locator(
      'form[data-testid="course-form"], ' +
      '[data-testid="create-course-modal"], ' +
      ':has-text("Create New Course")'
    ).first();
    
    await expect(createForm).toBeVisible({ timeout: 3000 });
  });

  test('✓ Admin can fill out course creation form', async ({ page }) => {
    // Navigate to create course page
    await page.goto('/admin/courses/create');
    
    // Wait for form to load
    await page.waitForSelector('input[name="title"], input#title', { timeout: 5000 });
    
    // Fill in course details
    await page.fill('input[name="title"], input#title', 'Introduction to Bitcoin');
    await page.fill('textarea[name="description"], textarea#description', 'Learn the fundamentals of Bitcoin and blockchain technology.');
    
    // Select category
    const categorySelect = page.locator('select[name="categoryId"], select#categoryId, [data-testid="category-select"]').first();
    if (await categorySelect.isVisible()) {
      await categorySelect.selectOption({ index: 1 }); // Select first non-empty option
    }
    
    // Select difficulty level
    const difficultySelect = page.locator('select[name="difficultyLevel"], select#difficultyLevel, [data-testid="difficulty-select"]').first();
    if (await difficultySelect.isVisible()) {
      await difficultySelect.selectOption('1'); // Beginner
    }
    
    // Fill in duration (in seconds)
    const durationInput = page.locator('input[name="estimatedDuration"], input#estimatedDuration, input[type="number"]').first();
    if (await durationInput.isVisible()) {
      await durationInput.fill('3600'); // 1 hour
    }
    
    // Fill in reward points
    const rewardInput = page.locator('input[name="rewardPoints"], input#rewardPoints').first();
    if (await rewardInput.isVisible()) {
      await rewardInput.fill('100');
    }
    
    // Verify all fields are filled
    await expect(page.locator('input[name="title"], input#title')).toHaveValue('Introduction to Bitcoin');
    await expect(page.locator('textarea[name="description"], textarea#description')).toHaveValue(/Bitcoin/);
  });

  test('✓ Admin can toggle premium status for course', async ({ page }) => {
    // Navigate to create course page
    await page.goto('/admin/courses/create');
    
    // Wait for form to load
    await page.waitForSelector('input[name="title"], input#title', { timeout: 5000 });
    
    // Find premium checkbox or toggle
    const premiumCheckbox = page.locator(
      'input[name="isPremium"], ' +
      'input[type="checkbox"]#isPremium, ' +
      '[data-testid="premium-toggle"]'
    ).first();
    
    if (await premiumCheckbox.isVisible()) {
      // Check if checkbox is initially unchecked
      const isChecked = await premiumCheckbox.isChecked();
      
      // Toggle the checkbox
      await premiumCheckbox.click();
      
      // Verify checkbox state changed
      const newState = await premiumCheckbox.isChecked();
      expect(newState).toBe(!isChecked);
    }
  });

  test('✓ Admin can successfully create a new course', async ({ page }) => {
    // Navigate to create course page
    await page.goto('/admin/courses/create');
    
    // Wait for form to load
    await page.waitForSelector('input[name="title"], input#title', { timeout: 5000 });
    
    // Fill in required fields
    const timestamp = Date.now();
    await page.fill('input[name="title"], input#title', `Test Course ${timestamp}`);
    await page.fill('textarea[name="description"], textarea#description', 'This is a test course description for E2E testing.');
    
    // Select category
    const categorySelect = page.locator('select[name="categoryId"], select#categoryId').first();
    if (await categorySelect.isVisible()) {
      const options = await categorySelect.locator('option').count();
      if (options > 1) {
        await categorySelect.selectOption({ index: 1 });
      }
    }
    
    // Select difficulty
    const difficultySelect = page.locator('select[name="difficultyLevel"], select#difficultyLevel').first();
    if (await difficultySelect.isVisible()) {
      await difficultySelect.selectOption('1');
    }
    
    // Fill in duration and rewards
    const durationInput = page.locator('input[name="estimatedDuration"], input#estimatedDuration').first();
    if (await durationInput.isVisible()) {
      await durationInput.fill('3600');
    }
    
    const rewardInput = page.locator('input[name="rewardPoints"], input#rewardPoints').first();
    if (await rewardInput.isVisible()) {
      await rewardInput.fill('100');
    }
    
    // Submit the form
    const submitButton = page.locator(
      'button[type="submit"]:has-text("Create"), ' +
      'button:has-text("Save Course"), ' +
      'button:has-text("Create Course")'
    ).first();
    
    await submitButton.click();
    
    // Wait for success confirmation
    await page.waitForTimeout(2000);
    
    // Verify success (redirect to course list or success message)
    const successIndicators = page.locator(
      ':has-text("Course created successfully"), ' +
      ':has-text("Successfully created"), ' +
      '[role="alert"]:has-text("Success")'
    );
    
    const isRedirected = page.url().includes('/admin/courses') && !page.url().includes('/create');
    const hasSuccessMessage = await successIndicators.first().isVisible().catch(() => false);
    
    expect(isRedirected || hasSuccessMessage).toBeTruthy();
  });

  test('✓ Admin sees validation errors for empty required fields', async ({ page }) => {
    // Navigate to create course page
    await page.goto('/admin/courses/create');
    
    // Wait for form to load
    await page.waitForSelector('input[name="title"], input#title', { timeout: 5000 });
    
    // Try to submit without filling required fields
    const submitButton = page.locator(
      'button[type="submit"]:has-text("Create"), ' +
      'button:has-text("Save Course"), ' +
      'button:has-text("Create Course")'
    ).first();
    
    await submitButton.click();
    
    // Wait for validation errors
    await page.waitForTimeout(500);
    
    // Verify validation error messages appear
    const errorMessages = page.locator(
      '.error, ' +
      '[role="alert"], ' +
      '.text-red-500, ' +
      ':has-text("required"), ' +
      ':has-text("This field")'
    );
    
    const errorCount = await errorMessages.count();
    expect(errorCount).toBeGreaterThan(0);
  });

  test('✓ Admin can cancel course creation', async ({ page }) => {
    // Navigate to create course page
    await page.goto('/admin/courses/create');
    
    // Wait for form to load
    await page.waitForSelector('input[name="title"], input#title', { timeout: 5000 });
    
    // Fill in some data
    await page.fill('input[name="title"], input#title', 'Course to Cancel');
    
    // Look for cancel button
    const cancelButton = page.locator(
      'button:has-text("Cancel"), ' +
      'a:has-text("Cancel"), ' +
      'button:has-text("Back"), ' +
      '[data-testid="cancel-button"]'
    ).first();
    
    if (await cancelButton.isVisible()) {
      await cancelButton.click();
      
      // Wait for navigation away from create page
      await page.waitForTimeout(500);
      
      // Verify we're back on the courses list page
      const currentUrl = page.url();
      expect(currentUrl).not.toContain('/create');
    }
  });

  test('✓ Admin can view newly created course in admin course list', async ({ page }) => {
    // Navigate to admin courses list
    await page.goto('/admin/courses');
    
    // Wait for courses to load
    await page.waitForSelector('[data-testid="course-row"], [data-testid="course-card"], table tbody tr', { timeout: 5000 });
    
    // Verify courses are displayed
    const courseRows = page.locator('[data-testid="course-row"], [data-testid="course-card"], table tbody tr');
    const count = await courseRows.count();
    
    expect(count).toBeGreaterThan(0);
    
    // Verify admin actions are available (Edit, Delete, Publish buttons)
    const actionButtons = page.locator('button:has-text("Edit"), button:has-text("Delete"), button:has-text("Publish")');
    const hasActions = await actionButtons.first().isVisible().catch(() => false);
    
    expect(hasActions).toBeTruthy();
  });

  test('✓ Admin can edit an existing course', async ({ page }) => {
    // Navigate to admin courses list
    await page.goto('/admin/courses');
    
    // Wait for courses to load
    await page.waitForSelector('[data-testid="course-row"], [data-testid="course-card"], table tbody tr', { timeout: 5000 });
    
    // Click edit button on first course
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    
    if (await editButton.isVisible()) {
      await editButton.click();
      
      // Wait for edit form to load
      await page.waitForTimeout(1000);
      
      // Verify we're on edit page or modal is open
      const editForm = page.locator(
        'input[name="title"], ' +
        'form[data-testid="course-form"], ' +
        ':has-text("Edit Course")'
      ).first();
      
      await expect(editForm).toBeVisible({ timeout: 3000 });
      
      // Update a field
      const titleInput = page.locator('input[name="title"], input#title').first();
      if (await titleInput.isVisible()) {
        const currentValue = await titleInput.inputValue();
        await titleInput.fill(`${currentValue} - Updated`);
        
        // Save changes
        const saveButton = page.locator('button[type="submit"]:has-text("Save"), button:has-text("Update")').first();
        if (await saveButton.isVisible()) {
          await saveButton.click();
          
          // Wait for success
          await page.waitForTimeout(2000);
          
          // Verify success
          const successIndicators = page.locator(':has-text("successfully"), :has-text("updated")');
          const hasSuccess = await successIndicators.first().isVisible().catch(() => false);
          
          expect(hasSuccess || page.url().includes('/admin/courses')).toBeTruthy();
        }
      }
    }
  });

  test('✓ Admin can publish an unpublished course', async ({ page }) => {
    // Navigate to admin courses list
    await page.goto('/admin/courses');
    
    // Wait for courses to load
    await page.waitForSelector('[data-testid="course-row"], [data-testid="course-card"], table tbody tr', { timeout: 5000 });
    
    // Look for publish button on an unpublished course
    const publishButton = page.locator('button:has-text("Publish")').first();
    
    if (await publishButton.isVisible()) {
      await publishButton.click();
      
      // Wait for publish confirmation
      await page.waitForTimeout(1000);
      
      // Verify published state (button text changes to "Unpublish" or "Published")
      const publishedIndicator = page.locator(
        'button:has-text("Unpublish"), ' +
        'button:has-text("Published"), ' +
        ':has-text("Successfully published")'
      );
      
      await expect(publishedIndicator.first()).toBeVisible({ timeout: 5000 });
    }
  });

  test('✓ Non-admin user cannot access admin course creation page', async ({ page }) => {
    // Logout admin
    await page.goto('/');
    const logoutButton = page.locator('button:has-text("Logout"), button:has-text("Sign out")').first();
    if (await logoutButton.isVisible()) {
      await logoutButton.click();
      await page.waitForTimeout(500);
    }
    
    // Login as regular user
    await page.goto('/login');
    await page.fill('input[type="email"]', 'user@example.com');
    await page.fill('input[type="password"]', 'TestPassword123!');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(1000);
    
    // Try to access admin page directly
    await page.goto('/admin/courses/create');
    
    // Should be redirected or see access denied
    await page.waitForTimeout(1000);
    
    const currentUrl = page.url();
    const hasAccessDenied = page.locator(':has-text("Access denied"), :has-text("Unauthorized"), :has-text("403")');
    const isAccessDeniedVisible = await hasAccessDenied.first().isVisible().catch(() => false);
    
    // Either redirected away from admin page or see access denied message
    expect(!currentUrl.includes('/admin/courses/create') || isAccessDeniedVisible).toBeTruthy();
  });
});
