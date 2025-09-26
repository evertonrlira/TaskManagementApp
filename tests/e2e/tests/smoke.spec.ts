import { test, expect } from '@playwright/test';

test.describe('Basic Setup Verification', () => {
  test('should be able to load the application', async ({ page }) => {
    // This is a basic smoke test to verify setup
    await page.goto('http://localhost:5173');
    
    // Check if page loads (you can modify this based on your actual app)
    await expect(page).toHaveTitle(/task|management|puma/i);
    
    // Check if main container exists
    await expect(page.locator('body')).toBeVisible();
  });
});
