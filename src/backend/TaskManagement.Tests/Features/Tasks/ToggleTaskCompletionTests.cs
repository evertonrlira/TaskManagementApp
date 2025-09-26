using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManagement.Tests.Common;

namespace TaskManagement.Tests.Features.Tasks;

public class ToggleTaskCompletionTests : IntegrationTestBase
{
	public ToggleTaskCompletionTests(WebApplicationFactory<Program> factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task ToggleTaskCompletion_ShouldMarkAsCompleted_WhenTaskIsTodo()
	{
		// Arrange - Create a new task
		var newTask = await CreateTestTask();
		var taskId = newTask.GetProperty("id").GetString();

		// Act - Toggle completion (should mark as completed)
		var response = await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		content.GetProperty("taskId").GetString().ShouldBe(taskId);
		content.GetProperty("completedAt").ValueKind.ShouldNotBe(JsonValueKind.Null);
		
		// Verify the completion date is recent
		var completedAt = content.GetProperty("completedAt").GetDateTime();
		completedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
	}

	[Fact]
	public async Task ToggleTaskCompletion_ShouldMarkAsTodo_WhenTaskIsCompleted()
	{
		// Arrange - Create and complete a task first
		var newTask = await CreateTestTask();
		var taskId = newTask.GetProperty("id").GetString();
		
		// Complete the task first
		await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);
		
		// Act - Toggle completion again (should mark as todo)
		var response = await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		content.GetProperty("taskId").GetString().ShouldBe(taskId);
		content.GetProperty("completedAt").ValueKind.ShouldBe(JsonValueKind.Null);
	}

	[Fact]
	public async Task ToggleTaskCompletion_ShouldReturnNotFound_WhenTaskDoesNotExist()
	{
		// Arrange
		var nonExistentTaskId = Guid.NewGuid();

		// Act
		var response = await _client.PatchAsync($"/api/tasks/{nonExistentTaskId}/toggleCompletion", null);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task ToggleTaskCompletion_ShouldReturnBadRequest_WhenTaskIdIsInvalid()
	{
		// Arrange
		var invalidTaskId = "invalid-guid";

		// Act
		var response = await _client.PatchAsync($"/api/tasks/{invalidTaskId}/toggleCompletion", null);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task ToggleTaskCompletion_ShouldWorkMultipleTimes_ForSameTask()
	{
		// Arrange - Create a new task
		var newTask = await CreateTestTask();
		var taskId = newTask.GetProperty("id").GetString();

		// Act & Assert - Toggle multiple times
		for (int i = 0; i < 3; i++)
		{
			// Toggle to completed
			var response1 = await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);
			var content1 = await response1.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
			
			response1.StatusCode.ShouldBe(HttpStatusCode.OK);
			content1.GetProperty("completedAt").ValueKind.ShouldNotBe(JsonValueKind.Null);

			// Toggle back to todo
			var response2 = await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);
			var content2 = await response2.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
			
			response2.StatusCode.ShouldBe(HttpStatusCode.OK);
			content2.GetProperty("completedAt").ValueKind.ShouldBe(JsonValueKind.Null);
		}
	}
}
