using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Commands;

public record DeleteTaskCommand(string Id) : ICommand;

public record DeleteTaskResult(string Id);
