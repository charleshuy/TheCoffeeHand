using Repositories.Seeds;
using TheCoffeeHand.MiddleWares;

namespace TheCoffeeHand {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public class Program {
        /// <summary>
        /// The main method which configures and runs the web application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Register all services using DependencyInjection
            builder.Services.AddApplication(builder.Configuration);

            // Thêm cấu hình CORS
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowLocalhost3000", policy => {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Seed data
            using (var scope = app.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                await Seed.Initialize(services);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowLocalhost3000");

            app.UseMiddleware<ExceptionMiddleware>();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
