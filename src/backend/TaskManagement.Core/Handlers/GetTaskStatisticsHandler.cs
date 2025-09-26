using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Queries;

namespace TaskManagement.Core.Handlers;

public class GetTaskStatisticsHandler : IQueryHandler<GetTaskStatisticsQuery, GetTaskStatisticsResult>
{
	private readonly ITaskDbContext _context;

	public GetTaskStatisticsHandler(ITaskDbContext context)
	{
		_context = context;
	}

	public async Task<GetTaskStatisticsResult> HandleAsync(GetTaskStatisticsQuery query)
	{
		if (query.UserId == Guid.Empty)
		{
			throw new ArgumentException("UserId cannot be empty.", nameof(query.UserId));
		}

		var tasks = await _context.Tasks
			.Where(t => t.UserId == query.UserId && t.DeletedAt == null)
			.Select(t => new { t.CompletedAt })
			.ToListAsync();

		var totalTasks = tasks.Count;
		var completedTasks = tasks.Count(t => t.CompletedAt.HasValue);
		var pendingTasks = totalTasks - completedTasks;

		return new GetTaskStatisticsResult(
			pendingTasks,
			completedTasks,
			totalTasks
		);
	}
}