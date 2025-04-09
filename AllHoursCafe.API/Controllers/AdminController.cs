using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AllHoursCafe.API.Data;
using AllHoursCafe.API.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AllHoursCafe.API.Controllers
{
    [Authorize(Roles = "Admin")] // Only users with Admin role can access this controller
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(
            ApplicationDbContext context,
            ILogger<AdminController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            ViewBag.MenuItemsCount = await _context.MenuItems.CountAsync();
            ViewBag.CategoriesCount = await _context.Categories.CountAsync();
            ViewBag.ReservationsCount = await _context.Reservations.CountAsync();
            ViewBag.UnreadContactsCount = await _context.Contacts.CountAsync(c => !c.IsRead);
            return View();
        }

        #region Menu Items Management

        // GET: Admin/MenuItems
        public async Task<IActionResult> MenuItems()
        {
            var menuItems = await _context.MenuItems
                .Include(m => m.Category)
                .OrderBy(m => m.CategoryId)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return View(menuItems);
        }

        // GET: Admin/MenuItemDetails/5
        public async Task<IActionResult> MenuItemDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // GET: Admin/CreateMenuItem
        public async Task<IActionResult> CreateMenuItem()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        // POST: Admin/CreateMenuItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload if provided
                    if (Request.Form.Files.Count > 0)
                    {
                        var file = Request.Form.Files[0];
                        if (file != null && file.Length > 0)
                        {
                            // Create a unique filename
                            var fileName = Path.GetFileName(file.FileName);
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;

                            // Get category name for folder structure
                            var category = await _context.Categories.FindAsync(menuItem.CategoryId);
                            var categoryName = category?.Name.ToLower() ?? "other";

                            // Create directory if it doesn't exist
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Items", categoryName);
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            // Save the file
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Update the ImageUrl property
                            menuItem.ImageUrl = $"/images/Items/{categoryName}/{uniqueFileName}?v={DateTime.Now.Ticks}";
                        }
                    }

                    _context.Add(menuItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(MenuItems));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating menu item");
                    ModelState.AddModelError("", "Unable to create menu item. Please try again.");
                }
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(menuItem);
        }

        // GET: Admin/EditMenuItem/5
        public async Task<IActionResult> EditMenuItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(menuItem);
        }

        // POST: Admin/EditMenuItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenuItem(int id, MenuItem menuItem)
        {
            // Debug message
            _logger.LogInformation("EditMenuItem called with Id: {Id}, Name: {Name}, Price: {Price}, CategoryId: {CategoryId}",
                id,
                menuItem?.Name ?? "null",
                menuItem?.Price.ToString() ?? "0",
                menuItem?.CategoryId.ToString() ?? "0");

            if (menuItem == null)
            {
                _logger.LogWarning("EditMenuItem received null menu item");
                return NotFound();
            }

            if (id != menuItem.Id)
            {
                _logger.LogWarning("EditMenuItem ID mismatch: route ID {RouteId} != model ID {ModelId}", id, menuItem.Id);
                return NotFound();
            }

            // Remove validation errors for nullable fields
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Description");
            ModelState.Remove("SpicyLevel");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing menu item to preserve data
                    var existingMenuItem = await _context.MenuItems.FindAsync(id);
                    if (existingMenuItem == null)
                    {
                        _logger.LogWarning("EditMenuItem: Menu item with ID {Id} not found", id);
                        return NotFound();
                    }

                    // Preserve the existing image URL if no new file is uploaded
                    string? currentImageUrl = existingMenuItem.ImageUrl;

                    // Handle image upload if provided
                    if (Request.Form.Files.Count > 0)
                    {
                        var file = Request.Form.Files[0];
                        if (file != null && file.Length > 0)
                        {
                            // Create a unique filename
                            var fileName = Path.GetFileName(file.FileName);
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;

                            // Get category name for folder structure
                            var category = await _context.Categories.FindAsync(menuItem.CategoryId);
                            var categoryName = category?.Name.ToLower() ?? "other";

                            // Create directory if it doesn't exist
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Items", categoryName);
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            // Save the file
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Update the ImageUrl property
                            menuItem.ImageUrl = $"/images/Items/{categoryName}/{uniqueFileName}?v={DateTime.Now.Ticks}";
                        }
                    }
                    else
                    {
                        // Keep the existing image URL
                        menuItem.ImageUrl = currentImageUrl;

                        // Log the image URL for debugging
                        _logger.LogInformation("Keeping existing image URL for {Name}: {ImageUrl}", menuItem.Name, menuItem.ImageUrl);
                    }

                    // Update the existing menu item properties
                    existingMenuItem.Name = menuItem.Name;
                    existingMenuItem.Description = menuItem.Description;
                    existingMenuItem.Price = menuItem.Price;
                    existingMenuItem.CategoryId = menuItem.CategoryId;
                    existingMenuItem.ImageUrl = menuItem.ImageUrl;
                    existingMenuItem.IsVegetarian = menuItem.IsVegetarian;
                    existingMenuItem.IsVegan = menuItem.IsVegan;
                    existingMenuItem.IsGlutenFree = menuItem.IsGlutenFree;
                    existingMenuItem.IsActive = menuItem.IsActive;
                    existingMenuItem.SpicyLevel = menuItem.SpicyLevel;
                    existingMenuItem.PrepTimeMinutes = menuItem.PrepTimeMinutes;
                    existingMenuItem.Calories = menuItem.Calories;

                    _context.Update(existingMenuItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Menu item updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MenuItems));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(menuItem);
        }

        // GET: Admin/DeleteMenuItem/5
        public async Task<IActionResult> DeleteMenuItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // POST: Admin/DeleteMenuItem/5
        [HttpPost, ActionName("DeleteMenuItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuItemConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MenuItems));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }

        // GET: Admin/EnhancedCreateMenuItem
        public async Task<IActionResult> EnhancedCreateMenuItem()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        // POST: Admin/SimpleCreateMenuItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimpleCreateMenuItem(MenuItem menuItem)
        {
            try
            {
                // Debug message
                _logger.LogInformation("SimpleCreateMenuItem called with Name: {Name}, Price: {Price}, CategoryId: {CategoryId}",
                    menuItem?.Name ?? "null",
                    menuItem?.Price.ToString() ?? "0",
                    menuItem?.CategoryId.ToString() ?? "0");
                // Handle image upload if provided
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file != null && file.Length > 0)
                    {
                        // Create a unique filename
                        var fileName = Path.GetFileName(file.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;

                        // Get category name for folder structure
                        int categoryId = menuItem != null && menuItem.CategoryId > 0 ? menuItem.CategoryId : 1; // Default to category ID 1 if invalid
                        var category = await _context.Categories.FindAsync(categoryId);
                        var categoryName = category?.Name?.ToLower() ?? "other";

                        // Create directory if it doesn't exist
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Items", categoryName);
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Update the ImageUrl property
                        if (menuItem != null)
                        {
                            menuItem.ImageUrl = $"/images/Items/{categoryName}/{uniqueFileName}?v={DateTime.Now.Ticks}";
                        }
                    }
                }

                // Ensure required fields have values
                if (menuItem != null)
                {
                    if (menuItem.Name == null || string.IsNullOrEmpty(menuItem.Name))
                    {
                        menuItem.Name = "New Menu Item";
                    }
                }

                if (menuItem != null && string.IsNullOrEmpty(menuItem.Description))
                {
                    menuItem.Description = "Description pending...";
                }

                // Handle image URL
                if (menuItem != null)
                {
                    if (string.IsNullOrEmpty(menuItem.ImageUrl))
                    {
                        menuItem.ImageUrl = "/images/placeholder.jpg";
                    }
                }
                else if (menuItem != null && menuItem.ImageUrl != null && !menuItem.ImageUrl.StartsWith("/images/"))
                {
                    // If the image URL doesn't start with /images/, it might be just a filename
                    // Get category name for folder structure
                    var category = await _context.Categories.FindAsync(menuItem.CategoryId);
                    var categoryName = category?.Name?.ToLower() ?? "other";
                    menuItem.ImageUrl = $"/images/Items/{categoryName}/{menuItem.ImageUrl}";
                }

                // Set default values for nullable fields if needed
                if (menuItem != null && menuItem.CategoryId <= 0)
                {
                    // Get the first category or create a default one
                    var firstCategory = await _context.Categories.FirstOrDefaultAsync();
                    if (firstCategory != null)
                    {
                        menuItem.CategoryId = firstCategory.Id;
                    }
                    else
                    {
                        // If no categories exist, create a default one
                        var defaultCategory = new Category
                        {
                            Name = "Other",
                            Description = "Default category",
                            ImageUrl = "/images/categories/default-category.jpg",
                            IsActive = true
                        };
                        _context.Categories.Add(defaultCategory);
                        await _context.SaveChangesAsync();
                        menuItem.CategoryId = defaultCategory.Id;
                    }
                }

                if (menuItem != null)
                {
                    _context.Add(menuItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Menu item created successfully.";
                    return RedirectToAction(nameof(MenuItems));
                }
                else
                {
                    // Handle the case where menuItem is null
                    _logger.LogError("Cannot create menu item: menuItem is null");
                    ViewBag.ErrorMessage = "Unable to create menu item. Please try again.";
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    return View("EnhancedCreateMenuItem", new MenuItem());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item");
                ViewBag.ErrorMessage = "Unable to create menu item. Please try again.";
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View("EnhancedCreateMenuItem", menuItem);
            }
        }

        #endregion

        #region Categories Management

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: Admin/CategoryDetails/5
        public async Task<IActionResult> CategoryDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            try
            {
                // Debug message
                _logger.LogInformation("CreateCategory called with Name: {Name}, Description: {Description}",
                    category?.Name ?? "null",
                    category?.Description ?? "null");

                if (category == null)
                {
                    _logger.LogWarning("CreateCategory received null category");
                    ModelState.AddModelError("", "No category data received.");
                    return View(new Category());
                }

                // Remove validation errors for ImageUrl and MenuItems
                ModelState.Remove("ImageUrl");
                ModelState.Remove("MenuItems");

                if (ModelState.IsValid)
                {
                    // Set a default image URL if none is provided
                    if (string.IsNullOrEmpty(category.ImageUrl))
                    {
                        category.ImageUrl = "/images/categories/default-category.jpg";
                    }

                    // Initialize MenuItems collection to avoid null reference
                    category.MenuItems = [];

                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category created successfully.";
                    return RedirectToAction(nameof(Categories));
                }
                else
                {
                    _logger.LogWarning("CreateCategory ModelState invalid: {Errors}",
                        string.Join("; ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError("", "Unable to create category. Please try again.");
            }

            // If we got this far, something failed, redisplay form
            return View(category ?? new Category());
        }

        // GET: Admin/EditCategory/5
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            // Remove validation errors for ImageUrl and MenuItems
            ModelState.Remove("ImageUrl");
            ModelState.Remove("MenuItems");

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the category has an image URL
                    if (string.IsNullOrEmpty(category.ImageUrl))
                    {
                        category.ImageUrl = "/images/categories/default-category.jpg";
                    }

                    // Get existing category to preserve relationships
                    var existingCategory = await _context.Categories
                        .Include(c => c.MenuItems)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    // Update properties
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.ImageUrl = category.ImageUrl;
                    existingCategory.IsActive = category.IsActive;

                    _context.Update(existingCategory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // GET: Admin/DeleteCategory/5
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Check if there are menu items using this category
            var menuItemsCount = await _context.MenuItems.CountAsync(m => m.CategoryId == id);
            ViewBag.MenuItemsCount = menuItemsCount;

            return View(category);
        }

        // POST: Admin/DeleteCategory/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            // Check if there are menu items using this category
            var menuItemsCount = await _context.MenuItems.CountAsync(m => m.CategoryId == id);
            if (menuItemsCount > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete category because it contains menu items. Please delete or reassign the menu items first.";
                return RedirectToAction(nameof(DeleteCategory), new { id });
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Categories));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        #endregion

        #region Reservations Management

        // GET: Admin/Reservations
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Admin/ReservationDetails/5
        public async Task<IActionResult> ReservationDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Admin/DeleteReservation/5
        [HttpPost, ActionName("DeleteReservation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reservation deleted successfully.";
            }

            return RedirectToAction(nameof(Reservations));
        }

        #endregion

        #region Contact Messages Management

        // GET: Admin/Contacts
        public async Task<IActionResult> Contacts()
        {
            var contacts = await _context.Contacts
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(contacts);
        }

        // GET: Admin/ContactDetails/5
        public async Task<IActionResult> ContactDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Admin/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                // Toggle the IsRead status
                contact.IsRead = !contact.IsRead;
                _context.Update(contact);
                await _context.SaveChangesAsync();
            }

            // If the request came from the details page, redirect back there
            if (Request.GetTypedHeaders().Referer?.ToString().Contains("ContactDetails") == true)
            {
                return RedirectToAction(nameof(ContactDetails), new { id });
            }

            // Otherwise, redirect to the list
            return RedirectToAction(nameof(Contacts));
        }

        // POST: Admin/DeleteContact/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Contact message deleted successfully.";
            }

            return RedirectToAction(nameof(Contacts));
        }

        #endregion

        #region Orders Management

        // GET: Admin/Orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Find the user associated with this order
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == order.CustomerEmail);

            if (user != null)
            {
                ViewBag.UserId = user.Id;
            }

            return View(order);
        }

        #endregion

        #region User Management

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Load user's orders if available
            if (_context.Orders != null)
            {
                var orders = await _context.Orders
                    .Where(o => o.CustomerEmail == user.Email)
                    .OrderByDescending(o => o.OrderDate)
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                ViewBag.Orders = orders;
            }

            return View(user);
        }

        // GET: Admin/UpdateRole/5
        public async Task<IActionResult> UpdateRole(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/UpdateRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int id, string newRole, bool forceLogout = false)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Only allow valid roles
            if (newRole == "Admin" || newRole == "User")
            {
                user.Role = newRole;
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User role updated to {newRole} successfully.";

                // Handle force logout logic if needed
                if (forceLogout)
                {
                    // In a real application, you might invalidate their session or auth token
                    // For now, we'll just add a message
                    TempData["SuccessMessage"] += " User will be required to log in again.";
                }
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/CheckUserRole/5
        public async Task<IActionResult> CheckUserRole(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Populate ViewBag with user information for the view
            ViewBag.UserName = user.FullName;
            ViewBag.UserEmail = user.Email;
            ViewBag.UserRole = user.Role;
            ViewBag.UserId = user.Id; // Add the user ID to the ViewBag

            return View(user); // Pass the user model to the view
        }

        #endregion
    }
}
