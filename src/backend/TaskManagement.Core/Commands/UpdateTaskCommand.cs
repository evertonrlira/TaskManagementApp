using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Commands;

public record UpdateTaskCommand(Guid TaskId, string Title, string? Description) : ICommand;

public record UpdateTaskResult(
	Guid Id,
	string Title,
	string? Description,
	DateTime CreatedAt,
	DateTime? CompletedAt);