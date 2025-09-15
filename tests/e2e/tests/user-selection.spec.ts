import { test, expect } from '@playwright/test';

test.describe('User Selection Flow', () => {
  test('should be able to select a user and see task interface', async ({ page }) => {
    // Navigate to the application
    await page.goto('/');

    // Check if page loads
    await expect(page).toHaveTitle(/TaskPuma/);

    // Look for user selection dropdown
    const userSelect = page.getByTestId('user-select');
    await expect(userSelect).toBeVisible();

    // Get the available options
    const options = await userSelect.locator('option').all();
    console.log(`Found ${options.length} user options`);

    // Select the first real user (skip "Select User" option)
    if (options.length > 1) {
      const firstUserOption = await options[1].textContent();
      console.log(`Selecting user: ${firstUserOption}`);
      await userSelect.selectOption({ index: 1 });
      
      // Wait a bit for the UI to update
      await page.waitForTimeout(2000);

      // Now check if task interface becomes visible
      const taskList = page.getByTestId('task-list');
      await expect(taskList).toBeVisible({ timeout: 10000 });

      // Check if task form is visible
      const taskTitleInput = page.getByTestId('task-title-input');
      await expect(taskTitleInput).toBeVisible();

      console.log('✅ User selection and task interface are working!');
    } else {
      throw new Error('No users found in the dropdown');
    }
  });

  test('should show user selection before task interface', async ({ page }) => {
    await page.goto('/');

    // Initially, task interface should NOT be visible
    const taskList = page.getByTestId('task-list');
    await expect(taskList).not.toBeVisible();

    // But user selection should be visible
    const userSelect = page.getByTestId('user-select');
    await expect(userSelect).toBeVisible();

    console.log('✅ User selection is properly gating task interface!');
  });
});
