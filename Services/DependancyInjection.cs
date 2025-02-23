using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Interfaces.Interfaces;
using Services.Services;
using Services.Services.MapperProfiles;
using Services.Services.RedisCache;

namespace Services
{
    public static class DependencyInjection
    {
        public static void AddServiceLayer(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<ICategoryService, CategoryServices>();
            services.AddScoped<IRedisCacheServices, RedisCacheServices>();
            //services.AddScoped<IFCMService, FCMService>();
            services.AddHttpClient<IFCMService, FCMService>();
            services.AddAutoMapper(typeof(UserProfile));
        }
    }
}
