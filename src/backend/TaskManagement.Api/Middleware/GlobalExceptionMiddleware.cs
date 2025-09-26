using FluentValidation;
using System.Text.Json;

namespace TaskManagement.Api.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
public class GlobalExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionMiddleware> _logger;
	private readonly IWebHostEnvironment _environment;

	public GlobalExceptionMiddleware(
		RequestDelegate next,
		ILogger<GlobalExceptionMiddleware> logger,
		IWebHostEnvironment environment)
	{
		_next = next;
		_logger = logger;
		_environment = environment;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception exception)
		{
			await HandleExceptionAsync(context, exception);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		_logger.LogError(exception, "An unhandled exception occurred");

		var (statusCode, message, details) = GetErrorDetails(exception);

		var response = new
		{
			Type = GetErrorType(exception),
			Title = GetErrorTitle(statusCode),
			Status = statusCode,
			Detail = message,
			Instance = context.Request.Path,
			Timestamp = DateTime.UtcNow,
			TechnicalDetails = _environment.IsDevelopment() ? details : null
		};

		context.Response.StatusCode = statusCode;
		context.Response.ContentType = "application/json";

		var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});

		await context.Response.WriteAsync(jsonResponse);
	}

	private (int statusCode, string message, object? details) GetErrorDetails(Exception exception)
	{
		return exception switch
		{
			ValidationException validationEx => (
				400,
				"One or more validation errors occurred.",
				validationEx.Errors.GroupBy(x => x.PropertyName)
					.ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
			),

			KeyNotFoundException => (
				404,
				"The requested resource was not found.",
				null
			),

			ArgumentException argumentEx => (
				400,
				argumentEx.Message,
				null
			),

			UnauthorizedAccessException => (
				401,
				"Access denied. Authentication required.",
				null
			),

			TimeoutException => (
				408,
				"The request timed out. Please try again.",
				null
			),

			_ => (
				500,
				_environment.IsDevelopment()
					? exception.Message
					: "An unexpected error occurred. Please try again later.",
				_environment.IsDevelopment() ? exception.StackTrace : null
			)
		};
	}

	private static string GetErrorType(Exception exception)
	{
		return exception switch
		{
			ValidationException => "ValidationError",
			KeyNotFoundException => "NotFoundError",
			ArgumentException => "ValidationError",
			UnauthorizedAccessException => "AuthenticationError",
			TimeoutException => "TimeoutError",
			_ => "InternalServerError"
		};
	}

	private static string GetErrorTitle(int statusCode)
	{
		return statusCode switch
		{
			400 => "Bad Request",
			401 => "Unauthorized",
			403 => "Forbidden",
			404 => "Not Found",
			408 => "Request Timeout",
			409 => "Conflict",
			422 => "Unprocessable Entity",
			500 => "Internal Server Error",
			_ => "Error"
		};
	}
}
