/**
 * Environment configuration and validation utilities
 * Provides runtime validation and type-safe access to environment variables
 */

const requiredEnvVars = ['VITE_API_URL'] as const;
const optionalEnvVars = [
  'VITE_APP_NAME',
  'VITE_APP_VERSION',
  'VITE_ENABLE_LOGGING',
  'VITE_ENABLE_DEVTOOLS',
  'VITE_ENABLE_ANALYTICS',
  'VITE_BUILD_SOURCEMAP'
] as const;

type RequiredEnvVar = typeof requiredEnvVars[number];
type OptionalEnvVar = typeof optionalEnvVars[number];
type EnvVar = RequiredEnvVar | OptionalEnvVar;

/**
 * Type-safe environment configuration
 */
export interface AppConfig {
  // API Configuration
  apiUrl: string;
  
  // App Information
  appName: string;
  appVersion: string;
  
  // Feature Flags
  enableLogging: boolean;
  enableDevtools: boolean;
  enableAnalytics: boolean;
  
  // Build Configuration
  isDevelopment: boolean;
  isProduction: boolean;
  buildSourcemap: boolean;
}

/**
 * Safely gets an environment variable with validation
 */
function getEnvVar(name: EnvVar, defaultValue?: string): string {
  const value = import.meta.env[name];
  
  if (value === undefined || value === '') {
    if (requiredEnvVars.includes(name as RequiredEnvVar)) {
      throw new Error(
        `Required environment variable ${name} is not defined. ` +
        `Please check your .env.local file or environment configuration.`
      );
    }
    return defaultValue || '';
  }
  
  return value;
}

/**
 * Converts string environment variable to boolean
 */
function getBooleanEnvVar(name: EnvVar, defaultValue = false): boolean {
  const value = getEnvVar(name, defaultValue.toString());
  return value.toLowerCase() === 'true';
}

/**
 * Validates all required environment variables are present
 */
function validateEnvironment(): void {
  const missingVars: string[] = [];
  
  for (const varName of requiredEnvVars) {
    try {
      getEnvVar(varName);
    } catch {
      missingVars.push(varName);
    }
  }
  
  if (missingVars.length > 0) {
    throw new Error(
      `Missing required environment variables: ${missingVars.join(', ')}. ` +
      `Please check your .env.local file.`
    );
  }
}

/**
 * Creates and validates the application configuration
 */
function createAppConfig(): AppConfig {
  validateEnvironment();
  
  const mode = import.meta.env.MODE;
  const isDevelopment = mode === 'development';
  const isProduction = mode === 'production';
  
  return {
    // API Configuration
    apiUrl: getEnvVar('VITE_API_URL'),
    
    // App Information
    appName: getEnvVar('VITE_APP_NAME', 'Task Management App'),
    appVersion: getEnvVar('VITE_APP_VERSION', '1.0.0'),
    
    // Feature Flags (default to secure values in production)
    enableLogging: getBooleanEnvVar('VITE_ENABLE_LOGGING', isDevelopment),
    enableDevtools: getBooleanEnvVar('VITE_ENABLE_DEVTOOLS', isDevelopment),
    enableAnalytics: getBooleanEnvVar('VITE_ENABLE_ANALYTICS', false),
    
    // Build Configuration
    isDevelopment,
    isProduction,
    buildSourcemap: getBooleanEnvVar('VITE_BUILD_SOURCEMAP', isDevelopment),
  };
}

/**
 * Application configuration singleton
 * Validates environment variables on first access
 */
export const appConfig: AppConfig = createAppConfig();

/**
 * Development-only utilities
 */
export const devUtils = {
  /**
   * Logs configuration in development mode only
   */
  logConfig(): void {
    if (appConfig.isDevelopment && appConfig.enableLogging) {
      console.info('🔧 App Configuration:', {
        mode: import.meta.env.MODE,
        apiUrl: appConfig.apiUrl,
        appName: appConfig.appName,
        appVersion: appConfig.appVersion,
        enableLogging: appConfig.enableLogging,
        enableDevtools: appConfig.enableDevtools,
        enableAnalytics: appConfig.enableAnalytics,
      });
    }
  },
  
  /**
   * Validates environment in development
   */
  validateDevelopmentConfig(): void {
    if (appConfig.isDevelopment) {
      if (appConfig.buildSourcemap && appConfig.isProduction) {
        console.warn('⚠️  Source maps are enabled in production mode');
      }
      
      if (!appConfig.apiUrl.startsWith('http')) {
        console.warn('⚠️  API URL should include protocol (http/https)');
      }
    }
  }
};

// Run development validations
if (appConfig.isDevelopment) {
  devUtils.logConfig();
  devUtils.validateDevelopmentConfig();
}
