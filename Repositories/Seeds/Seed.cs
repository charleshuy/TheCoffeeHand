using Core.Constants;
using Interfracture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Base;

namespace Repositories.Seeds
{
    public class Seed
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed roles
            await SeedRoles(roleManager);

            // Seed admin user
            await SeedAdminUser(userManager);

            // Seed categories
            await SeedCategories(context);

            // Seed drinks
            await SeedDrinks(context);

            // Seed orders
            await SeedOrders(context, userManager);

            // Seed ingredients
            await SeedIngredients(context);
        }

        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@thecoffeehand.com",
                Email = "admin@thecoffeehand.com",
                FirstName = "Admin",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            string adminPassword = "Admin@123";
            var user = await userManager.FindByEmailAsync(adminUser.Email);

            if (user == null)
            {
                var createUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedCategories(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Coffee" },
                    new Category { Name = "Tea" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDrinks(ApplicationDbContext context)
        {
            if (!context.Drinks.Any())
            {
                var coffeeCategory = context.Categories.First(c => c.Name == "Coffee");
                var teaCategory = context.Categories.First(c => c.Name == "Tea");

                context.Drinks.AddRange(
                    new Drink { Name = "Espresso", Price = 2.50, CategoryId = coffeeCategory.Id, isAvailable = true },
                    new Drink { Name = "Latte", Price = 3.50, CategoryId = coffeeCategory.Id, isAvailable = true },
                    new Drink { Name = "Green Tea", Price = 2.00, CategoryId = teaCategory.Id, isAvailable = true }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedOrders(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!context.Orders.Any())
            {
                var adminUser = await userManager.FindByEmailAsync("admin@thecoffeehand.com");
                var espresso = context.Drinks.First(d => d.Name == "Espresso");
                var latte = context.Drinks.First(d => d.Name == "Latte");

                var order = new Order
                {
                    Date = DateTimeOffset.Now,
                    Status = EnumOrderStatus.Done,
                    TotalPrice = 6.00, // Espresso + Latte
                    UserId = adminUser.Id,
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail { DrinkId = espresso.Id, Total = 1, Note = "No sugar" },
                        new OrderDetail { DrinkId = latte.Id, Total = 1, Note = "Extra milk" }
                    }
                };

                context.Orders.Add(order);
                await context.SaveChangesAsync();
            }
        }
        private static async Task SeedIngredients(ApplicationDbContext context)
        {
            if (!context.Ingredients.Any()) // Prevent duplicate seeding
            {
                context.Ingredients.AddRange(
                    new Ingredient { Name = "Coffee Beans", Quantity = 1000 },
                    new Ingredient { Name = "Milk", Quantity = 50 },
                    new Ingredient { Name = "Sugar", Quantity = 500 },
                    new Ingredient { Name = "Tea Leaves", Quantity = 300 },
                    new Ingredient { Name = "Cocoa Powder", Quantity = 200 },
                    new Ingredient { Name = "Vanilla Syrup", Quantity = 150 }
                );
                await context.SaveChangesAsync();
            }
        }

    }
}
