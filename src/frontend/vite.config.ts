import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => ({
  plugins: [react()],
  build: {
    // Enable source maps only in development
    sourcemap: mode === 'development',
    // Optimize bundle
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          ui: ['@radix-ui/react-icons', '@radix-ui/react-switch'],
          query: ['@tanstack/react-query'],
        }
      }
    },
    // Security: Minify in production
    minify: mode === 'production' ? 'terser' : false,
  },
  server: {
    // Configure dev server
    port: 5173,
    open: true,
    // Security headers for dev server
    headers: {
      'X-Content-Type-Options': 'nosniff',
      'X-Frame-Options': 'DENY',
      'X-XSS-Protection': '1; mode=block',
    },
  },
}))
