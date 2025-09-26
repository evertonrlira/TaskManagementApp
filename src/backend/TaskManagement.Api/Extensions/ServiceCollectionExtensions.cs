using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TaskManagement.Api.Configuration;
using TaskManagement.Api.Infrastructure.Configuration;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Extensions;

/// <summary>
/// Extension methods for registering API layer services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers API security and CORS configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApiSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure security settings
        var securityConfig = configuration.GetSection(SecurityConfiguration.SectionName)
            .Get<SecurityConfiguration>() ?? new SecurityConfiguration();
        services.Configure<SecurityConfiguration>(
            configuration.GetSection(SecurityConfiguration.SectionName));

        // Configure request size limits
        services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = securityConfig.MaxRequestBodySize;
        });

        // Add CORS with configuration-based settings
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var corsConfig = securityConfig.Cors;

                // Validate that we have allowed origins configured
                if (corsConfig.AllowedOrigins?.Length > 0)
                {
                    policy.WithOrigins(corsConfig.AllowedOrigins);
                }
                else
                {
                    throw new InvalidOperationException(
                        "CORS AllowedOrigins must be configured in appsettings.json under Security:Cors:AllowedOrigins");
                }

                // Configure headers
                if (corsConfig.AllowedHeaders?.Length > 0)
                {
                    if (corsConfig.AllowedHeaders.Contains("*"))
                    {
                        policy.AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithHeaders(corsConfig.AllowedHeaders);
                    }
                }

                // Configure methods
                if (corsConfig.AllowedMethods?.Length > 0)
                {
                    if (corsConfig.AllowedMethods.Contains("*"))
                    {
                        policy.AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithMethods(corsConfig.AllowedMethods);
                    }
                }

                // Configure credentials
                if (corsConfig.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        // Register configuration validation
        services.AddSingleton<IValidateOptions<SecurityConfiguration>, SecurityConfigurationValidator>();

        return services;
    }
}
