using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Queries;

namespace TaskManagement.Core.Handlers;

public class GetTasksHandler : IQueryHandler<GetTasksQuery, GetTasksResult>
{
	private readonly ITaskDbContext _context;
	private readonly IPaginationService _paginationService;

	public GetTasksHandler(ITaskDbContext context, IPaginationService paginationService)
	{
		_context = context;
		_paginationService = paginationService;
	}

	public async Task<GetTasksResult> HandleAsync(GetTasksQuery query)
	{
		var tasksQuery = _context.Tasks
			.Where(t => t.UserId == query.UserId && t.DeletedAt == null)
			.OrderBy(t => t.CompletedAt.HasValue)
			.ThenByDescending(t => t.CompletedAt.HasValue
				? t.CompletedAt
				: t.CreatedAt)
			.Select(t => new TaskDto(
				t.Id,
				t.UserId,
				t.Title,
				t.Description,
				t.CreatedAt,
				t.CompletedAt
			));

		var paginatedTasks = await _paginationService.CreateAsync(
			tasksQuery,
			query.PageNumber,
			query.PageSize);

		return new GetTasksResult(
			paginatedTasks.Items,
			paginatedTasks.PageNumber,
			paginatedTasks.PageSize,
			paginatedTasks.TotalCount,
			paginatedTasks.TotalPages,
			paginatedTasks.HasPreviousPage,
			paginatedTasks.HasNextPage);
	}
}
