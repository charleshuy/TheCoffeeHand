using Interfracture.PaggingItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IRedisCacheServices _cacheService;

        public UserController(IUserServices userServices, IRedisCacheServices cacheService)
        {
            _userServices = userServices;
            _cacheService = cacheService;
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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userServices.GetUserByIdAsync(userId);
            return Ok(user);
        }
    }
}
