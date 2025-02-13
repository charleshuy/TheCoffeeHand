using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Interfracture.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.Base;
using Services;

namespace TheCoffeeHand
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRepositoryLayer();
            services.AddServiceLayer();
            services.AddAutoMapper(typeof(Services.DependencyInjection).Assembly);

            // Add DbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add Firebase Authentication
            services.AddFirebaseAuthentication(configuration);

            // Add Authorization
            services.AddAuthorization();

            // Add Swagger Configuration
            services.AddSwaggerDocumentation();

            // Add Controllers and API-related services
            services.AddControllers();
            services.AddEndpointsApiExplorer();
        }

        private static void AddFirebaseAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Initialize Firebase Admin SDK
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(configuration["Firebase:AdminSDKPath"])
            });

            // Configure Firebase Authentication using JWT Bearer
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://securetoken.google.com/{configuration["Firebase:ProjectId"]}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{configuration["Firebase:ProjectId"]}",
                        ValidateAudience = true,
                        ValidAudience = configuration["Firebase:ProjectId"],
                        ValidateLifetime = true
                    };
                });
        }

        private static void AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {your Firebase ID token}'"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }
    }
}
