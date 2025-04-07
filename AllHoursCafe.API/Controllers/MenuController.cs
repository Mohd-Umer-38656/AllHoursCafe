using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AllHoursCafe.API.Data;
using AllHoursCafe.API.Models;

namespace AllHoursCafe.API.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MenuController> _logger;

        public MenuController(ApplicationDbContext context, ILogger<MenuController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Menu
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get all active categories
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .ToListAsync();

                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("No active categories found in the database");
                    ViewBag.ErrorMessage = "No menu categories found. Please try again later.";
                    return View(new List<Category>());
                }

                // Get all active menu items
                var menuItems = await _context.MenuItems
                    .Where(m => m.IsActive)
                    .ToListAsync();

                // Store menu items in ViewBag for use in the view
                ViewBag.MenuItems = menuItems;

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu page");
                ViewBag.ErrorMessage = "An error occurred while loading the menu. Please try again later.";
                return View(new List<Category>());
            }
        }

        // API Endpoints
        [Route("api/[controller]/categories")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .ToListAsync();

                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("No active categories found in the database");
                    return NotFound(new { message = "No categories found" });
                }

                // Convert to simple objects to avoid serialization issues
                var result = categories.Select(c => new {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.ImageUrl,
                    c.IsActive
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { message = "An error occurred while retrieving categories" });
            }
        }

        [Route("api/[controller]/categories/{id}/items")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItemsByCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found", id);
                    return NotFound(new { message = $"Category with ID {id} not found" });
                }

                var items = await _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => m.CategoryId == id && m.IsActive)
                    .ToListAsync();

                // Add category name to each item for client-side use
                var result = items.Select(item => new {
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Price,
                    item.ImageUrl,
                    item.IsVegetarian,
                    item.IsVegan,
                    item.IsGlutenFree,
                    item.IsActive,
                    item.CategoryId,
                    CategoryName = item.Category?.Name,
                    item.SpicyLevel,
                    item.PrepTimeMinutes,
                    item.Calories
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu items for category {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving menu items" });
            }
        }

        [Route("api/[controller]/items")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetAllMenuItems()
        {
            try
            {
                var items = await _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => m.IsActive)
                    .ToListAsync();

                if (items == null || !items.Any())
                {
                    _logger.LogWarning("No active menu items found in the database");
                    return NotFound(new { message = "No menu items found" });
                }

                // Add category name to each item for client-side use
                var result = items.Select(item => new {
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Price,
                    item.ImageUrl,
                    item.IsVegetarian,
                    item.IsVegan,
                    item.IsGlutenFree,
                    item.IsActive,
                    item.CategoryId,
                    CategoryName = item.Category?.Name,
                    item.SpicyLevel,
                    item.PrepTimeMinutes,
                    item.Calories
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all menu items");
                return StatusCode(500, new { message = "An error occurred while retrieving menu items" });
            }
        }

        [Route("api/[controller]/items/search")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> SearchMenuItems([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    _logger.LogWarning("Search query is empty");
                    return BadRequest(new { message = "Search query cannot be empty" });
                }

                var items = await _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => m.IsActive &&
                        (m.Name.Contains(query) ||
                         m.Description.Contains(query) ||
                         m.Category.Name.Contains(query)))
                    .ToListAsync();

                // Add category name to each item for client-side use
                var result = items.Select(item => new {
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Price,
                    item.ImageUrl,
                    item.IsVegetarian,
                    item.IsVegan,
                    item.IsGlutenFree,
                    item.IsActive,
                    item.CategoryId,
                    CategoryName = item.Category?.Name,
                    item.SpicyLevel,
                    item.PrepTimeMinutes,
                    item.Calories
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching menu items with query: {Query}", query);
                return StatusCode(500, new { message = "An error occurred while searching menu items" });
            }
        }
    }
}