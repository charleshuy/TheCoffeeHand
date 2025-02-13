
using System.Security.Claims;

namespace Services.Interfaces.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string> GenerateTokenAsync(ClaimsPrincipal user);
    }
}
