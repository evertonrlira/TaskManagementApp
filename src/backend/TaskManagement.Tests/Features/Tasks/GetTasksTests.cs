using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManagement.Tests.Common;

namespace TaskManagement.Tests.Features.Tasks;

public class GetTasksTests : IntegrationTestBase
{
	public GetTasksTests(WebApplicationFactory<Program> factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task GetTasks_ShouldReturnEmptyList_WhenNoTasksExist()
	{
		// Use a user ID that doesn't have seeded tasks
		var emptyUserId = EmptyUserId;
		
		// Act
		var response = await _client.GetAsync($"/api/tasks?UserId={emptyUserId}");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		var itemsElement = content.GetProperty("items");
		itemsElement.GetArrayLength().ShouldBe(0);
	}

	[Fact]
	public async Task GetTasks_ShouldReturnTasks_WhenTasksExist()
	{
		// Arrange - Create a test task to ensure we have data to retrieve
		var userId = Guid.NewGuid();
		await CreateTestTask("Test Task for GetTasks", "Test Description", userId);
		
		// Act
		var response = await _client.GetAsync($"/api/tasks?UserId={userId}");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		var items = content.GetProperty("items");
		items.GetArrayLength().ShouldBeGreaterThan(0);
		var firstTask = items.EnumerateArray().First();
		firstTask.GetProperty("id").GetString().ShouldNotBeNullOrEmpty();
		firstTask.GetProperty("title").GetString().ShouldBe("Test Task for GetTasks");
	}

	[Fact]
	public async Task GetTasks_ShouldReturnDefaultPagination_WhenNoPaginationParametersProvided()
	{
		// Arrange - Create test tasks
		var userId = Guid.NewGuid();
		await CreateTestTask("Task 1", "Description 1", userId);
		await CreateTestTask("Task 2", "Description 2", userId);
		
		// Act - Request without pagination parameters (should use defaults: page=1, pageSize=10)
		var response = await _client.GetAsync($"/api/tasks?UserId={userId}");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		// Verify pagination metadata
		content.GetProperty("pageNumber").GetInt32().ShouldBe(1);
		content.GetProperty("pageSize").GetInt32().ShouldBe(10);
		content.GetProperty("totalCount").GetInt32().ShouldBe(2);
		content.GetProperty("totalPages").GetInt32().ShouldBe(1);
		content.GetProperty("hasPreviousPage").GetBoolean().ShouldBe(false);
		content.GetProperty("hasNextPage").GetBoolean().ShouldBe(false);
		
		// Verify items
		var items = content.GetProperty("items");
		items.GetArrayLength().ShouldBe(2);
	}

	[Fact]
	public async Task GetTasks_ShouldReturnFirstPage_WhenRequestingFirstPageWithCustomPageSize()
	{
		// Arrange - Create 5 test tasks
		var userId = Guid.NewGuid();
		for (int i = 1; i <= 5; i++)
		{
			await CreateTestTask($"Task {i}", $"Description {i}", userId);
		}
		
		// Act - Request first page with page size of 3
		var response = await _client.GetAsync($"/api/tasks?UserId={userId}&PageNumber=1&PageSize=3");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		// Verify pagination metadata
		content.GetProperty("pageNumber").GetInt32().ShouldBe(1);
		content.GetProperty("pageSize").GetInt32().ShouldBe(3);
		content.GetProperty("totalCount").GetInt32().ShouldBe(5);
		content.GetProperty("totalPages").GetInt32().ShouldBe(2);
		content.GetProperty("hasPreviousPage").GetBoolean().ShouldBe(false);
		content.GetProperty("hasNextPage").GetBoolean().ShouldBe(true);
		
		// Verify exactly 3 items returned
		var items = content.GetProperty("items");
		items.GetArrayLength().ShouldBe(3);
	}

	[Fact]
	public async Task GetTasks_ShouldReturnSecondPage_WhenRequestingSecondPageWithCustomPageSize()
	{
		// Arrange - Create 5 test tasks
		var userId = Guid.NewGuid();
		for (int i = 1; i <= 5; i++)
		{
			await CreateTestTask($"Task {i}", $"Description {i}", userId);
		}
		
		// Act - Request second page with page size of 3
		var response = await _client.GetAsync($"/api/tasks?UserId={userId}&PageNumber=2&PageSize=3");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		// Verify pagination metadata
		content.GetProperty("pageNumber").GetInt32().ShouldBe(2);
		content.GetProperty("pageSize").GetInt32().ShouldBe(3);
		content.GetProperty("totalCount").GetInt32().ShouldBe(5);
		content.GetProperty("totalPages").GetInt32().ShouldBe(2);
		content.GetProperty("hasPreviousPage").GetBoolean().ShouldBe(true);
		content.GetProperty("hasNextPage").GetBoolean().ShouldBe(false);
		
		// Verify exactly 2 items returned (remaining items)
		var items = content.GetProperty("items");
		items.GetArrayLength().ShouldBe(2);
	}

	[Fact]
	public async Task GetTasks_ShouldReturnEmptyItemsList_WhenRequestingPageBeyondAvailableData()
	{
		// Arrange - Create 2 test tasks
		var userId = Guid.NewGuid();
		await CreateTestTask("Task 1", "Description 1", userId);
		await CreateTestTask("Task 2", "Description 2", userId);
		
		// Act - Request page 3 when only 1 page exists
		var response = await _client.GetAsync($"/api/tasks?UserId={userId}&PageNumber=3&PageSize=10");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		// Verify pagination metadata - should clamp to valid page
		content.GetProperty("pageNumber").GetInt32().ShouldBe(1); // Should be clamped to page 1
		content.GetProperty("pageSize").GetInt32().ShouldBe(10);
		content.GetProperty("totalCount").GetInt32().ShouldBe(2);
		content.GetProperty("totalPages").GetInt32().ShouldBe(1);
		content.GetProperty("hasPreviousPage").GetBoolean().ShouldBe(false);
		content.GetProperty("hasNextPage").GetBoolean().ShouldBe(false);
		
		// Should return all available items (clamped to valid page)
		var items = content.GetProperty("items");
		items.GetArrayLength().ShouldBe(2);
	}

	[Fact]
	public async Task GetTasks_ShouldHandleZeroPageNumber_ByClampingToPageOne()
	{
		// Arrange - Create test tasks
		var userId = Guid.NewGuid();
		await CreateTestTask("Task 1", "Description 1", userId);
		
		// Act - Request page 0 (invalid)
		var response = await _client.GetAsync($"/api/tasks?UserId={userId}&PageNumber=0&PageSize=10");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		// Should be clamped to page 1
		content.GetProperty("pageNumber").GetInt32().ShouldBe(1);
		content.GetProperty("totalCount").GetInt32().ShouldBe(1);
		
		var items = content.GetProperty("items");
		items.GetArrayLength().ShouldBe(1);
	}

	[Fact]
	public async Task GetTasks_ShouldHandleNegativePageNumber_ByClampingToPageOne()
	{
		// Arrange - Create test tasks
		var userId = Guid.NewGuid();
		await CreateTestTask("Task 1", "Description 1", userId);
		
		// Act - Request negative page number
		var response = await _client.GetAsync($"/api/tasks?UserId={userId}&PageNumber=-1&PageSize=10");
		var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		
		// Should be clamped to page 1
		content.GetProperty("pageNumber").GetInt32().ShouldBe(1);
		content.GetProperty("totalCount").GetInt32().ShouldBe(1);
		
		var items = content.GetProperty("items");
		items.GetArrayLength().ShouldBe(1);
	}

	[Fact]
	public async Task GetTasks_ShouldReturnCorrectTotalPages_WithVariousPageSizes()
	{
		// Arrange - Create 7 test tasks
		var userId = Guid.NewGuid();
		for (int i = 1; i <= 7; i++)
		{
			await CreateTestTask($"Task {i}", $"Description {i}", userId);
		}
		
		// Act & Assert - Test different page sizes
		
		// PageSize 3: 7 items = 3 pages (3,3,1)
		var response3 = await _client.GetAsync($"/api/tasks?UserId={userId}&PageSize=3");
		var content3 = await response3.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		content3.GetProperty("totalPages").GetInt32().ShouldBe(3);
		content3.GetProperty("totalCount").GetInt32().ShouldBe(7);
		
		// PageSize 5: 7 items = 2 pages (5,2)
		var response5 = await _client.GetAsync($"/api/tasks?UserId={userId}&PageSize=5");
		var content5 = await response5.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		content5.GetProperty("totalPages").GetInt32().ShouldBe(2);
		content5.GetProperty("totalCount").GetInt32().ShouldBe(7);
		
		// PageSize 10: 7 items = 1 page
		var response10 = await _client.GetAsync($"/api/tasks?UserId={userId}&PageSize=10");
		var content10 = await response10.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		content10.GetProperty("totalPages").GetInt32().ShouldBe(1);
		content10.GetProperty("totalCount").GetInt32().ShouldBe(7);
	}

	[Fact]
	public async Task GetTasks_ShouldMaintainCorrectOrderingAcrossPages()
	{
		// Arrange - Create tasks with specific order (incomplete tasks first, then completed by completion time)
		var userId = Guid.NewGuid();
		
		// Create incomplete tasks first
		var task1 = await CreateTestTask("Incomplete Task 1", "Description 1", userId);
		var task2 = await CreateTestTask("Incomplete Task 2", "Description 2", userId);
		
		// Create completed tasks
		var task3 = await CreateTestTask("Completed Task 1", "Description 3", userId);
		var task4 = await CreateTestTask("Completed Task 2", "Description 4", userId);
		
		// Complete some tasks (this will set CompletedAt)
		await _client.PatchAsync($"/api/tasks/{task3.GetProperty("id").GetString()}/toggleCompletion", null);
		await Task.Delay(10); // Small delay to ensure different completion times
		await _client.PatchAsync($"/api/tasks/{task4.GetProperty("id").GetString()}/toggleCompletion", null);
		
		// Act - Get first page with page size 2
		var response1 = await _client.GetAsync($"/api/tasks?UserId={userId}&PageNumber=1&PageSize=2");
		var content1 = await response1.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
		
		// Act - Get second page with page size 2
		var response2 = await _client.GetAsync($"/api/tasks?UserId={userId}&PageNumber=2&PageSize=2");
		var content2 = await response2.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

		// Assert - First page should contain incomplete tasks (ordered by creation date desc)
		response1.StatusCode.ShouldBe(HttpStatusCode.OK);
		var page1Items = content1.GetProperty("items");
		page1Items.GetArrayLength().ShouldBe(2);
		
		var page1Task1 = page1Items.EnumerateArray().First();
		var page1Task2 = page1Items.EnumerateArray().Skip(1).First();
		
		// Both should be incomplete (no completedAt)
		page1Task1.TryGetProperty("completedAt", out var completedAt1).ShouldBe(true);
		completedAt1.ValueKind.ShouldBe(JsonValueKind.Null);
		
		page1Task2.TryGetProperty("completedAt", out var completedAt2).ShouldBe(true);
		completedAt2.ValueKind.ShouldBe(JsonValueKind.Null);
		
		// Assert - Second page should contain completed tasks
		response2.StatusCode.ShouldBe(HttpStatusCode.OK);
		var page2Items = content2.GetProperty("items");
		page2Items.GetArrayLength().ShouldBe(2);
		
		var page2Task1 = page2Items.EnumerateArray().First();
		var page2Task2 = page2Items.EnumerateArray().Skip(1).First();
		
		// Both should be completed (have completedAt)
		page2Task1.TryGetProperty("completedAt", out var completedAt3).ShouldBe(true);
		completedAt3.ValueKind.ShouldNotBe(JsonValueKind.Null);
		
		page2Task2.TryGetProperty("completedAt", out var completedAt4).ShouldBe(true);
		completedAt4.ValueKind.ShouldNotBe(JsonValueKind.Null);
	}
}
