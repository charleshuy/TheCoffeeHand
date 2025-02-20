
using Interfracture.Base;
using Interfracture.DTOs;
using Interfracture.Entities;

namespace Services.Interfaces.Interfaces
{
    public interface IUserServices
    {
        Task<BasePaginatedList<UserDTO>> SearchUsersAsync(
            string? firstName,
            string? lastName,
            string? phone,
            string? email,
            string? roleName,
            int pageNumber,
            int pageSize);
    }
}
