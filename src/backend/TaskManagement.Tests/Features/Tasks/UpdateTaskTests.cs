using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManagement.Tests.Common;

namespace TaskManagement.Tests.Features.Tasks;

public class UpdateTaskTests : IntegrationTestBase
{
	private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

	public UpdateTaskTests(WebApplicationFactory<Program> factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task UpdateTask_ShouldUpdateSuccessfully_WhenValidDataProvided()
	{
		// Arrange - Create a task first
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Original Task Title",
			Description = "Original description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Prepare update request
		var updateRequest = new
		{
			Title = "Updated Task Title",
			Description = "Updated description with new content"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);
		var updateContent = await updateResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		updateContent.GetProperty("id").GetString().ShouldBe(taskId);
		updateContent.GetProperty("title").GetString().ShouldBe("Updated Task Title");
		updateContent.GetProperty("description").GetString().ShouldBe("Updated description with new content");
		
		// CreatedAt should remain unchanged
		updateContent.TryGetProperty("createdAt", out var createdAt).ShouldBe(true);
		createdAt.ValueKind.ShouldNotBe(JsonValueKind.Null);
	}

	[Fact]
	public async Task UpdateTask_ShouldReturnBadRequest_WhenTitleIsEmpty()
	{
		// Arrange - Create a task first
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Task to Update",
			Description = "Description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Prepare invalid update request
		var updateRequest = new
		{
			Title = "",
			Description = "Valid description"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task UpdateTask_ShouldReturnBadRequest_WhenTitleIsWhitespace()
	{
		// Arrange - Create a task first
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Task to Update",
			Description = "Description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Prepare invalid update request
		var updateRequest = new
		{
			Title = "   ",
			Description = "Valid description"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task UpdateTask_ShouldReturnBadRequest_WhenTitleIsTooLong()
	{
		// Arrange - Create a task first
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Task to Update",
			Description = "Description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Prepare invalid update request with title longer than 1024 characters
		var longTitle = new string('A', 1025);
		var updateRequest = new
		{
			Title = longTitle,
			Description = "Valid description"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task UpdateTask_ShouldReturnBadRequest_WhenDescriptionIsTooLong()
	{
		// Arrange - Create a task first
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Task to Update",
			Description = "Description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Prepare invalid update request with description longer than 4096 characters
		var longDescription = new string('B', 4097);
		var updateRequest = new
		{
			Title = "Valid title",
			Description = longDescription
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task UpdateTask_ShouldReturnBadRequest_WhenTaskIdIsInvalid()
	{
		// Arrange
		var updateRequest = new
		{
			Title = "Valid Title",
			Description = "Valid description"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync("/api/tasks/invalid-guid", updateRequest);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task UpdateTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
	{
		// Arrange
		var nonExistentTaskId = Guid.NewGuid();
		var updateRequest = new
		{
			Title = "Valid Title",
			Description = "Valid description"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{nonExistentTaskId}", updateRequest);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task UpdateTask_ShouldAllowNullDescription()
	{
		// Arrange - Create a task first
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Task with Description",
			Description = "Original description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Prepare update request with null description
		var updateRequest = new
		{
			Title = "Updated Title",
			Description = (string?)null
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);
		var updateContent = await updateResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
		updateContent.GetProperty("title").GetString().ShouldBe("Updated Title");
		updateContent.GetProperty("description").ValueKind.ShouldBe(JsonValueKind.Null);
	}

	[Fact]
	public async Task UpdateTask_ShouldPreserveTaskCompletionState()
	{
		// Arrange - Create a task and toggle it to completed
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Task to Complete and Update",
			Description = "Original description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Toggle task to completed
		await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);

		// Prepare update request
		var updateRequest = new
		{
			Title = "Updated Title",
			Description = "Updated description"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);
		var updateContent = await updateResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
		updateContent.GetProperty("title").GetString().ShouldBe("Updated Title");
		updateContent.GetProperty("description").GetString().ShouldBe("Updated description");
		
		// Verify task completion state is preserved by checking if we can get it via tasks endpoint
		// (Status is not returned in update response per requirement)
		var tasksResponse = await _client.GetAsync($"/api/tasks?UserId={userId}");
		var tasksContent = await tasksResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var tasks = tasksContent.GetProperty("items").EnumerateArray();
		var updatedTask = tasks.FirstOrDefault(t => t.GetProperty("id").GetString() == taskId);
		
		updatedTask.ValueKind.ShouldNotBe(JsonValueKind.Undefined);
		updatedTask.GetProperty("title").GetString().ShouldBe("Updated Title");
		// Verify task is still completed by checking completedAt is not null
		updatedTask.TryGetProperty("completedAt", out var completedAt).ShouldBe(true);
		completedAt.ValueKind.ShouldNotBe(JsonValueKind.Null);
	}

	[Fact]
	public async Task UpdateTask_ShouldNotUpdateDeletedTask()
	{
		// Arrange - Create a task and delete it
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Task to Delete",
			Description = "Original description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Delete the task
		await _client.DeleteAsync($"/api/tasks/{taskId}");

		// Prepare update request
		var updateRequest = new
		{
			Title = "This Should Not Work",
			Description = "This should not update"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task UpdateTask_ShouldTrimWhitespaceFromTitle()
	{
		// Arrange - Create a task first
		var userId = TestUserId;
		var createRequest = new
		{
			UserId = userId,
			Title = "Original Title",
			Description = "Description"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// Prepare update request with whitespace around title
		var updateRequest = new
		{
			Title = "  Updated Title  ",
			Description = "Updated description"
		};

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);
		var updateContent = await updateResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
		updateContent.GetProperty("title").GetString().ShouldBe("Updated Title"); // Trimmed
	}
}