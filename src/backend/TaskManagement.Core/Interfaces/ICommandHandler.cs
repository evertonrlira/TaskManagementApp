namespace TaskManagement.Core.Interfaces;

/// <summary>
/// Base interface for all commands
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Base interface for all command handlers
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface ICommandHandler<in TCommand, TResult>
	where TCommand : ICommand
{
	Task<TResult> HandleAsync(TCommand command);
}
