namespace TaskManagement.Api.Middleware;

/// <summary>
/// Middleware to add security headers to API responses
/// </summary>
public class SecurityHeadersMiddleware
{
	private readonly RequestDelegate _next;

	public SecurityHeadersMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// Add modern security headers
		context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
		context.Response.Headers.Append("X-Frame-Options", "DENY");
		context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
		context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
		context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");

		// Remove server information
		context.Response.Headers.Remove("Server");
		context.Response.Headers.Remove("X-Powered-By");
		context.Response.Headers.Remove("X-AspNet-Version");
		context.Response.Headers.Remove("X-AspNetMvc-Version");

		await _next(context);
	}
}
