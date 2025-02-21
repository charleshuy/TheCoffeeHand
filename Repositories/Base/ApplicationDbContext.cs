using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Interfracture.Entities;

namespace Repositories.Base
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
                            IdentityUserClaim<Guid>, IdentityUserRole<Guid>,
                            IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        // ✅ Add DbSet properties for your entities
        public DbSet<Drink> Drinks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Rename Identity Tables
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // ✅ Configure Entity Relationships

            // 1. Drink → Category (1 Drink belongs to 1 Category)
            builder.Entity<Drink>()
                .HasOne(d => d.Category)
                .WithMany(c => c.Drinks)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Order → ApplicationUser (1 User can have many Orders)
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Order → OrderDetails (1 Order can have many OrderDetails)
            builder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. OrderDetail → Drink (1 Drink can be in many OrderDetails)
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Drink)
                .WithMany(d => d.OrderDetails)
                .HasForeignKey(od => od.DrinkId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Recipe → Drink (1 Drink can have many Recipes)
            builder.Entity<Recipe>()
                .HasOne(r => r.Drink)
                .WithMany(d => d.Recipes)
                .HasForeignKey(r => r.DrinkId)
                .OnDelete(DeleteBehavior.Cascade);

            // 6. Recipe → Ingredient (1 Ingredient can have many Recipes)
            builder.Entity<Recipe>()
                .HasOne(r => r.Ingredient)
                .WithMany(i => i.Recipes)
                .HasForeignKey(r => r.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
