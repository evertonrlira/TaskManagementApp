using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Commands;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Handlers;

public class DeleteTaskHandler : ICommandHandler<DeleteTaskCommand, DeleteTaskResult>
{
	private readonly ITaskDbContext _context;

	public DeleteTaskHandler(ITaskDbContext context)
	{
		_context = context;
	}

	public async Task<DeleteTaskResult> HandleAsync(DeleteTaskCommand command)
	{
		if (!Guid.TryParse(command.Id, out var taskId))
		{
			throw new ArgumentException("Invalid task ID format");
		}

		var task = await _context.Tasks
			.Where(t => t.Id == taskId && t.DeletedAt == null)
			.FirstOrDefaultAsync();

		if (task == null)
		{
			throw new KeyNotFoundException($"Task with ID {command.Id} not found");
		}

		// Soft delete: set DeletedAt timestamp
		task.DeletedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return new DeleteTaskResult(task.Id.ToString());
	}
}
