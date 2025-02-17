using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces.Interfaces;
using Services.Services;
using Services.Services.MapperProfiles;

namespace Services
{
    public static class DependencyInjection
    {
        public static void AddServiceLayer(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddAutoMapper(typeof(UserProfile)); // Register AutoMapper
        }
    }
}
