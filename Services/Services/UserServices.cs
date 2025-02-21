using AutoMapper;
using AutoMapper.QueryableExtensions;
using Interfracture.Base;
using Interfracture.DTOs;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces.Interfaces;

namespace Services.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserServices(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
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
    }
}
