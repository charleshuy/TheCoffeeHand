using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces.Interfaces;
using Services.Services;

namespace Services
{
    public static class DependencyInjection
    {
        public static void AddServiceLayer(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
        }
    }
}
