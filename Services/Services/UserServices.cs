using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Base;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Interfracture.PaggingItems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Services.DTOs;
using Services.ServiceInterfaces;
using System.Security.Claims;

namespace Services.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRedisCacheServices _cacheService;
        private readonly IMemoryCache _cache;

        public UserServices(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IRedisCacheServices cacheService, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _cacheService = cacheService;
        }


        public async Task<PaginatedList<UserDTO>> SearchUsersAsync(
            string? firstName,
            string? lastName,
            string? phone,
            string? email,
            string? roleName,
            int pageNumber,
            int pageSize)
        {
            string cacheKey = $"users_{firstName}_{lastName}_{phone}_{email}_{roleName}_{pageNumber}_{pageSize}";

            var cachedUsers = await _cacheService.GetAsync<PaginatedList<UserDTO>>(cacheKey);
            if (cachedUsers != null) return cachedUsers; // ✅ Return from cache if found

            var usersQuery = _unitOfWork.GetRepository<ApplicationUser>().Entities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(firstName))
                usersQuery = usersQuery.Where(u => u.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                usersQuery = usersQuery.Where(u => u.LastName.Contains(lastName));

            if (!string.IsNullOrWhiteSpace(phone))
                usersQuery = usersQuery.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(phone));

            if (!string.IsNullOrWhiteSpace(email))
                usersQuery = usersQuery.Where(u => u.Email != null && u.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                var userIds = usersInRole.Select(u => u.Id).ToList();
                usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
            }

            int totalItems = await usersQuery.CountAsync();
            var users = await usersQuery
                .OrderBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<UserDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                user.Roles = appUser != null ? (await _userManager.GetRolesAsync(appUser)).ToList() : new List<string>();
            }

            var paginatedUsers = new PaginatedList<UserDTO>(users, totalItems, pageNumber, pageSize);

            // ✅ Store in cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, paginatedUsers, TimeSpan.FromMinutes(30));

            return paginatedUsers;


        }

        public async Task<UserDTO?> GetUserByIdAsync(string userId)
        {
            string cacheKey = $"user_{userId}";

            var cachedUser = await _cacheService.GetAsync<UserDTO>(cacheKey);
            if (cachedUser != null) return cachedUser;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new BaseException.NotFoundException("user_not_found", $"User with ID {userId} not found.");

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            // ✅ Store in cache for 1 hour
            await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromHours(1));

            return userDto;

        }

        public async Task<UserDTO?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                throw new BaseException.NotFoundException("user_not_found", $"User with ID {userId} not found.");

            string cacheKey = $"CurrentUser_{userId}";

            if (!_cache.TryGetValue(cacheKey, out UserDTO? cachedUser))
            {
                cachedUser = await GetUserByIdAsync(userId);

                if (cachedUser == null)
                {
                    throw new BaseException.NotFoundException("user_not_found", $"User with ID {userId} not found."); 
                }
                _cache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(10));
            }
            return cachedUser;
        }


        public string? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) throw new BaseException.UnauthorizedException("unauthenticated", "Require authentication"); ;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task RemoveUserCacheAsync(string userId)
        {
            await _cacheService.RemoveAsync($"user_{userId}");
        }

        public async Task RemoveUsersCacheAsync()
        {
            await _cacheService.RemoveByPrefixAsync("users_");
        }
    }
}
