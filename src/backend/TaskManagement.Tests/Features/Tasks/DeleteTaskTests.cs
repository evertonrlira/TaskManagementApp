using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManagement.Tests.Common;

namespace TaskManagement.Tests.Features.Tasks;

public class DeleteTaskTests : IntegrationTestBase
{
	public DeleteTaskTests(WebApplicationFactory<Program> factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task DeleteTask_ShouldDeleteTask_WhenTaskExists()
	{
		// Arrange - Create a new task
		var newTask = await CreateTestTask();
		var taskId = newTask.GetProperty("id").GetString();

		// Act - Delete the task
		var response = await _client.DeleteAsync($"/api/tasks/{taskId}");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		content.GetProperty("id").GetString().ShouldBe(taskId);

		// Verify task is no longer accessible (soft deleted)
		var getResponse = await _client.GetAsync($"/api/tasks?UserId=c7d3d975-9b97-4c4f-9e0a-041e31fd60f5");
		var getContent = await getResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var items = getContent.GetProperty("items");
		
		// The deleted task should not appear in the results
		var taskExists = false;
		foreach (var item in items.EnumerateArray())
		{
			if (item.GetProperty("id").GetString() == taskId)
			{
				taskExists = true;
				break;
			}
		}
		taskExists.ShouldBeFalse("Deleted task should not appear in GetTasks results");
	}

	[Fact]
	public async Task DeleteTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
	{
		// Arrange
		var nonExistentTaskId = Guid.NewGuid();

		// Act
		var response = await _client.DeleteAsync($"/api/tasks/{nonExistentTaskId}");

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task DeleteTask_ShouldReturnBadRequest_WhenTaskIdIsInvalid()
	{
		// Arrange
		var invalidTaskId = "invalid-guid";

		// Act
		var response = await _client.DeleteAsync($"/api/tasks/{invalidTaskId}");

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task DeleteTask_ShouldReturnNotFound_WhenTaskAlreadyDeleted()
	{
		// Arrange - Create and delete a task
		var newTask = await CreateTestTask();
		var taskId = newTask.GetProperty("id").GetString();
		
		// Delete the task first time
		var firstDeleteResponse = await _client.DeleteAsync($"/api/tasks/{taskId}");
		firstDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

		// Act - Try to delete the same task again
		var response = await _client.DeleteAsync($"/api/tasks/{taskId}");

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task DeleteTask_ShouldWorkForBothTodoAndCompletedTasks()
	{
		// Arrange - Create two tasks, complete one
		var todoTask = await CreateTestTask();
		var completedTask = await CreateTestTask();
		
		var todoTaskId = todoTask.GetProperty("id").GetString();
		var completedTaskId = completedTask.GetProperty("id").GetString();
		
		// Complete one task
		await _client.PatchAsync($"/api/tasks/{completedTaskId}/toggleCompletion", null);

		// Act - Delete both tasks
		var response1 = await _client.DeleteAsync($"/api/tasks/{todoTaskId}");
		var response2 = await _client.DeleteAsync($"/api/tasks/{completedTaskId}");

		// Assert
		response1.StatusCode.ShouldBe(HttpStatusCode.OK);
		response2.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content1 = await response1.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var content2 = await response2.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		content1.GetProperty("id").GetString().ShouldBe(todoTaskId);
		content2.GetProperty("id").GetString().ShouldBe(completedTaskId);
	}
}
