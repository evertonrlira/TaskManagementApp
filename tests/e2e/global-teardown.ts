import { FullConfig } from '@playwright/test';

async function globalTeardown(config: FullConfig) {
  console.log('🧹 Starting global teardown for E2E tests...');
  
  // Clean up any test data, logs, or other resources
  console.log('🗑️ Cleaning up test data...');
  
  // Optional: Clean database, reset state, etc.
  
  console.log('✅ Global teardown complete');
}

export default globalTeardown;
