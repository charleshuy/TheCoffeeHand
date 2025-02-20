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

        public UserServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            // Apply filters
            if (!string.IsNullOrWhiteSpace(firstName))
                usersQuery = usersQuery.Where(u => u.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                usersQuery = usersQuery.Where(u => u.LastName.Contains(lastName));

            if (!string.IsNullOrWhiteSpace(phone))
                usersQuery = usersQuery.Where(u => u.PhoneNumber.Contains(phone));

            if (!string.IsNullOrWhiteSpace(email))
                usersQuery = usersQuery.Where(u => u.Email.Contains(email));

            var roleRepo = _unitOfWork.GetRepository<IdentityRole>().Entities;
            var userRoleRepo = _unitOfWork.GetRepository<IdentityUserRole<string>>().Entities;

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var role = await roleRepo.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    var userIds = userRoleRepo.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId);
                    usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
                }
                else
                {
                    return new BasePaginatedList<UserDTO>(new List<UserDTO>(), 0, pageNumber, pageSize);
                }
            }

            int totalItems = await usersQuery.CountAsync();

            // Apply pagination and project using AutoMapper
            var users = await usersQuery
                .OrderBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<UserDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // Fetch roles separately and assign them
            var userIdsList = users.Select(u => u.Id).ToList();
            var userRoles = await userRoleRepo
                .Where(ur => userIdsList.Contains(ur.UserId))
                .Join(roleRepo, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .ToListAsync();

            foreach (var user in users)
            {
                user.Roles = userRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.Name)
                    .ToList();
            }

            return new BasePaginatedList<UserDTO>(users, totalItems, pageNumber, pageSize);
        }
    }
}
