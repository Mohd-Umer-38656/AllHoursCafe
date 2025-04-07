using System;
using System.Threading.Tasks;
using AllHoursCafe.API.Data;
using AllHoursCafe.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AllHoursCafe.API.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(ApplicationDbContext context, ILogger<ReservationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Reservation
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Reservation/Create
        public IActionResult Create()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the return URL in TempData
                TempData["ReturnUrl"] = "/Reservation/Create";

                // Redirect to login page
                return RedirectToAction("Login", "Auth");
            }

            // Set default reservation date and time
            var reservation = new Reservation
            {
                ReservationDate = DateTime.Today.AddDays(1),
                ReservationTime = DateTime.Today.AddHours(18) // Default to 6 PM
            };

            return View(reservation);
        }

        // POST: /Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the return URL in TempData
                TempData["ReturnUrl"] = "/Reservation/Create";

                // Redirect to login page
                return RedirectToAction("Login", "Auth");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Combine date and time
                    var reservationDateTime = reservation.ReservationDate.Date.Add(reservation.ReservationTime.TimeOfDay);

                    // Ensure reservation is in the future
                    if (reservationDateTime <= DateTime.Now)
                    {
                        ModelState.AddModelError("ReservationDate", "Reservation must be for a future date and time.");
                        return View(reservation);
                    }

                    // Set creation time
                    reservation.CreatedAt = DateTime.Now;

                    // Add to database
                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"New reservation created for {reservation.Name} on {reservationDateTime}");

                    // Redirect to confirmation page
                    return RedirectToAction(nameof(Confirmation), new { id = reservation.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating reservation");
                    ModelState.AddModelError("", "An error occurred while processing your reservation. Please try again.");
                }
            }

            return View(reservation);
        }

        // GET: /Reservation/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }
    }
}
