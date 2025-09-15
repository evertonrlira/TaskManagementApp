namespace TaskManagement.Api.Configuration;

/// <summary>
/// Configuration model for security settings
/// </summary>
public class SecurityConfiguration
{
	public const string SectionName = "Security";

	public long MaxRequestBodySize { get; set; } = 1048576; // 1MB
	public int RequestTimeout { get; set; } = 30;
	public bool DetailedErrors { get; set; } = false;
	public CorsConfiguration Cors { get; set; } = new();
}

/// <summary>
/// Configuration model for CORS settings
/// </summary>
public class CorsConfiguration
{
	public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
	public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
	public string[] AllowedMethods { get; set; } = Array.Empty<string>();
	public bool AllowCredentials { get; set; } = false;
}
