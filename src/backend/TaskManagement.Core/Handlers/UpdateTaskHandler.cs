using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Commands;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Handlers;

public class UpdateTaskHandler : ICommandHandler<UpdateTaskCommand, UpdateTaskResult>
{
	private readonly ITaskDbContext _context;
	private readonly IValidator<UserTask> _validator;

	public UpdateTaskHandler(ITaskDbContext context, IValidator<UserTask> validator)
	{
		_context = context;
		_validator = validator;
	}

	public async Task<UpdateTaskResult> HandleAsync(UpdateTaskCommand command)
	{
		var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == command.TaskId && !t.DeletedAt.HasValue);
		
		if (task == null)
		{
			throw new ArgumentException($"Task with ID {command.TaskId} not found.");
		}

		// Update the task properties with trimmed title
		task.Title = command.Title?.Trim() ?? string.Empty;
		task.Description = command.Description?.Trim();

		var validationResult = await _validator.ValidateAsync(task);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		await _context.SaveChangesAsync();

		return new UpdateTaskResult(
			task.Id,
			task.Title,
			task.Description,
			task.CreatedAt,
			task.CompletedAt
		);
	}
}