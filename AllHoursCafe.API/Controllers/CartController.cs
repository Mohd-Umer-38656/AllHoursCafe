using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllHoursCafe.API.Data;
using AllHoursCafe.API.Extensions;
using AllHoursCafe.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AllHoursCafe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/cart/save
        [HttpPost("save")]
        public IActionResult SaveCart([FromBody] List<CartItem> cartItems)
        {
            try
            {
                if (cartItems == null)
                {
                    return BadRequest("Cart items cannot be null");
                }

                // Save cart to session
                HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cartItems));

                return Ok(new { message = "Cart saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving cart to session");
                return StatusCode(500, new { message = "An error occurred while saving the cart" });
            }
        }

        // GET: api/cart
        [HttpGet]
        public IActionResult GetCart()
        {
            try
            {
                // Get cart from session
                var cartJson = HttpContext.Session.GetString("Cart");
                var cartItems = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>()
                    : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart from session");
                return StatusCode(500, new { message = "An error occurred while getting the cart" });
            }
        }

        // DELETE: api/cart
        [HttpDelete]
        public IActionResult ClearCart()
        {
            try
            {
                // Clear cart from session
                HttpContext.Session.Remove("Cart");

                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart from session");
                return StatusCode(500, new { message = "An error occurred while clearing the cart" });
            }
        }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }
}
