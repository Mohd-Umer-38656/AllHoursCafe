using AllHoursCafe.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AllHoursCafe.API.Data
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(ApplicationDbContext context, ILogger<DbSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Ensure database is created and migrations are applied
                await _context.Database.MigrateAsync();

                // Seed categories if none exist
                if (!await _context.Categories.AnyAsync())
                {
                    _logger.LogInformation("Seeding categories to database...");
                    await SeedCategoriesAsync();
                    _logger.LogInformation("Categories seeded successfully.");
                }

                // Seed menu items if none exist
                if (!await _context.MenuItems.AnyAsync())
                {
                    _logger.LogInformation("Seeding menu items to database...");
                    await SeedMenuItemsAsync();
                    _logger.LogInformation("Menu items seeded successfully.");
                }

                // Seed admin user if none exists
                if (!await _context.Users.AnyAsync(u => u.Role == "Admin"))
                {
                    _logger.LogInformation("Seeding admin user to database...");
                    await SeedAdminUserAsync();
                    _logger.LogInformation("Admin user seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private async Task SeedCategoriesAsync()
        {
            // Create categories
            var categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Breakfast",
                    Description = "Start your day with our delicious breakfast options",
                    ImageUrl = "/images/categories/breakfast.jpg",
                    IsActive = true
                },
                new Category
                {
                    Id = 2,
                    Name = "Lunch",
                    Description = "Midday favorites to fuel your afternoon",
                    ImageUrl = "/images/categories/lunch.jpg",
                    IsActive = true
                },
                new Category
                {
                    Id = 3,
                    Name = "Dinner",
                    Description = "Satisfying dinner options for a perfect evening",
                    ImageUrl = "/images/categories/dinner.jpg",
                    IsActive = true
                },
                new Category
                {
                    Id = 4,
                    Name = "Beverages",
                    Description = "Refreshing drinks to complement your meal",
                    ImageUrl = "/images/categories/beverages.jpg",
                    IsActive = true
                },
                new Category
                {
                    Id = 5,
                    Name = "Desserts",
                    Description = "Sweet treats to satisfy your cravings",
                    ImageUrl = "/images/categories/desserts.jpg",
                    IsActive = true
                },
                new Category
                {
                    Id = 6,
                    Name = "Snacks",
                    Description = "Light bites for any time of day",
                    ImageUrl = "/images/categories/snacks.jpg",
                    IsActive = true
                }
            };

            // Add categories to the database
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedMenuItemsAsync()
        {
            // Breakfast Items
            var breakfastItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Classic Pancakes",
                    Description = "Fluffy pancakes served with maple syrup and butter",
                    Price = 200m,
                    ImageUrl = "/images/Items/breakfast/classic-pancake.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 1, // Breakfast
                    SpicyLevel = "None",
                    PrepTimeMinutes = 15,
                    Calories = 450
                },
                new MenuItem
                {
                    Name = "Avocado Toast",
                    Description = "Toasted sourdough bread topped with mashed avocado, cherry tomatoes, and a poached egg",
                    Price = 100m,
                    ImageUrl = "/images/Items/breakfast/Avocado-Toast.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 1, // Breakfast
                    SpicyLevel = "Mild",
                    PrepTimeMinutes = 10,
                    Calories = 320
                },
                new MenuItem
                {
                    Name = "Breakfast Burrito",
                    Description = "Scrambled eggs, black beans, cheese, and salsa wrapped in a flour tortilla",
                    Price = 90m,
                    ImageUrl = "/images/Items/breakfast/Breakfast-Burrito.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 1, // Breakfast
                    SpicyLevel = "Medium",
                    PrepTimeMinutes = 12,
                    Calories = 550
                }
            };

            // Lunch Items
            var lunchItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Chicken Caesar Salad",
                    Description = "Romaine lettuce, grilled chicken, croutons, and Caesar dressing",
                    Price = 120m,
                    ImageUrl = "/images/Items/lunch/chicken-caesar-salad.jpg",
                    IsVegetarian = false,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 2, // Lunch
                    SpicyLevel = "None",
                    PrepTimeMinutes = 10,
                    Calories = 380
                },
                new MenuItem
                {
                    Name = "Veggie Wrap",
                    Description = "Hummus, mixed greens, cucumber, bell pepper, and avocado wrapped in a spinach tortilla",
                    Price = 100m,
                    ImageUrl = "/images/Items/lunch/Veggie-Wrap.jpg",
                    IsVegetarian = true,
                    IsVegan = true,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 2, // Lunch
                    SpicyLevel = "None",
                    PrepTimeMinutes = 8,
                    Calories = 320
                },
                new MenuItem
                {
                    Name = "Turkey Club Sandwich",
                    Description = "Roasted turkey, bacon, lettuce, tomato, and mayo on toasted bread",
                    Price = 110m,
                    ImageUrl = "/images/Items/lunch/turkey-club-sandwich.jpg",
                    IsVegetarian = false,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 2, // Lunch
                    SpicyLevel = "None",
                    PrepTimeMinutes = 10,
                    Calories = 480
                }
            };

            // Dinner Items
            var dinnerItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Grilled Salmon",
                    Description = "Fresh salmon fillet grilled to perfection, served with roasted vegetables and quinoa",
                    Price = 180m,
                    ImageUrl = "/images/Items/dinner/Grilled-Salmon.jpg",
                    IsVegetarian = false,
                    IsVegan = false,
                    IsGlutenFree = true,
                    IsActive = true,
                    CategoryId = 3, // Dinner
                    SpicyLevel = "None",
                    PrepTimeMinutes = 20,
                    Calories = 420
                },
                new MenuItem
                {
                    Name = "Vegetable Stir Fry",
                    Description = "Mixed vegetables stir-fried in a savory sauce, served over rice",
                    Price = 140m,
                    ImageUrl = "/images/Items/dinner/vegetable-stir-fry.jpg",
                    IsVegetarian = true,
                    IsVegan = true,
                    IsGlutenFree = true,
                    IsActive = true,
                    CategoryId = 3, // Dinner
                    SpicyLevel = "Medium",
                    PrepTimeMinutes = 15,
                    Calories = 350
                },
                new MenuItem
                {
                    Name = "Pasta Primavera",
                    Description = "Fettuccine pasta with seasonal vegetables in a light cream sauce",
                    Price = 150m,
                    ImageUrl = "/images/Items/dinner/Pasta-Primavera.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 3, // Dinner
                    SpicyLevel = "None",
                    PrepTimeMinutes = 18,
                    Calories = 520
                }
            };

            // Beverages
            var beverageItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Fresh Brewed Coffee",
                    Description = "Locally roasted coffee beans, brewed fresh",
                    Price = 34m,
                    ImageUrl = "/images/Items/beverages/fresh-brewed-coffee.jpg",
                    IsVegetarian = true,
                    IsVegan = true,
                    IsGlutenFree = true,
                    IsActive = true,
                    CategoryId = 4, // Beverages
                    SpicyLevel = "None",
                    PrepTimeMinutes = 5,
                    Calories = 5
                },
                new MenuItem
                {
                    Name = "Iced Tea",
                    Description = "Freshly brewed black tea served over ice with lemon",
                    Price = 29m,
                    ImageUrl = "/images/Items/beverages/Iced-Tea.jpg",
                    IsVegetarian = true,
                    IsVegan = true,
                    IsGlutenFree = true,
                    IsActive = true,
                    CategoryId = 4, // Beverages
                    SpicyLevel = "None",
                    PrepTimeMinutes = 3,
                    Calories = 10
                },
                new MenuItem
                {
                    Name = "Fruit Smoothie",
                    Description = "Blend of seasonal fruits with yogurt and honey",
                    Price = 59m,
                    ImageUrl = "/images/Items/beverages/Fruit-Smoothie.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = true,
                    IsActive = true,
                    CategoryId = 4, // Beverages
                    SpicyLevel = "None",
                    PrepTimeMinutes = 5,
                    Calories = 220
                }
            };

            // Desserts
            var dessertItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Chocolate Brownie",
                    Description = "Rich chocolate brownie served with vanilla ice cream",
                    Price = 69m,
                    ImageUrl = "/images/Items/dessert/chocolate-brownie.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 5, // Desserts
                    SpicyLevel = "None",
                    PrepTimeMinutes = 5,
                    Calories = 450
                },
                new MenuItem
                {
                    Name = "New York Cheesecake",
                    Description = "Classic New York style cheesecake with berry compote",
                    Price = 79m,
                    ImageUrl = "/images/Items/dessert/new-york-cheesecake.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 5, // Desserts
                    SpicyLevel = "None",
                    PrepTimeMinutes = 5,
                    Calories = 380
                },
                new MenuItem
                {
                    Name = "Fruit Tart",
                    Description = "Buttery pastry shell filled with custard and topped with fresh seasonal fruits",
                    Price = 64m,
                    ImageUrl = "/images/Items/dessert/fruit-tart.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 5, // Desserts
                    SpicyLevel = "None",
                    PrepTimeMinutes = 5,
                    Calories = 320
                }
            };

            // Snacks
            var snackItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Hummus Plate",
                    Description = "Creamy hummus served with warm pita bread and vegetable sticks",
                    Price = 89m,
                    ImageUrl = "/images/Items/snacks/Hummus-Plate.jpg",
                    IsVegetarian = true,
                    IsVegan = true,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 6, // Snacks
                    SpicyLevel = "None",
                    PrepTimeMinutes = 5,
                    Calories = 280
                },
                new MenuItem
                {
                    Name = "Cheese Board",
                    Description = "Selection of artisanal cheeses with crackers, nuts, and dried fruits",
                    Price = 129m,
                    ImageUrl = "/images/Items/snacks/Cheese-Board.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    IsActive = true,
                    CategoryId = 6, // Snacks
                    SpicyLevel = "None",
                    PrepTimeMinutes = 5,
                    Calories = 420
                },
                new MenuItem
                {
                    Name = "Sweet Potato Fries",
                    Description = "Crispy sweet potato fries served with chipotle aioli",
                    Price = 69m,
                    ImageUrl = "/images/Items/snacks/Sweet-Potato-Fries.jpg",
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = true,
                    IsActive = true,
                    CategoryId = 6, // Snacks
                    SpicyLevel = "Mild",
                    PrepTimeMinutes = 10,
                    Calories = 320
                }
            };

            // Add all menu items to the context
            await _context.MenuItems.AddRangeAsync(breakfastItems);
            await _context.MenuItems.AddRangeAsync(lunchItems);
            await _context.MenuItems.AddRangeAsync(dinnerItems);
            await _context.MenuItems.AddRangeAsync(beverageItems);
            await _context.MenuItems.AddRangeAsync(dessertItems);
            await _context.MenuItems.AddRangeAsync(snackItems);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        private async Task SeedAdminUserAsync()
        {
            // Create admin user
            var adminUser = new User
            {
                FullName = "Admin User",
                Email = "admin@allhourscafe.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default password: Admin@123
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Role = "Admin"
            };

            // Add admin user to the database
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin user created with email: {Email}", adminUser.Email);
        }
    }
}
