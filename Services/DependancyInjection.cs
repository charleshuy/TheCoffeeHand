using Microsoft.Extensions.DependencyInjection;
using Services.ServiceInterfaces;
using Services.Services;
using Services.Services.RedisCache;

namespace Services
{
    public static class DependencyInjection
    {
        public static void AddServiceLayer(this IServiceCollection services)
        {
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<ICategoryService, CategoryServices>();
            services.AddScoped<IRedisCacheServices, RedisCacheServices>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IDrinkService, DrinkService>();
            services.AddScoped<IRecipeService, RecipeService>();
            //services.AddScoped<IFCMService, FCMService>();
            services.AddHttpClient<IFCMService, FCMService>();
        }
    }
}
