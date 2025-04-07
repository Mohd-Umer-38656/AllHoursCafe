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
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the return URL in TempData
                TempData["ReturnUrl"] = "/Checkout";

                // Redirect to login page
                return RedirectToAction("Login", "Auth");
            }
            // Form validation is now working correctly

            if (ModelState.IsValid)
            {
                try
                {
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
                        PaymentStatus = "Pending",
                        OrderStatus = "Pending",
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

                    // Save order to database
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Store order ID in session for payment page
                    HttpContext.Session.SetInt32("CurrentOrderId", order.Id);

                    // Redirect to payment page
                    return RedirectToAction("Payment", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing checkout");
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

            var viewModel = new PaymentViewModel
            {
                Order = order,
                PaymentMethods = new List<string> { "Credit Card", "Debit Card", "UPI", "Cash on Delivery" }
            };

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

                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    // Clear the cart
                    HttpContext.Session.Remove("Cart");

                    // Redirect to confirmation page
                    return RedirectToAction("Confirmation", new { id = order.Id });
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
    }

    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public Order Order { get; set; }
    }

    public class PaymentViewModel
    {
        public Order Order { get; set; }
        public List<string> PaymentMethods { get; set; }
        public string PaymentMethod { get; set; }
        public string CardNumber { get; set; }
        public string CardholderName { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
        public string UPIId { get; set; }
    }

    // Using CartItem from CartController
}
