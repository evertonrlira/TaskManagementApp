using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Queries;

public record GetTasksQuery(
	Guid UserId,
	int PageNumber = 1,
	int PageSize = 10) : IQuery;

public record TaskDto(
	Guid Id,
	Guid UserId,
	string Title,
	string? Description,
	DateTime CreatedAt,
	DateTime? CompletedAt);

public record GetTasksResult(
	List<TaskDto> Items,
	int PageNumber,
	int PageSize,
	int TotalCount,
	int TotalPages,
	bool HasPreviousPage,
	bool HasNextPage);
