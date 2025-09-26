using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.Commands;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Queries;

namespace TaskManagement.Api.Controllers;

public record UpdateTaskRequestBody(string Title, string? Description);

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
	private readonly IQueryHandler<GetTasksQuery, GetTasksResult> _getTasksHandler;
	private readonly IQueryHandler<GetTaskStatisticsQuery, GetTaskStatisticsResult> _getTaskStatisticsHandler;
	private readonly ICommandHandler<CreateTaskCommand, CreateTaskResult> _createTaskHandler;
	private readonly ICommandHandler<UpdateTaskCommand, UpdateTaskResult> _updateTaskHandler;
	private readonly ICommandHandler<ToggleTaskCompletionCommand, ToggleTaskCompletionResult> _toggleCompletionHandler;
	private readonly ICommandHandler<DeleteTaskCommand, DeleteTaskResult> _deleteTaskHandler;

	public TasksController(
		IQueryHandler<GetTasksQuery, GetTasksResult> getTasksHandler,
		IQueryHandler<GetTaskStatisticsQuery, GetTaskStatisticsResult> getTaskStatisticsHandler,
		ICommandHandler<CreateTaskCommand, CreateTaskResult> createTaskHandler,
		ICommandHandler<UpdateTaskCommand, UpdateTaskResult> updateTaskHandler,
		ICommandHandler<ToggleTaskCompletionCommand, ToggleTaskCompletionResult> toggleCompletionHandler,
		ICommandHandler<DeleteTaskCommand, DeleteTaskResult> deleteTaskHandler)
	{
		_getTasksHandler = getTasksHandler;
		_getTaskStatisticsHandler = getTaskStatisticsHandler;
		_createTaskHandler = createTaskHandler;
		_updateTaskHandler = updateTaskHandler;
		_toggleCompletionHandler = toggleCompletionHandler;
		_deleteTaskHandler = deleteTaskHandler;
	}

	[HttpGet]
	[ProducesResponseType(typeof(GetTasksResult), StatusCodes.Status200OK)]
	public async Task<ActionResult<GetTasksResult>> GetTasks([FromQuery] GetTasksQuery query)
	{
		var result = await _getTasksHandler.HandleAsync(query);
		return Ok(result);
	}

	[HttpPost]
	[ProducesResponseType(typeof(CreateTaskResult), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<CreateTaskResult>> CreateTask([FromBody] CreateTaskCommand command)
	{
		var result = await _createTaskHandler.HandleAsync(command);
		return CreatedAtAction(nameof(GetTasks), result);
	}

	[HttpPut("{id}")]
	[ProducesResponseType(typeof(UpdateTaskResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<UpdateTaskResult>> UpdateTask([FromRoute] string id, [FromBody] UpdateTaskRequestBody request)
	{
		if (!Guid.TryParse(id, out var taskId))
		{
			return BadRequest(new ProblemDetails
			{
				Title = "Invalid Task ID",
				Detail = "The provided task ID is not a valid GUID format.",
				Status = StatusCodes.Status400BadRequest
			});
		}

		var command = new UpdateTaskCommand(taskId, request.Title, request.Description);
		
		try
		{
			var result = await _updateTaskHandler.HandleAsync(command);
			return Ok(result);
		}
		catch (ArgumentException ex)
		{
			return NotFound(new ProblemDetails
			{
				Title = "Task Not Found",
				Detail = ex.Message,
				Status = StatusCodes.Status404NotFound
			});
		}
	}

	[HttpPatch("{id}/toggleCompletion")]
	[ProducesResponseType(typeof(ToggleTaskCompletionResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ToggleTaskCompletionResult>> ToggleTaskCompletion([FromRoute] string id)
	{
		if (!Guid.TryParse(id, out var taskId))
		{
			return BadRequest(new ProblemDetails
			{
				Title = "Invalid Task ID",
				Detail = "The provided task ID is not a valid GUID format.",
				Status = StatusCodes.Status400BadRequest
			});
		}

		var command = new ToggleTaskCompletionCommand(taskId);
		var result = await _toggleCompletionHandler.HandleAsync(command);
		return Ok(result);
	}

	[HttpDelete("{id}")]
	[ProducesResponseType(typeof(DeleteTaskResult), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<DeleteTaskResult>> DeleteTask([FromRoute] string id)
	{
		var command = new DeleteTaskCommand(id);
		var result = await _deleteTaskHandler.HandleAsync(command);
		return Ok(result);
	}

	[HttpGet("statistics")]
	[ProducesResponseType(typeof(GetTaskStatisticsResult), StatusCodes.Status200OK)]
	public async Task<ActionResult<GetTaskStatisticsResult>> GetTaskStatistics([FromQuery] GetTaskStatisticsQuery query)
	{
		var result = await _getTaskStatisticsHandler.HandleAsync(query);
		return Ok(result);
	}
}
