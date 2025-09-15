using FluentValidation;
using TaskManagement.Core.Constants;
using UserTask = TaskManagement.Core.Entities.UserTask;

namespace TaskManagement.Core.Validation;

public class TaskValidator : AbstractValidator<UserTask>
{
	public TaskValidator()
	{
		RuleFor(t => t.UserId)
		  .NotEmpty().WithMessage("User ID is required");

		RuleFor(t => t.Title)
		  .NotEmpty().WithMessage("Title is required")
		  .MaximumLength(TaskValidationConstants.MaxTitleLength).WithMessage($"Title cannot exceed {TaskValidationConstants.MaxTitleLength} characters");

		RuleFor(t => t.Description)
		  .MaximumLength(TaskValidationConstants.MaxDescriptionLength).WithMessage($"Description cannot exceed {TaskValidationConstants.MaxDescriptionLength} characters");
	}
}
