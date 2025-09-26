using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskManagement.Tests.Common;

/// <summary>
/// Base class for integration tests that provides common functionality for API testing
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
	protected readonly WebApplicationFactory<Program> _factory;
	protected readonly HttpClient _client;
	protected readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	/// <summary>
	/// Empty user ID for testing scenarios with no tasks
	/// </summary>
	protected static readonly Guid EmptyUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");

	protected IntegrationTestBase(WebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = _factory.CreateClient();
	}

	/// <summary>
	/// Creates a test task using the API and returns the created task as JsonElement
	/// </summary>
	/// <param name="title">Optional custom title for the task</param>
	/// <param name="description">Optional custom description for the task</param>
	/// <param name="userId">Optional user ID, defaults to SeededUserId</param>
	/// <returns>JsonElement representing the created task</returns>
	protected async Task<JsonElement> CreateTestTask(string? title = null, string? description = null, Guid? userId = null)
	{
		var request = new
		{
			UserId = userId ?? Guid.NewGuid(),
			Title = title ?? $"Test Task {Guid.NewGuid()}",
			Description = description ?? "Test Description"
		};

		var response = await _client.PostAsJsonAsync("/api/tasks", request);
		response.StatusCode.ShouldBe(HttpStatusCode.Created);
		
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		return content;
	}
}
