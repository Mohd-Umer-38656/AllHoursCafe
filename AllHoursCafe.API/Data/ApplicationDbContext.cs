using Microsoft.EntityFrameworkCore;
using AllHoursCafe.API.Models;

namespace AllHoursCafe.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Breakfast", Description = "Start your day right with our delicious breakfast options", ImageUrl = "/images/categories/breakfast.jpg" },
                new Category { Id = 2, Name = "Lunch", Description = "Perfect midday meals to keep you going", ImageUrl = "/images/categories/lunch.jpg" },
                new Category { Id = 3, Name = "Dinner", Description = "End your day with our satisfying dinner selections", ImageUrl = "/images/categories/dinner.jpg" },
                new Category { Id = 4, Name = "Beverages", Description = "Refreshing drinks and beverages", ImageUrl = "/images/categories/beverages.jpg" },
                new Category { Id = 5, Name = "Desserts", Description = "Sweet treats to satisfy your cravings", ImageUrl = "/images/categories/desserts.jpg" },
                new Category { Id = 6, Name = "Snacks", Description = "Light bites and quick snacks", ImageUrl = "/images/categories/snacks.jpg" }
            );
        }
    }
}