using AutoMapper;
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

        public async Task<BasePaginatedList<UserResponseDTO>> SearchUsersAsync(
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
                    return new BasePaginatedList<UserResponseDTO>(new List<UserResponseDTO>(), 0, pageNumber, pageSize);
                }
            }

            int totalItems = await usersQuery.CountAsync();

            // Optimized query: Direct projection instead of retrieving entire entity
            var users = await usersQuery
                .OrderBy(u => u.FirstName) // Optional: Improve pagination predictability
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Roles = userRoleRepo
                        .Where(ur => ur.UserId == u.Id)
                        .Join(roleRepo, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .ToList()
                })
                .ToListAsync();

            return new BasePaginatedList<UserResponseDTO>(users, totalItems, pageNumber, pageSize);
        }

    }
}
