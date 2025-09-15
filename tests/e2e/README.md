# Task Management E2E Tests

This directory contains end-to-end tests for the Task Management application using Playwright.

## Setup

1. **Install dependencies:**
   ```bash
   cd tests/e2e
   npm install
   npm run setup
   ```

2. **Install Playwright browsers:**
   ```bash
   npm run install
   ```

## Running Tests

### Basic Commands

```bash
# Run all tests
npm test

# Run tests in headed mode (see browser)
npm run test:headed

# Run tests with UI mode
npm run test:ui

# Run tests in debug mode
npm run test:debug

# Show test report
npm run test:report

# Run with trace recording
npm run test:trace
```

### Specific Test Files

```bash
# Run specific test file
npx playwright test task-crud.spec.ts

# Run specific test by name
npx playwright test --grep "should create a new task"

# Run tests for specific browser
npx playwright test --project=chromium
```

## Test Structure

### Page Objects (`/pages`)
- `TaskManagementPage.ts` - Main page object for task management interface
- Contains reusable methods for interacting with UI elements

### Test Files (`/tests`)
- `task-crud.spec.ts` - Basic CRUD operations (Create, Read, Update, Delete)
- `pagination.spec.ts` - Pagination functionality
- `ui-interactions.spec.ts` - User interface interactions and responsiveness
- `api-integration.spec.ts` - API integration and error handling

### Utilities (`/utils`)
- `test-data.ts` - Test data generators and utilities

## Test Categories

### 🔧 CRUD Operations
- Task creation with title and description
- Task completion toggling
- Task deletion
- Form validation
- Empty state handling

### 📄 Pagination
- Navigation between pages
- Correct task counts per page
- Pagination controls state
- Data consistency across pages

### 🎨 UI/UX
- Theme toggling (light/dark)
- User switching
- Responsive design (mobile/desktop)
- Keyboard navigation
- Accessibility features

### 🌐 API Integration
- Success response handling
- Error response handling
- Network timeouts
- Request retries
- Data integrity validation

## Configuration

The tests are configured to:
- Run against multiple browsers (Chrome, Firefox, Safari, Edge)
- Test on both desktop and mobile viewports
- Automatically start backend API (port 5000) and frontend (port 5173)
- Generate HTML reports with screenshots and videos on failure
- Record traces for debugging

## Environment Setup

### Prerequisites
- Backend API running on `http://localhost:5000`
- Frontend application running on `http://localhost:5173`
- Both services will be started automatically by Playwright

### Environment Variables
Set these in your CI/CD pipeline:
- `CI=true` - Enables CI-specific configurations
- `ASPNETCORE_ENVIRONMENT=Testing` - Sets backend to testing mode

## Best Practices

### 1. Page Object Model
Use page objects to encapsulate UI interactions:
```typescript
const taskPage = new TaskManagementPage(page);
await taskPage.createTask('My Task', 'Description');
```

### 2. Self-Contained Tests
Each test should:
- Create its own test data
- Clean up after itself
- Not depend on other tests

### 3. Reliable Selectors
Use stable selectors:
- `data-testid` attributes (preferred)
- Role-based selectors
- Text content selectors
- Avoid CSS class names

### 4. Error Handling
Tests should verify both success and error scenarios:
- Network failures
- API errors
- Validation errors
- Timeout handling

### 5. Assertions
Use meaningful assertions:
```typescript
// Good
await expect(taskPage.taskList).toContainText('My Task');

// Better
const taskTitles = await taskPage.getVisibleTaskTitles();
expect(taskTitles).toContain('My Task');
```

## Debugging

### Visual Debugging
```bash
# Run in headed mode
npm run test:headed

# Use UI mode for interactive debugging
npm run test:ui

# Debug specific test
npm run test:debug -- task-crud.spec.ts
```

### Trace Analysis
```bash
# Record traces
npm run test:trace

# View traces in Playwright trace viewer
npx playwright show-trace trace.zip
```

### Screenshots and Videos
Failed tests automatically capture:
- Screenshots at the point of failure
- Videos of the entire test execution
- Network logs and console messages

## CI/CD Integration

### GitHub Actions Example
```yaml
name: E2E Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: |
          cd tests/e2e
          npm ci
          npx playwright install --with-deps
      
      - name: Run E2E tests
        run: |
          cd tests/e2e
          npm test
      
      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: playwright-report
          path: tests/e2e/playwright-report/
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 5000 and 5173 are available
2. **Browser installation**: Run `npm run install` to install browsers
3. **Timeout errors**: Increase timeouts in `playwright.config.ts`
4. **Flaky tests**: Add proper wait conditions and stabilize selectors

### Performance Tips

1. Use `page.waitForLoadState('networkidle')` for dynamic content
2. Prefer `locator.waitFor()` over `page.waitForTimeout()`
3. Run tests in parallel where possible
4. Use `--project` flag to test specific browsers during development

## Contributing

When adding new tests:
1. Follow the existing page object pattern
2. Add appropriate test data generators
3. Include both positive and negative test cases
4. Update this README if adding new test categories
