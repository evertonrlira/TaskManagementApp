#!/bin/bash
# E2E Test Environment Setup Script for Unix/Linux/macOS

echo "🔧 Installing E2E test dependencies..."
cd tests/e2e
npm install

echo "🎭 Installing Playwright browsers..."
npx playwright install

echo "📦 Installing system dependencies..."
npx playwright install-deps

# Create .env file if it doesn't exist
if [ ! -f ".env" ]; then
    echo "📄 Creating .env file..."
    cat > .env << EOL
# E2E Test Configuration
BASE_URL=http://localhost:5173
API_URL=http://localhost:5000
TEST_TIMEOUT=30000
HEADLESS=true
EOL
fi

# Verify installation
echo "✅ Verifying Playwright installation..."
npx playwright --version

echo "🎉 E2E test setup complete!"
echo "To run tests:"
echo "  npm test                # Run all tests"
echo "  npm run test:headed     # Run with browser visible"
echo "  npm run test:ui         # Run with UI mode"
