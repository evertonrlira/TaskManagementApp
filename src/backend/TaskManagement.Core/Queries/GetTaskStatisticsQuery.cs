using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Queries;

public record GetTaskStatisticsQuery(Guid UserId) : IQuery;

public record GetTaskStatisticsResult(
	int PendingTasks,
	int CompletedTasks,
	int TotalTasks);