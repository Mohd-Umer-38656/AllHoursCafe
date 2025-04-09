using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllHoursCafe.API.Controllers;
using AllHoursCafe.API.Data;
using AllHoursCafe.API.Extensions;
using AllHoursCafe.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AllHoursCafe.API.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ApplicationDbContext context, ILogger<CheckoutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Checkout
        public IActionResult Index()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the return URL in TempData
                TempData["ReturnUrl"] = "/Checkout";

                // Redirect to login page
                return RedirectToAction("Login", "Auth");
            }
            // Get cart items from session
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index", "Menu");
            }

            // Calculate totals
            decimal subtotal = cartItems.Sum(item => item.Price * item.Quantity);
            decimal tax = Math.Round(subtotal * 0.05m, 2); // 5% tax
            decimal deliveryFee = 30.00m; // Fixed delivery fee
            decimal total = subtotal + tax + deliveryFee;

            // Create view model
            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                SubTotal = subtotal,
                Tax = tax,
                DeliveryFee = deliveryFee,
                Total = total,
                Order = new Order
                {
                    OrderType = "Delivery",
                    DeliveryTime = DateTime.Now.AddHours(1)
                }
            };

            return View(viewModel);
        }

        // POST: /Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            _logger.LogInformation("Checkout POST action called");

            // Set default values for required fields
            if (model.Order != null)
            {
                model.Order.PaymentMethod = model.Order.PaymentMethod ?? "Pending";
                model.Order.PaymentStatus = model.Order.PaymentStatus ?? "Pending";
                model.Order.OrderStatus = model.Order.OrderStatus ?? "Pending";
                model.Order.PaymentDetails = model.Order.PaymentDetails ?? "None";
            }

            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                // Store the return URL in TempData
                TempData["ReturnUrl"] = "/Checkout";

                // Redirect to login page
                return RedirectToAction("Login", "Auth");
            }

            _logger.LogInformation("User authenticated: {Email}", User.Identity.Name);

            // Check model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                }

                // Get cart items from session to redisplay the form
                var cartJson = HttpContext.Session.GetString("Cart");
                var cartItems = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>()
                    : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

                // Recalculate totals
                decimal subtotal = cartItems.Sum(item => item.Price * item.Quantity);
                decimal tax = Math.Round(subtotal * 0.05m, 2);
                decimal deliveryFee = model.Order?.OrderType == "Delivery" ? 30.00m : 0;
                decimal total = subtotal + tax + deliveryFee;

                // Update model with cart items and totals
                model.CartItems = cartItems;
                model.SubTotal = subtotal;
                model.Tax = tax;
                model.DeliveryFee = deliveryFee;
                model.Total = total;

                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Processing checkout with valid model");

                    // Get cart items from session
                    var cartJson = HttpContext.Session.GetString("Cart");
                    _logger.LogInformation("Cart JSON: {CartJson}", cartJson ?? "null");

                    var cartItems = string.IsNullOrEmpty(cartJson)
                        ? new List<CartItem>()
                        : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

                    if (cartItems == null || !cartItems.Any())
                    {
                        _logger.LogWarning("Cart is empty, redirecting to menu");
                        return RedirectToAction("Index", "Menu");
                    }

                    _logger.LogInformation("Cart has {Count} items", cartItems.Count);

                    // Calculate totals
                    decimal subtotal = cartItems.Sum(item => item.Price * item.Quantity);
                    decimal tax = Math.Round(subtotal * 0.05m, 2); // 5% tax
                    decimal deliveryFee = model.Order.OrderType == "Delivery" ? 30.00m : 0; // Fee only for delivery
                    decimal total = subtotal + tax + deliveryFee;

                    // Create order
                    var order = new Order
                    {
                        CustomerName = model.Order.CustomerName,
                        CustomerEmail = model.Order.CustomerEmail,
                        CustomerPhone = model.Order.CustomerPhone,
                        DeliveryAddress = model.Order.DeliveryAddress,
                        City = model.Order.City,
                        State = model.Order.State,
                        PostalCode = model.Order.PostalCode,
                        SpecialInstructions = model.Order.SpecialInstructions,
                        OrderType = model.Order.OrderType,
                        DeliveryTime = model.Order.DeliveryTime,
                        SubTotal = subtotal,
                        Tax = tax,
                        DeliveryFee = deliveryFee,
                        Total = total,
                        PaymentMethod = "Pending",
                        PaymentStatus = "Pending",
                        OrderStatus = "Pending",
                        PaymentDetails = "None",
                        OrderDate = DateTime.Now,
                        OrderItems = cartItems.Select(item => new OrderItem
                        {
                            MenuItemId = item.Id,
                            Name = item.Name,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            Total = item.Price * item.Quantity
                        }).ToList()
                    };

                    // Update user information with checkout details if user is logged in
                    if (User.Identity.IsAuthenticated)
                    {
                        var userEmail = User.Identity.Name; // Email is stored in Identity.Name
                        _logger.LogInformation("Updating user information for: {Email}", userEmail);

                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                        if (user != null)
                        {
                            _logger.LogInformation("Found user with ID: {UserId}", user.Id);

                            // Update user's contact and address information
                            user.PhoneNumber = model.Order.CustomerPhone;
                            user.Address = model.Order.DeliveryAddress;
                            user.City = model.Order.City;
                            user.State = model.Order.State;
                            user.PostalCode = model.Order.PostalCode;

                            _logger.LogInformation("Updated user fields - Phone: {Phone}, Address: {Address}, City: {City}, State: {State}, PostalCode: {PostalCode}",
                                user.PhoneNumber, user.Address, user.City, user.State, user.PostalCode);

                            // Save the changes to the database
                            _context.Update(user);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("User information updated in database - User ID: {UserId}, Phone: {Phone}, Address: {Address}, City: {City}, State: {State}, PostalCode: {PostalCode}",
                                user.Id, user.PhoneNumber, user.Address, user.City, user.State, user.PostalCode);

                            // Reload the user from the database to verify the change
                            await _context.Entry(user).ReloadAsync();
                            _logger.LogInformation("User reloaded from database - User ID: {UserId}, Phone: {Phone}, Address: {Address}",
                                user.Id, user.PhoneNumber, user.Address);

                            _logger.LogInformation("Successfully saved user profile with checkout information - User ID: {UserId}", user.Id);
                        }
                        else
                        {
                            _logger.LogWarning("User not found with email: {Email}", userEmail);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("User not authenticated when updating profile");
                    }

                    // Save order to database
                    _logger.LogInformation("Saving order to database");
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Order saved with ID: {OrderId}", order.Id);

                    // Store order ID in session for payment page
                    HttpContext.Session.SetInt32("CurrentOrderId", order.Id);
                    _logger.LogInformation("Stored order ID in session: {OrderId}", order.Id);

                    // Redirect to payment page
                    _logger.LogInformation("Redirecting to payment page for order: {OrderId}", order.Id);
                    TempData["OrderId"] = order.Id;
                    TempData["RedirectToPayment"] = Url.Action("Payment", "Checkout", new { id = order.Id });

                    // Redirect to the simple payment page for more reliable processing
                    return RedirectToAction("SimplePayment", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing checkout: {Message}", ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logger.LogError("Inner exception: {Message}", ex.InnerException.Message);
                    }
                    _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                    ModelState.AddModelError("", "An error occurred while processing your order. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            // Recalculate totals
            var recalcCartJson = HttpContext.Session.GetString("Cart");
            var recalcCartItems = string.IsNullOrEmpty(recalcCartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(recalcCartJson);

            decimal recalcSubtotal = recalcCartItems.Sum(item => item.Price * item.Quantity);
            decimal recalcTax = Math.Round(recalcSubtotal * 0.05m, 2);
            decimal recalcDeliveryFee = model.Order.OrderType == "Delivery" ? 30.00m : 0;
            decimal recalcTotal = recalcSubtotal + recalcTax + recalcDeliveryFee;

            model.CartItems = recalcCartItems;
            model.SubTotal = recalcSubtotal;
            model.Tax = recalcTax;
            model.DeliveryFee = recalcDeliveryFee;
            model.Total = recalcTotal;

            return View(model);
        }

        // GET: /Checkout/Payment/5
        public async Task<IActionResult> Payment(int id)
        {
            _logger.LogInformation("Payment GET action called for order ID: {OrderId}", id);

            // Check if we have an order ID from TempData (for direct redirects)
            if (TempData["OrderId"] != null)
            {
                int tempOrderId = (int)TempData["OrderId"];
                _logger.LogInformation("Found order ID in TempData: {OrderId}", tempOrderId);

                // If the URL doesn't match the TempData order ID, redirect to the correct URL
                if (id != tempOrderId)
                {
                    _logger.LogInformation("Redirecting to correct payment URL for order ID: {OrderId}", tempOrderId);
                    return RedirectToAction("Payment", new { id = tempOrderId });
                }
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Order not found with ID: {OrderId}", id);
                return NotFound();
            }

            _logger.LogInformation("Found order with ID: {OrderId}, Customer: {Customer}, Total: {Total}",
                order.Id, order.CustomerName, order.Total);

            // Verify this order belongs to the current session
            var sessionOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
            _logger.LogInformation("Session order ID: {SessionOrderId}, Requested order ID: {OrderId}", sessionOrderId, id);

            // If session order ID is null but we have a valid order, set it in the session
            if (sessionOrderId == null)
            {
                _logger.LogInformation("Setting session order ID to: {OrderId}", id);
                HttpContext.Session.SetInt32("CurrentOrderId", id);
            }
            else if (sessionOrderId != id)
            {
                _logger.LogWarning("Order ID mismatch. Session: {SessionOrderId}, Requested: {OrderId}", sessionOrderId, id);
                // Instead of redirecting, update the session to match this order
                HttpContext.Session.SetInt32("CurrentOrderId", id);
                _logger.LogInformation("Updated session order ID to: {OrderId}", id);
            }

            var viewModel = new PaymentViewModel
            {
                Order = order,
                PaymentMethods = new List<string> { "Credit Card", "Debit Card", "UPI", "Cash on Delivery" }
            };

            _logger.LogInformation("Returning payment view for order ID: {OrderId}", id);
            return View(viewModel);
        }

        // POST: /Checkout/Payment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(int id, PaymentViewModel model)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Verify this order belongs to the current session
            var sessionOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (sessionOrderId != id)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update order with payment information
                    order.PaymentMethod = model.PaymentMethod;

                    // For demo purposes, we'll mark the payment as completed
                    // In a real application, you would integrate with a payment gateway
                    order.PaymentStatus = "Completed";
                    order.OrderStatus = "Confirmed";

                    // Save payment details based on payment method
                    if (model.PaymentMethod == "Credit Card" || model.PaymentMethod == "Debit Card")
                    {
                        // Store last 4 digits of card number for reference
                        string cardNumber = model.CardNumber?.Replace(" ", "");
                        if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length >= 4)
                        {
                            order.PaymentDetails = $"Card ending in {cardNumber.Substring(cardNumber.Length - 4)}";
                        }
                        else
                        {
                            order.PaymentDetails = "Card payment";
                        }
                    }
                    else if (model.PaymentMethod == "UPI")
                    {
                        order.PaymentDetails = !string.IsNullOrEmpty(model.UPIId) ? $"UPI ID: {model.UPIId}" : "UPI payment";
                    }
                    else if (model.PaymentMethod == "Cash on Delivery")
                    {
                        order.PaymentDetails = "Cash on Delivery";
                    }
                    else
                    {
                        // Fallback for any other payment method
                        order.PaymentDetails = model.PaymentMethod;
                    }

                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    // If user is authenticated, update their information
                    if (User.Identity.IsAuthenticated)
                    {
                        var userEmail = User.Identity.Name;
                        _logger.LogInformation("Updating user information during payment for: {Email}", userEmail);

                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                        if (user != null)
                        {
                            _logger.LogInformation("Found user with ID: {UserId} during payment processing", user.Id);

                            // Always update user information with the latest order details
                            user.PhoneNumber = order.CustomerPhone;
                            user.Address = order.DeliveryAddress;
                            user.City = order.City;
                            user.State = order.State;
                            user.PostalCode = order.PostalCode;

                            _logger.LogInformation("Updated user fields during payment - Phone: {Phone}, Address: {Address}, City: {City}, State: {State}, PostalCode: {PostalCode}",
                                user.PhoneNumber, user.Address, user.City, user.State, user.PostalCode);

                            _context.Update(user);
                            await _context.SaveChangesAsync();

                            // Reload the user from the database to verify the change
                            await _context.Entry(user).ReloadAsync();

                            _logger.LogInformation("User information updated during payment processing - User ID: {UserId}, Phone: {Phone}, Address: {Address}",
                                user.Id, user.PhoneNumber, user.Address);
                        }
                        else
                        {
                            _logger.LogWarning("User not found with email during payment: {Email}", userEmail);
                        }
                    }

                    // Clear the cart
                    HttpContext.Session.Remove("Cart");

                    // Add success message to TempData
                    TempData["SuccessMessage"] = "Payment completed successfully! Your order has been confirmed.";

                    // Redirect to the simple success page
                    return RedirectToAction("PaymentSuccess", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment");
                    ModelState.AddModelError("", "An error occurred while processing your payment. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            model.Order = order;
            model.PaymentMethods = new List<string> { "Credit Card", "Debit Card", "UPI", "Cash on Delivery" };

            return View(model);
        }

        // GET: /Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Verify this order belongs to the current session
            var sessionOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (sessionOrderId != id)
            {
                return RedirectToAction("Index", "Home");
            }

            // Clear the current order from session
            HttpContext.Session.Remove("CurrentOrderId");

            return View(order);
        }

        // GET: /Checkout/OrderSuccess/5
        public async Task<IActionResult> OrderSuccess(int id)
        {
            _logger.LogInformation("OrderSuccess action called for order ID: {OrderId}", id);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Order not found with ID: {OrderId}", id);
                return NotFound();
            }

            _logger.LogInformation("Found order with ID: {OrderId}, Customer: {Customer}, Total: {Total}",
                order.Id, order.CustomerName, order.Total);

            // Clear the current order from session
            HttpContext.Session.Remove("CurrentOrderId");

            // Update user information if authenticated
            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.Identity.Name;
                _logger.LogInformation("Updating user information for: {Email} in OrderSuccess", userEmail);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null)
                {
                    _logger.LogInformation("Found user with ID: {UserId} in OrderSuccess", user.Id);

                    // Update user's contact and address information
                    user.PhoneNumber = order.CustomerPhone;
                    user.Address = order.DeliveryAddress;
                    user.City = order.City;
                    user.State = order.State;
                    user.PostalCode = order.PostalCode;

                    _logger.LogInformation("Updated user fields in OrderSuccess - Phone: {Phone}, Address: {Address}",
                        user.PhoneNumber, user.Address);

                    // Save the changes to the database
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully saved user profile in OrderSuccess - User ID: {UserId}", user.Id);
                }
            }

            return View(order);
        }

        // GET: /Checkout/PaymentSuccess/5
        public async Task<IActionResult> PaymentSuccess(int id)
        {
            _logger.LogInformation("PaymentSuccess action called for order ID: {OrderId}", id);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Order not found with ID: {OrderId}", id);
                return NotFound();
            }

            _logger.LogInformation("Found order with ID: {OrderId}, Customer: {Customer}, Total: {Total}",
                order.Id, order.CustomerName, order.Total);

            // Clear the current order from session
            HttpContext.Session.Remove("CurrentOrderId");

            // Update user information if authenticated
            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.Identity.Name;
                _logger.LogInformation("Updating user information for: {Email} in PaymentSuccess", userEmail);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null)
                {
                    _logger.LogInformation("Found user with ID: {UserId} in PaymentSuccess", user.Id);

                    // Update user's contact and address information
                    user.PhoneNumber = order.CustomerPhone;
                    user.Address = order.DeliveryAddress;
                    user.City = order.City;
                    user.State = order.State;
                    user.PostalCode = order.PostalCode;

                    _logger.LogInformation("Updated user fields in PaymentSuccess - Phone: {Phone}, Address: {Address}",
                        user.PhoneNumber, user.Address);

                    // Save the changes to the database
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully saved user profile in PaymentSuccess - User ID: {UserId}", user.Id);
                }
            }

            return View(order);
        }

        // GET: /Checkout/DirectPayment/5
        public IActionResult DirectPayment(int id)
        {
            _logger.LogInformation("DirectPayment action called for order ID: {OrderId}", id);

            // Store the order ID in session and ViewBag
            HttpContext.Session.SetInt32("CurrentOrderId", id);
            ViewBag.OrderId = id;

            return View();
        }

        // GET: /Checkout/SimplePayment/5
        public async Task<IActionResult> SimplePayment(int id)
        {
            _logger.LogInformation("SimplePayment action called for order ID: {OrderId}", id);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Store the order ID in session
            HttpContext.Session.SetInt32("CurrentOrderId", id);

            var viewModel = new PaymentViewModel
            {
                Order = order,
                PaymentMethods = new List<string> { "Credit Card", "Debit Card", "UPI", "Cash on Delivery" }
            };

            return View(viewModel);
        }

        // GET: /Checkout/Debug
        public IActionResult Debug()
        {
            _logger.LogInformation("Debug action called");

            // Check if user is authenticated
            bool isAuthenticated = User.Identity.IsAuthenticated;
            string userEmail = isAuthenticated ? User.Identity.Name : "Not authenticated";

            // Get cart items from session
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            int cartItemCount = cartItems?.Count ?? 0;

            // Get current order ID from session
            var currentOrderId = HttpContext.Session.GetInt32("CurrentOrderId");

            // Create debug info
            var debugInfo = new
            {
                IsAuthenticated = isAuthenticated,
                UserEmail = userEmail,
                CartItemCount = cartItemCount,
                CurrentOrderId = currentOrderId,
                ServerTime = DateTime.Now
            };

            return Json(debugInfo);
        }
    }

    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public Order Order { get; set; } = new Order();
    }

    public class PaymentViewModel
    {
        public Order Order { get; set; } = new Order();
        public List<string> PaymentMethods { get; set; } = new List<string>();
        public string PaymentMethod { get; set; } = "Credit Card";
        public string CardNumber { get; set; } = string.Empty;
        public string CardholderName { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public string UPIId { get; set; } = string.Empty;
    }

    // Using CartItem from CartController
}
