using Repositories.Seeds;
using TheCoffeeHand.MiddleWares;

namespace TheCoffeeHand
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry method of the application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register all services using DependencyInjection
            builder.Services.AddApplication(builder.Configuration);

            var app = builder.Build();

            // Seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await Seed.Initialize(services);
            }

            if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Coffee Hand API v1");
                });
            }



            app.UseHttpsRedirection();

            // Middleware to handle exceptions globally
            app.UseMiddleware<ExceptionMiddleware>();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map controllers to endpoints
            app.MapControllers();

            app.Run();
        }
    }
}
