using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Queries;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly IQueryHandler<GetUsersQuery, GetUsersResult> _getUsersHandler;

	public UsersController(IQueryHandler<GetUsersQuery, GetUsersResult> getUsersHandler)
	{
		_getUsersHandler = getUsersHandler;
	}

	[HttpGet]
	[ProducesResponseType(typeof(GetUsersResult), StatusCodes.Status200OK)]
	public async Task<ActionResult<GetUsersResult>> GetUsers()
	{
		var result = await _getUsersHandler.HandleAsync(new GetUsersQuery());
		return Ok(result);
	}
}
