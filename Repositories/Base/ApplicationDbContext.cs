using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Interfracture.Entities;

namespace Repositories.Base
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        // Add DbSet properties for your entities
        public DbSet<Drink> Drinks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the schema if needed
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users"); // Rename the table
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles"); // Rename the table
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles"); // Rename the table
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims"); // Rename the table
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins"); // Rename the table
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims"); // Rename the table
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens"); // Rename the table
            });
            // Configure relationships for your custom entities

            // 1. Drink and Category (1 Drink belongs to 1 Category)
            builder.Entity<Drink>()
                .HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // 2. Order and ApplicationUser (1 User can have many Orders)
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // 3. Order and OrderDetail (1 Order can have many OrderDetails)
            builder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete OrderDetails when Order is deleted

            // 4. OrderDetail and Drink (1 Drink can be in many OrderDetails)
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Drink)
                .WithMany()
                .HasForeignKey(od => od.DrinkId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        }
    }
}

