using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Queries;

namespace TaskManagement.Core.Handlers;

public class GetUsersHandler : IQueryHandler<GetUsersQuery, GetUsersResult>
{
	private readonly ITaskDbContext _context;

	public GetUsersHandler(ITaskDbContext context)
	{
		_context = context;
	}

	public async Task<GetUsersResult> HandleAsync(GetUsersQuery query)
	{
		var users = await _context.Users
			.Select(u => new UserDto(u.Id, u.Name))
			.ToListAsync();

		return new GetUsersResult(users);
	}
}
