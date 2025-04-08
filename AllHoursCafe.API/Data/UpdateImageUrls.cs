using Microsoft.EntityFrameworkCore;
using AllHoursCafe.API.Models;
using System.IO;

namespace AllHoursCafe.API.Data
{
    public class UpdateImageUrls
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdateImageUrls> _logger;

        public UpdateImageUrls(ApplicationDbContext context, ILogger<UpdateImageUrls> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateMenuItemImageUrlsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to update menu item image URLs");

                // Get all menu items
                var menuItems = await _context.MenuItems
                    .Include(m => m.Category)
                    .ToListAsync();

                foreach (var item in menuItems)
                {
                    if (string.IsNullOrEmpty(item.ImageUrl))
                        continue;

                    // Extract the filename from the old path
                    string oldFileName = Path.GetFileName(item.ImageUrl);
                    string categoryName = item.Category?.Name?.ToLower() ?? "unknown";

                    // Special handling for dessert items
                    if (item.Name == "Chocolate Brownie" || item.Name == "New York Cheesecake" || item.Name == "Fruit Tart")
                    {
                        categoryName = "dessert";
                    }

                    // Map old filenames to new filenames
                    string newFileName = MapFileName(oldFileName, item.Name);

                    // Create the new path
                    string newPath = $"/images/Items/{categoryName}/{newFileName}";

                    _logger.LogInformation($"Updating image URL for {item.Name} from {item.ImageUrl} to {newPath}");

                    // Update the image URL
                    item.ImageUrl = newPath;
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated menu item image URLs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item image URLs");
                throw;
            }
        }

        private string MapFileName(string oldFileName, string itemName)
        {
            // Map item names to the actual image files
            switch (itemName)
            {
                case "Classic Pancakes":
                    return "classic-pancake.jpg";
                case "Chicken Caesar Salad":
                    return "chicken-caesar-salad.jpg";
                case "Vegetable Stir Fry":
                    return "vegetable-stir-fry.jpg";
                case "Fresh Brewed Coffee":
                    return "fresh-brewed-coffee.jpg";
                case "Chocolate Brownie":
                    return "chocolate-brownie.jpg";
                case "New York Cheesecake":
                    return "new-york-cheesecake.jpg";
                case "Fruit Tart":
                    return "fruit-tart.jpg";
                case "Avocado Toast":
                    return "Avocado-Toast.jpg";
                case "Breakfast Burrito":
                    return "Breakfast-Burrito.jpg";
                case "Turkey Club Sandwich":
                    return "turkey-club-sandwich.jpg";
                case "Grilled Salmon":
                    return "Grilled-Salmon.jpg";
                case "Pasta Primavera":
                    return "Pasta-Primavera.jpg";
                case "Iced Tea":
                    return "Iced-Tea.jpg";
                case "Fruit Smoothie":
                    return "Fruit-Smoothie.jpg";
                case "Hummus Plate":
                    return "Hummus-Plate.jpg";
                case "Cheese Board":
                    return "Cheese-Board.jpg";
                case "Sweet Potato Fries":
                    return "Sweet-Potato-Fries.jpg";
                // Default case
                default:
                    // If no specific mapping, try to use a simplified version of the item name
                    return itemName.Replace(" ", "-").ToLower() + ".jpg";
            }
        }
    }
}
