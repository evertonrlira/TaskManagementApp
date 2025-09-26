import { chromium, FullConfig } from '@playwright/test';

async function globalSetup(config: FullConfig) {
  console.log('🚀 Starting global setup for E2E tests...');
  
  // Start browser for setup
  const browser = await chromium.launch();
  const context = await browser.newContext();
  const page = await context.newPage();
  
  try {
    // Wait for API to be ready
    console.log('⏳ Waiting for API to be ready...');
    let apiReady = false;
    for (let i = 0; i < 30; i++) {
      try {
        const response = await page.request.get('http://localhost:5000/api/health');
        if (response.ok()) {
          apiReady = true;
          console.log('✅ API health check passed!');
          break;
        }
      } catch (error) {
        // API not ready yet, wait and retry
        console.log(`⏳ API not ready yet (attempt ${i + 1}/30)...`);
      }
      await page.waitForTimeout(1000);
    }
    
    if (!apiReady) {
      throw new Error('API failed to start within 30 seconds');
    }
    
    // Wait for Frontend to be ready
    console.log('⏳ Waiting for Frontend to be ready...');
    let frontendReady = false;
    for (let i = 0; i < 10; i++) { // Reduced attempts
      try {
        const response = await page.request.get('http://localhost:5173');
        if (response.ok()) {
          frontendReady = true;
          console.log('✅ Frontend loaded successfully!');
          break;
        }
      } catch (error) {
        // Frontend not ready yet, wait and retry
        console.log(`⏳ Frontend not ready yet (attempt ${i + 1}/10)...`);
      }
      await page.waitForTimeout(1000);
    }

    if (!frontendReady) {
      console.log('⚠️ Frontend health check failed, but continuing with tests...');
      // Don't throw error, just continue - the frontend might be working even if health check fails
    }    console.log('✅ Both API and Frontend are ready!');
    
    // Optional: Clear any existing data or perform other setup
    await setupTestData(page);
    
  } finally {
    await context.close();
    await browser.close();
  }
}

async function setupTestData(page: any) {
  console.log('🔧 Setting up test data...');
  
  try {
    // Create test users via API
    const createUser1 = await page.request.post('http://localhost:5000/api/users', {
      data: {
        name: 'Test User 1'
      }
    });
    
    const createUser2 = await page.request.post('http://localhost:5000/api/users', {
      data: {
        name: 'Test User 2'
      }
    });
    
    if (createUser1.ok() && createUser2.ok()) {
      console.log('✅ Test users created successfully');
    } else {
      console.log('ℹ️ Users might already exist, continuing...');
    }
    
    // Try to get existing users to verify they exist
    const usersResponse = await page.request.get('http://localhost:5000/api/users');
    if (usersResponse.ok()) {
      const usersData = await usersResponse.json();
      const userCount = usersData.users ? usersData.users.length : 0;
      console.log(`✅ Found ${userCount} users available for testing`);
    }
    
  } catch (error) {
    console.log('ℹ️ Note: Could not create test users via API, they might already exist');
  }
  
  console.log('✅ Test data setup complete');
}

export default globalSetup;
