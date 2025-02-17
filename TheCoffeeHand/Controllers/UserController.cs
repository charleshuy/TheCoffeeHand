using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.Interfaces;

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
            var users = await _userServices.SearchUsersAsync(firstName, LastName,  phone, email, roleName, pageNumber, pageSize);
            return Ok(users);
        }
    }
}
