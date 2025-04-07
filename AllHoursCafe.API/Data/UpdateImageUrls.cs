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
            // Map common filenames to the actual files in the Items directory
            switch (oldFileName.ToLower())
            {
                case "pancakes.jpg":
                    return "pancake.jpg";
                case "avocado-toast.jpg":
                    return "Avocado-Toast.jpg";
                case "breakfast-burrito.jpg":
                    return "Breakfast-Burrito.jpg";
                // Add more mappings as needed
                default:
                    // If no specific mapping, try to use a simplified version of the item name
                    return itemName.Replace(" ", "-").ToLower() + ".jpg";
            }
        }
    }
}
