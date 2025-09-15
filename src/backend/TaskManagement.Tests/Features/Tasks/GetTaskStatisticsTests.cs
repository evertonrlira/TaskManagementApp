using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManagement.Tests.Common;

namespace TaskManagement.Tests.Features.Tasks;

public class GetTaskStatisticsTests : IntegrationTestBase
{
	private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

	public GetTaskStatisticsTests(WebApplicationFactory<Program> factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task GetTaskStatistics_ShouldReturnZeros_WhenUserHasNoTasks()
	{
		// Use a user ID that doesn't have seeded tasks (but is valid)
		var emptyUserId = Guid.NewGuid();
		
		// Act
		var response = await _client.GetAsync($"/api/tasks/statistics?UserId={emptyUserId}");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		content.GetProperty("pendingTasks").GetInt32().ShouldBe(0);
		content.GetProperty("completedTasks").GetInt32().ShouldBe(0);
		content.GetProperty("totalTasks").GetInt32().ShouldBe(0);
	}

	[Fact]
	public async Task GetTaskStatistics_ShouldReturnCorrectCounts_WhenUserHasMixedTasks()
	{
		// Arrange - Use the first seeded user who has mixed tasks
		var userId = TestUserId;

		// Create a few test tasks with known completion states
		await CreateTestTaskAsync(userId, "Pending Task 1", false);
		await CreateTestTaskAsync(userId, "Pending Task 2", false);
		await CreateTestTaskAsync(userId, "Completed Task 1", true);

		// Act
		var response = await _client.GetAsync($"/api/tasks/statistics?UserId={userId}");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		var pendingTasks = content.GetProperty("pendingTasks").GetInt32();
		var completedTasks = content.GetProperty("completedTasks").GetInt32();
		var totalTasks = content.GetProperty("totalTasks").GetInt32();

		// Should include our test tasks plus any seeded tasks
		pendingTasks.ShouldBeGreaterThanOrEqualTo(2); // At least our 2 pending tasks
		completedTasks.ShouldBeGreaterThanOrEqualTo(1); // At least our 1 completed task
		totalTasks.ShouldBe(pendingTasks + completedTasks); // Total should equal sum
	}

	[Fact]
	public async Task GetTaskStatistics_ShouldReturnBadRequest_WhenUserIdIsMissing()
	{
		// Act
		var response = await _client.GetAsync("/api/tasks/statistics");

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task GetTaskStatistics_ShouldReturnBadRequest_WhenUserIdIsInvalid()
	{
		// Act
		var response = await _client.GetAsync("/api/tasks/statistics?UserId=invalid-guid");

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task GetTaskStatistics_ShouldUpdateCorrectly_WhenTaskCompletionChanges()
	{
		// Arrange - Use a unique user to avoid interference from other tests
		var userId = Guid.NewGuid();

		// Create a pending task
		var taskId = await CreateTestTaskAsync(userId, "Test Task", false);

		// Get initial statistics (should be 1 pending, 0 completed)
		var initialResponse = await _client.GetAsync($"/api/tasks/statistics?UserId={userId}");
		var initialContent = await initialResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var initialPending = initialContent.GetProperty("pendingTasks").GetInt32();
		var initialCompleted = initialContent.GetProperty("completedTasks").GetInt32();

		// Verify expected initial state
		initialPending.ShouldBe(1);
		initialCompleted.ShouldBe(0);

		// Toggle task completion
		await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);

		// Get updated statistics (should be 0 pending, 1 completed)
		var updatedResponse = await _client.GetAsync($"/api/tasks/statistics?UserId={userId}");
		var updatedContent = await updatedResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var updatedPending = updatedContent.GetProperty("pendingTasks").GetInt32();
		var updatedCompleted = updatedContent.GetProperty("completedTasks").GetInt32();

		// Assert final state
		updatedPending.ShouldBe(0);
		updatedCompleted.ShouldBe(1);
	}

	[Fact]
	public async Task GetTaskStatistics_ShouldUpdateCorrectly_WhenTaskIsDeleted()
	{
		// Arrange - Use a unique user to avoid interference from other tests
		var userId = Guid.NewGuid();
		
		// Create multiple tasks for this user to ensure we have something to delete
		var taskId1 = await CreateTestTaskAsync(userId, "Task to Keep 1", false);
		var taskId2 = await CreateTestTaskAsync(userId, "Task to Keep 2", true);
		var taskIdToDelete = await CreateTestTaskAsync(userId, "Task to Delete", false);

		// Get initial statistics
		var initialResponse = await _client.GetAsync($"/api/tasks/statistics?UserId={userId}");
		var initialContent = await initialResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var initialTotal = initialContent.GetProperty("totalTasks").GetInt32();
		var initialPending = initialContent.GetProperty("pendingTasks").GetInt32();
		var initialCompleted = initialContent.GetProperty("completedTasks").GetInt32();

		// Verify we have the expected initial state (3 tasks: 2 pending, 1 completed)
		initialTotal.ShouldBe(3);
		initialPending.ShouldBe(2);
		initialCompleted.ShouldBe(1);

		// Delete the task
		await _client.DeleteAsync($"/api/tasks/{taskIdToDelete}");

		// Get updated statistics
		var updatedResponse = await _client.GetAsync($"/api/tasks/statistics?UserId={userId}");
		var updatedContent = await updatedResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var updatedTotal = updatedContent.GetProperty("totalTasks").GetInt32();
		var updatedPending = updatedContent.GetProperty("pendingTasks").GetInt32();
		var updatedCompleted = updatedContent.GetProperty("completedTasks").GetInt32();

		// Assert - Should have 1 less total and 1 less pending
		updatedTotal.ShouldBe(2);
		updatedPending.ShouldBe(1);
		updatedCompleted.ShouldBe(1); // Completed count should remain the same
	}

	[Fact]
	public async Task GetTaskStatistics_ShouldReturnBadRequest_WhenUserIdIsEmpty()
	{
		// Arrange
		var emptyUserId = Guid.Empty;
		
		// Act
		var response = await _client.GetAsync($"/api/tasks/statistics?UserId={emptyUserId}");

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
	}

	private async Task<string> CreateTestTaskAsync(Guid userId, string title, bool completed)
	{
		var createRequest = new
		{
			UserId = userId,
			Title = title,
			Description = $"Test description for {title}"
		};

		var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
		var createContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		var taskId = createContent.GetProperty("id").GetString();

		// If task should be completed, toggle it
		if (completed)
		{
			await _client.PatchAsync($"/api/tasks/{taskId}/toggleCompletion", null);
		}

		return taskId!;
	}
}