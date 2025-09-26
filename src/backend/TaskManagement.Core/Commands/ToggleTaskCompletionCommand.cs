using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Commands;

public record ToggleTaskCompletionCommand(Guid TaskId) : ICommand;

public record ToggleTaskCompletionResult(
	Guid TaskId,
	DateTime? CompletedAt);
