using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Queries;

public record GetUsersQuery() : IQuery;

public record UserDto(Guid Id, string Name);

public record GetUsersResult(IEnumerable<UserDto> Users);
