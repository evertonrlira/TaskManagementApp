using Microsoft.Extensions.Options;
using TaskManagement.Api.Configuration;

namespace TaskManagement.Api.Infrastructure.Configuration;

public class SecurityConfigurationValidator : IValidateOptions<SecurityConfiguration>
{
    public ValidateOptionsResult Validate(string? name, SecurityConfiguration options)
    {
        var failures = new List<string>();

        // Validate CORS origins
        if (options.Cors.AllowedOrigins == null || !options.Cors.AllowedOrigins.Any())
        {
            failures.Add("At least one CORS origin must be configured");
        }
        else
        {
            foreach (var origin in options.Cors.AllowedOrigins)
            {
                if (string.IsNullOrWhiteSpace(origin))
                {
                    failures.Add("CORS origins cannot be null or empty");
                }
                else if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    failures.Add($"Invalid CORS origin format: {origin}");
                }
            }
        }

        // Validate allowed headers
        if (options.Cors.AllowedHeaders == null || !options.Cors.AllowedHeaders.Any())
        {
            failures.Add("At least one CORS header must be configured");
        }

        // Validate allowed methods
        if (options.Cors.AllowedMethods == null || !options.Cors.AllowedMethods.Any())
        {
            failures.Add("At least one CORS method must be configured");
        }

        // Validate request limits
        if (options.MaxRequestBodySize <= 0)
        {
            failures.Add("MaxRequestBodySize must be greater than 0");
        }

        if (options.RequestTimeout <= 0)
        {
            failures.Add("RequestTimeout must be greater than 0");
        }

        return failures.Any() 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
