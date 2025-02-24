using Interfracture.PaggingItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.Interfaces;
using Services.Interfaces.Interfaces;

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
            string cacheKey = $"users_{pageNumber}_{pageSize}";
            var cachedUser = await _cacheService.GetAsync<PaginatedList<UserDTO>>(cacheKey);
            if (cachedUser != null)
                return Ok(cachedUser);

            var users = await _userServices.SearchUsersAsync(firstName, LastName,  phone, email, roleName, pageNumber, pageSize);
            await _cacheService.SetAsync(cacheKey, users, TimeSpan.FromMinutes(30));
            return Ok(users);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            string cacheKey = $"user_{userId}";

            var user = await _cacheService.GetAsync<UserDTO>(cacheKey);
            if (user != null)
                return Ok(user);

            user = await _userServices.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
            return Ok(user);
        }
    }
}
