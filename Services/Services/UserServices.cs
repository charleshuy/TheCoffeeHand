using AutoMapper;
using AutoMapper.QueryableExtensions;
using Interfracture.Base;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.DTOs;
using Services.Interfaces.Interfaces;
using System.Security.Claims;

namespace Services.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserServices(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BasePaginatedList<UserDTO>> SearchUsersAsync(
            string? firstName,
            string? lastName,
            string? phone,
            string? email,
            string? roleName,
            int pageNumber,
            int pageSize)
        {
            var usersQuery = _unitOfWork.GetRepository<ApplicationUser>().Entities.AsQueryable();

            // ✅ Apply filters
            if (!string.IsNullOrWhiteSpace(firstName))
                usersQuery = usersQuery.Where(u => u.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                usersQuery = usersQuery.Where(u => u.LastName.Contains(lastName));

            if (!string.IsNullOrWhiteSpace(phone))
                usersQuery = usersQuery.Where(u => u.PhoneNumber.Contains(phone));

            if (!string.IsNullOrWhiteSpace(email))
                usersQuery = usersQuery.Where(u => u.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                var userIds = usersInRole.Select(u => u.Id).ToList();
                usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
            }

            int totalItems = await usersQuery.CountAsync();

            // ✅ Apply pagination and project using AutoMapper
            var users = await usersQuery
                .OrderBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<UserDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // ✅ Fetch roles using UserManager and assign them
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                user.Roles = appUser != null ? (await _userManager.GetRolesAsync(appUser)).ToList() : new List<string>();
            }

            return new BasePaginatedList<UserDTO>(users, totalItems, pageNumber, pageSize);
        }
        public async Task<UserDTO?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return userDto;
        }
        public async Task<UserDTO?> GetCurrentUserAsync()
        {

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            return await GetUserByIdAsync(userId);
        }
        public string? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
