using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Commands;

public record CreateTaskCommand(Guid UserId, string Title, string? Description) : ICommand;

public record CreateTaskResult(
	Guid Id,
	string Title,
	string? Description,
	UserTaskStatus Status,
	DateTime CreatedAt,
	DateTime? CompletedAt);
