using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AllHoursCafe.API.Models;
using AllHoursCafe.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AllHoursCafe.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    contact.CreatedAt = DateTime.UtcNow;
                    contact.IsRead = false;

                    _context.Contacts.Add(contact);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"New contact form submission from {contact.Name} ({contact.Email})");

                    TempData["SuccessMessage"] = "Thank you for your message! We will get back to you soon.";
                    return RedirectToAction(nameof(Contact));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving contact form submission");
                    ModelState.AddModelError("", "An error occurred while submitting your message. Please try again.");
                }
            }

            return View(contact);
        }

        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}