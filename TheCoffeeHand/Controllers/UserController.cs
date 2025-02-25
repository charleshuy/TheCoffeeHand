using Microsoft.AspNetCore.Mvc;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string? firstName,
            [FromQuery] string? LastName,
            [FromQuery] string? phone,
            [FromQuery] string? email,
            [FromQuery] string? roleName,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }
            var users = await _userServices.SearchUsersAsync(firstName, LastName,  phone, email, roleName, pageNumber, pageSize);
            return Ok(users);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userServices.GetUserByIdAsync(userId);
            return Ok(user);
        }
    }
}
