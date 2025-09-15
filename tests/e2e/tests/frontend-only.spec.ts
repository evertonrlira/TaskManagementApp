import { test, expect } from '@playwright/test';

test.describe('Frontend Components - Data TestId Verification', () => {
  test('should find all components with data-testid attributes', async ({ page }) => {
    // Navigate to the frontend (assuming it's running on port 5173)
    await page.goto('http://localhost:5173');

    // Check if the main page loads (correct title is "TaskPuma")
    await expect(page).toHaveTitle(/TaskPuma/);

    // Check for theme toggle
    const themeToggle = page.getByTestId('theme-toggle');
    await expect(themeToggle).toBeVisible();

    // Select a user first to see task form elements
    const userSelect = page.getByTestId('user-select');
    await expect(userSelect).toBeVisible();
    
    const userOptions = await userSelect.locator('option').allTextContents();
    const firstUser = userOptions.find(option => option !== 'Select User');
    if (firstUser) {
      await userSelect.selectOption(firstUser);
      await page.waitForTimeout(1500);
    }

    // Check for task form elements (now they should be visible)
    const titleInput = page.getByTestId('task-title-input');
    await expect(titleInput).toBeVisible();

    const descriptionInput = page.getByTestId('task-description-input');
    await expect(descriptionInput).toBeVisible();

    const addButton = page.getByTestId('add-task-button');
    await expect(addButton).toBeVisible();

    // Check for task list container
    const taskList = page.getByTestId('task-list');
    await expect(taskList).toBeVisible();

    console.log('✅ All frontend components with data-testid found successfully!');
  });

  test('should be able to interact with theme toggle', async ({ page }) => {
    await page.goto('http://localhost:5173');
    
    const themeToggle = page.getByTestId('theme-toggle');
    await expect(themeToggle).toBeVisible();
    
    // Click the theme toggle
    await themeToggle.click();
    
    console.log('✅ Theme toggle interaction successful!');
  });

  test('should be able to type in task form', async ({ page }) => {
    await page.goto('http://localhost:5173');
    
    // Select a user first to access task form
    const userSelect = page.getByTestId('user-select');
    await expect(userSelect).toBeVisible();
    
    const userOptions = await userSelect.locator('option').allTextContents();
    const firstUser = userOptions.find(option => option !== 'Select User');
    if (firstUser) {
      await userSelect.selectOption(firstUser);
      await page.waitForTimeout(1500);
    }
    
    const titleInput = page.getByTestId('task-title-input');
    const descriptionInput = page.getByTestId('task-description-input');
    
    // Type in the form fields
    await titleInput.fill('Test Task Title');
    await descriptionInput.fill('Test Task Description');
    
    // Verify the values
    await expect(titleInput).toHaveValue('Test Task Title');
    await expect(descriptionInput).toHaveValue('Test Task Description');
    
    console.log('✅ Task form interaction successful!');
  });
});
