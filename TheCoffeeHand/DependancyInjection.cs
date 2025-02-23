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
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;

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
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddDistributedMemoryCache();

            // Add Firebase Authentication
            services.AddAuthentication(configuration);

            // Add Authorization
            services.AddAuthorization();

            // Add Swagger Configuration
            services.AddSwaggerDocumentation();

            // Add Controllers and API-related services
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            // Add Redis Service
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisHost = configuration["Redis:Host"];
                var redisPort = configuration["Redis:Port"];
                var redisPassword = configuration["Redis:Password"];

                var options = new ConfigurationOptions
                {
                    EndPoints = { $"{redisHost}:{redisPort}" },
                    Password = redisPassword,
                    AbortOnConnectFail = false
                };

                return ConnectionMultiplexer.Connect(options);
            });

        }

        private static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                string adminSdkRelativePath = configuration["Firebase:AdminSDKPath"];
                string adminSdkFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, adminSdkRelativePath);

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(adminSdkFullPath)
                });
            }
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // 🔥 Firebase Authentication
            .AddJwtBearer("Firebase", options =>
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
            })
            // 🔑 Custom JWT Authentication
            .AddJwtBearer("Jwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // ❌ No Issuer validation
                    ValidateAudience = false, // ❌ No Audience validation
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtKey,
                    RoleClaimType = ClaimTypes.Role
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
