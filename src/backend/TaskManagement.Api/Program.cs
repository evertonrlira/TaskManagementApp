
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Configuration;
using TaskManagement.Api.Extensions;
using TaskManagement.Api.Middleware;
using TaskManagement.Core.Extensions;
using TaskManagement.Infrastructure.Extensions;
using TaskManagement.Infrastructure.Persistence;

public class Program
{
	public static void Main(string[] args)
	{
		// Auto-create appsettings.json from example if it doesn't exist
		EnsureAppSettingsExists();

		var builder = WebApplication.CreateBuilder(args);

		// Configure security settings
		builder.Services.AddApiSecurity(builder.Configuration);

		// Add services to the container.
		builder.Services.AddControllers(options =>
		{
			// Limit request body size (default 30MB is too much for API)
			options.ModelBinderProviders.Insert(0, new Microsoft.AspNetCore.Mvc.ModelBinding.Binders.SimpleTypeModelBinderProvider());
		});

		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		// Add Health Checks
		builder.Services.AddHealthChecks();

		// Add Infrastructure layer (DB, repositories, etc.)
		builder.Services.AddInfrastructure();

		// Add Core layer (handlers, validators, etc.)
		builder.Services.AddCore();

		var app = builder.Build();

		// Global exception handling middleware (must be first)
		app.UseMiddleware<GlobalExceptionMiddleware>();

		// Security headers middleware
		app.UseMiddleware<SecurityHeadersMiddleware>();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		// Apply CORS middleware - this needs to be before other middleware like UseAuthorization
		app.UseCors();

		app.UseAuthorization();

		// Add Health Check endpoint
		app.MapHealthChecks("/api/health");

		app.MapControllers();

		// Ensure database is created and seeded
		app.Services.EnsureDatabaseCreated();

		app.Run();
	}

	private static void EnsureAppSettingsExists()
	{
		var appSettingsPath = "appsettings.json";
		var examplePath = "appsettings.example.json";

		if (!File.Exists(appSettingsPath) && File.Exists(examplePath))
		{
			File.Copy(examplePath, appSettingsPath);
			Console.WriteLine($"Created {appSettingsPath} from {examplePath}");
		}
	}
}
