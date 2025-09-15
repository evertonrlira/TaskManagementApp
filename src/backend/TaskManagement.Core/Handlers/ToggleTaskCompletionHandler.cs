using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Commands;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Handlers;

public class ToggleTaskCompletionHandler : ICommandHandler<ToggleTaskCompletionCommand, ToggleTaskCompletionResult>
{
	private readonly ITaskDbContext _context;

	public ToggleTaskCompletionHandler(ITaskDbContext context)
	{
		_context = context;
	}

	public async Task<ToggleTaskCompletionResult> HandleAsync(ToggleTaskCompletionCommand command)
	{
		var task = await _context.Tasks
			.Where(t => t.Id == command.TaskId && t.DeletedAt == null)
			.FirstOrDefaultAsync();

		if (task == null)
		{
			throw new KeyNotFoundException($"Task with ID {command.TaskId} not found");
		}

		// Toggle completion status
		if (task.CompletedAt == null)
		{
			// Mark as completed
			task.CompletedAt = DateTime.UtcNow;
		}
		else
		{
			// Mark as incomplete
			task.CompletedAt = null;
		}

		await _context.SaveChangesAsync();

		return new ToggleTaskCompletionResult(
			task.Id,
			task.CompletedAt
		);
	}
}
