using Interfracture.Base;
using Services.DTOs;

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
        Task<UserDTO?> GetUserByIdAsync(string userId);
        Task<UserDTO?> GetCurrentUserAsync();
    }
}
