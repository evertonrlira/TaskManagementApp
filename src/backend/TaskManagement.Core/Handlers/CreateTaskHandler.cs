using FluentValidation;
using TaskManagement.Core.Commands;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Handlers;

public class CreateTaskHandler : ICommandHandler<CreateTaskCommand, CreateTaskResult>
{
	private readonly ITaskDbContext _context;
	private readonly IValidator<UserTask> _validator;

	public CreateTaskHandler(ITaskDbContext context, IValidator<UserTask> validator)
	{
		_context = context;
		_validator = validator;
	}

	public async Task<CreateTaskResult> HandleAsync(CreateTaskCommand command)
	{
		var task = new UserTask
		{
			Title = command.Title,
			Description = command.Description,
			UserId = command.UserId,
			CreatedAt = DateTime.UtcNow
		};

		var validationResult = await _validator.ValidateAsync(task);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		_context.Tasks.Add(task);
		await _context.SaveChangesAsync();

		return new CreateTaskResult(
			task.Id,
			task.Title,
			task.Description,
			task.Status,
			task.CreatedAt,
			task.CompletedAt
		);
	}
}
