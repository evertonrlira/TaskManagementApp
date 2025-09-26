# E2E Test Environment Setup Script for Windows

# Install dependencies
Write-Host "🔧 Installing E2E test dependencies..." -ForegroundColor Green
Set-Location "tests\e2e"
npm install

# Install Playwright browsers
Write-Host "🎭 Installing Playwright browsers..." -ForegroundColor Green
npx playwright install

# Install system dependencies for browsers
Write-Host "📦 Installing system dependencies..." -ForegroundColor Green
npx playwright install-deps

# Create .env file if it doesn't exist
if (-not (Test-Path ".env")) {
    Write-Host "📄 Creating .env file..." -ForegroundColor Green
    @"
# E2E Test Configuration
BASE_URL=http://localhost:5173
API_URL=http://localhost:5000
TEST_TIMEOUT=30000
HEADLESS=true
"@ | Out-File -FilePath ".env" -Encoding UTF8
}

# Verify installation
Write-Host "✅ Verifying Playwright installation..." -ForegroundColor Green
npx playwright --version

Write-Host "🎉 E2E test setup complete!" -ForegroundColor Green
Write-Host "To run tests:" -ForegroundColor Yellow
Write-Host "  npm test                # Run all tests" -ForegroundColor Cyan
Write-Host "  npm run test:headed     # Run with browser visible" -ForegroundColor Cyan
Write-Host "  npm run test:ui         # Run with UI mode" -ForegroundColor Cyan
