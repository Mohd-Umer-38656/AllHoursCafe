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
            if (id != menuItem.Id)
            {
                return NotFound();
            }

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

                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
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
            if (ModelState.IsValid)
            {
                try
                {
                    // Set a default image URL if none is provided
                    if (string.IsNullOrEmpty(category.ImageUrl))
                    {
                        category.ImageUrl = "/images/categories/default-category.jpg";
                    }

                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating category");
                    ModelState.AddModelError("", "Unable to create category. Please try again.");
                }
            }
            return View(category);
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

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the category has an image URL
                    if (string.IsNullOrEmpty(category.ImageUrl))
                    {
                        category.ImageUrl = "/images/categories/default-category.jpg";
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
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
    }
}
