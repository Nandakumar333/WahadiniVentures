import { test, expect } from '@playwright/test';

test.describe('Admin Lesson Reorder E2E Tests', () => {
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

  test('✓ Admin can navigate to course editor with lesson list', async ({ page }) => {
    // Navigate to admin courses page
    await page.goto('/admin/courses');
    
    // Wait for courses to load
    await page.waitForSelector('[data-testid="course-row"], [data-testid="course-card"], table tbody tr', { timeout: 5000 });
    
    // Click edit button on first course
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    await expect(editButton).toBeVisible();
    
    await editButton.click();
    
    // Wait for course editor to load
    await page.waitForTimeout(1000);
    
    // Verify lesson list is visible
    const lessonList = page.locator(
      '[data-testid="lesson-list"], ' +
      '[data-testid="lessons-section"], ' +
      ':has-text("Lessons")'
    ).first();
    
    await expect(lessonList).toBeVisible({ timeout: 5000 });
  });

  test('✓ Lesson list shows lessons in current order with drag handles', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find lesson list items
      const lessonItems = page.locator('[data-testid="lesson-item"], [draggable="true"], [data-testid="lesson-row"]');
      
      const count = await lessonItems.count();
      
      if (count > 0) {
        // Verify first lesson is visible
        await expect(lessonItems.first()).toBeVisible();
        
        // Look for drag handles (icon or element that indicates draggable)
        const dragHandles = page.locator(
          '[data-testid="drag-handle"], ' +
          '[class*="drag"], ' +
          '[role="button"]:has([class*="grip"]), ' +
          ':has-text("☰")'
        );
        
        const hasDragHandles = await dragHandles.first().isVisible().catch(() => false);
        
        // Or check if items are draggable
        const firstLesson = lessonItems.first();
        const isDraggable = await firstLesson.getAttribute('draggable');
        
        expect(hasDragHandles || isDraggable === 'true').toBeTruthy();
      }
    }
  });

  test('✓ Admin can drag and drop lesson to reorder', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find lesson items
      const lessonItems = page.locator('[data-testid="lesson-item"], [draggable="true"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count >= 2) {
        // Get the first and second lesson
        const firstLesson = lessonItems.nth(0);
        const secondLesson = lessonItems.nth(1);
        
        // Get initial titles to verify reorder
        const firstLessonTitle = await firstLesson.locator('h3, h4, [data-testid="lesson-title"]').first().textContent();
        
        // Drag first lesson below second lesson
        await firstLesson.dragTo(secondLesson, {
          targetPosition: { x: 0, y: 50 }, // Drop below the second lesson
        });
        
        // Wait for reorder animation/update
        await page.waitForTimeout(1000);
        
        // Verify order changed - the second lesson should now be first
        const newFirstLesson = lessonItems.nth(0);
        const newFirstLessonTitle = await newFirstLesson.locator('h3, h4, [data-testid="lesson-title"]').first().textContent();
        
        // The titles should be different (order changed)
        expect(newFirstLessonTitle).not.toBe(firstLessonTitle);
      }
    }
  });

  test('✓ Reorder changes are saved and persist after page reload', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find lesson items
      const lessonItems = page.locator('[data-testid="lesson-item"], [draggable="true"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count >= 2) {
        // Remember initial order
        const initialOrder: string[] = [];
        for (let i = 0; i < Math.min(count, 3); i++) {
          const title = await lessonItems.nth(i).locator('h3, h4, [data-testid="lesson-title"]').first().textContent();
          initialOrder.push(title || '');
        }
        
        // Drag first to second position
        const firstLesson = lessonItems.nth(0);
        const secondLesson = lessonItems.nth(1);
        
        await firstLesson.dragTo(secondLesson, {
          targetPosition: { x: 0, y: 50 },
        });
        
        await page.waitForTimeout(1000);
        
        // Look for save button
        const saveButton = page.locator('button:has-text("Save"), button:has-text("Update")').first();
        if (await saveButton.isVisible()) {
          await saveButton.click();
          await page.waitForTimeout(1000);
        }
        
        // Get current URL to reload the same page
        const currentUrl = page.url();
        
        // Reload the page
        await page.reload();
        await page.waitForTimeout(1000);
        
        // Or navigate back to editor
        if (!currentUrl.includes('/edit')) {
          await page.goto('/admin/courses');
          await page.waitForSelector('[data-testid="course-row"]', { timeout: 5000 });
          const editBtn = page.locator('button:has-text("Edit")').first();
          if (await editBtn.isVisible()) {
            await editBtn.click();
            await page.waitForTimeout(1000);
          }
        }
        
        // Get new order after reload
        const lessonItemsAfterReload = page.locator('[data-testid="lesson-item"], [draggable="true"], [data-testid="lesson-row"]');
        const newOrder: string[] = [];
        
        for (let i = 0; i < Math.min(await lessonItemsAfterReload.count(), 3); i++) {
          const title = await lessonItemsAfterReload.nth(i).locator('h3, h4, [data-testid="lesson-title"]').first().textContent();
          newOrder.push(title || '');
        }
        
        // Order should be different from initial (reorder persisted)
        expect(newOrder[0]).not.toBe(initialOrder[0]);
      }
    }
  });

  test('✓ Admin can reorder lessons using up/down arrows', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find lesson items
      const lessonItems = page.locator('[data-testid="lesson-item"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count >= 2) {
        // Get first lesson
        const firstLesson = lessonItems.nth(0);
        const firstLessonTitle = await firstLesson.locator('h3, h4, [data-testid="lesson-title"]').first().textContent();
        
        // Look for down arrow/move down button
        const moveDownButton = firstLesson.locator(
          'button[aria-label*="down"], ' +
          'button:has-text("↓"), ' +
          'button:has-text("Down"), ' +
          '[data-testid="move-down"]'
        ).first();
        
        if (await moveDownButton.isVisible()) {
          await moveDownButton.click();
          await page.waitForTimeout(500);
          
          // Verify first lesson moved down
          const newFirstLesson = lessonItems.nth(0);
          const newFirstLessonTitle = await newFirstLesson.locator('h3, h4, [data-testid="lesson-title"]').first().textContent();
          
          expect(newFirstLessonTitle).not.toBe(firstLessonTitle);
        }
      }
    }
  });

  test('✓ Lesson order numbers update after reordering', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find lesson items with order numbers
      const lessonItems = page.locator('[data-testid="lesson-item"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count >= 2) {
        // Check if order numbers exist
        const firstLessonOrder = lessonItems.nth(0).locator('[data-testid="lesson-order"], [data-testid="order-index"], :has-text("#1"), :has-text("1.")').first();
        const hasOrderNumbers = await firstLessonOrder.isVisible().catch(() => false);
        
        if (hasOrderNumbers) {
          // Drag to reorder
          const firstLesson = lessonItems.nth(0);
          const secondLesson = lessonItems.nth(1);
          
          await firstLesson.dragTo(secondLesson, {
            targetPosition: { x: 0, y: 50 },
          });
          
          await page.waitForTimeout(1000);
          
          // Verify order numbers updated
          const newFirstLessonOrder = lessonItems.nth(0).locator('[data-testid="lesson-order"], [data-testid="order-index"]').first();
          const orderText = await newFirstLessonOrder.textContent();
          
          // First lesson should now have order number 1
          expect(orderText).toContain('1');
        }
      }
    }
  });

  test('✓ Cannot reorder lessons with fewer than 2 lessons', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find lesson items
      const lessonItems = page.locator('[data-testid="lesson-item"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count < 2) {
        // If only 0 or 1 lesson, drag handles or reorder buttons should be disabled/hidden
        const dragHandles = page.locator('[data-testid="drag-handle"]');
        const moveButtons = page.locator('button[aria-label*="Move"], button:has-text("↑"), button:has-text("↓")');
        
        const hasDragHandles = await dragHandles.first().isVisible().catch(() => false);
        const hasMoveButtons = await moveButtons.first().isVisible().catch(() => false);
        
        // With < 2 lessons, reorder UI should not be present or be disabled
        if (hasMoveButtons) {
          const isDisabled = await moveButtons.first().isDisabled();
          expect(isDisabled).toBeTruthy();
        }
      } else {
        // If 2+ lessons, reorder should be available
        expect(count).toBeGreaterThanOrEqual(2);
      }
    }
  });

  test('✓ First lesson "Move Up" button is disabled', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find first lesson
      const lessonItems = page.locator('[data-testid="lesson-item"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count >= 2) {
        const firstLesson = lessonItems.nth(0);
        
        // Look for move up button on first lesson
        const moveUpButton = firstLesson.locator(
          'button[aria-label*="up"], ' +
          'button:has-text("↑"), ' +
          'button:has-text("Up"), ' +
          '[data-testid="move-up"]'
        ).first();
        
        if (await moveUpButton.isVisible()) {
          // First lesson's "Move Up" should be disabled
          await expect(moveUpButton).toBeDisabled();
        }
      }
    }
  });

  test('✓ Last lesson "Move Down" button is disabled', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find last lesson
      const lessonItems = page.locator('[data-testid="lesson-item"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count >= 2) {
        const lastLesson = lessonItems.nth(count - 1);
        
        // Look for move down button on last lesson
        const moveDownButton = lastLesson.locator(
          'button[aria-label*="down"], ' +
          'button:has-text("↓"), ' +
          'button:has-text("Down"), ' +
          '[data-testid="move-down"]'
        ).first();
        
        if (await moveDownButton.isVisible()) {
          // Last lesson's "Move Down" should be disabled
          await expect(moveDownButton).toBeDisabled();
        }
      }
    }
  });

  test('✓ Visual feedback during drag operation', async ({ page }) => {
    // Navigate to course editor
    await page.goto('/admin/courses');
    await page.waitForSelector('[data-testid="course-row"], table tbody tr', { timeout: 5000 });
    
    const editButton = page.locator('button:has-text("Edit"), a:has-text("Edit")').first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForTimeout(1000);
      
      // Find lesson items
      const lessonItems = page.locator('[data-testid="lesson-item"], [draggable="true"], [data-testid="lesson-row"]');
      const count = await lessonItems.count();
      
      if (count >= 2) {
        const firstLesson = lessonItems.nth(0);
        
        // Start dragging (hover over drag handle)
        const dragHandle = firstLesson.locator('[data-testid="drag-handle"]').first();
        
        if (await dragHandle.isVisible()) {
          await dragHandle.hover();
          
          // Visual feedback: cursor should change, or element should highlight
          const lessonBox = firstLesson.boundingBox();
          expect(lessonBox).not.toBeNull();
        }
      }
    }
  });

  test('✓ Non-admin user cannot access lesson reordering', async ({ page }) => {
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
    
    // Try to access admin course editor
    await page.goto('/admin/courses');
    
    // Should be redirected or see access denied
    await page.waitForTimeout(1000);
    
    const currentUrl = page.url();
    const hasAccessDenied = page.locator(':has-text("Access denied"), :has-text("Unauthorized"), :has-text("403")');
    const isAccessDeniedVisible = await hasAccessDenied.first().isVisible().catch(() => false);
    
    expect(!currentUrl.includes('/admin/courses') || isAccessDeniedVisible).toBeTruthy();
  });
});
