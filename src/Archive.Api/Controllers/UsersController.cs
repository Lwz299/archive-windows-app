using Archive.Application.Interfaces;
using Archive.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            return Ok(await _userService.GetUsersAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
