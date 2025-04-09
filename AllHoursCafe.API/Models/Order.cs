using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AllHoursCafe.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50, ErrorMessage = "State cannot be longer than 50 characters")]
        public string State { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot be longer than 20 characters")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [StringLength(500, ErrorMessage = "Special instructions cannot be longer than 500 characters")]
        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; }

        [Required(ErrorMessage = "Order type is required")]
        [Display(Name = "Order Type")]
        public string OrderType { get; set; } // Delivery or Pickup

        [Display(Name = "Delivery Time")]
        public DateTime? DeliveryTime { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal Tax { get; set; }

        public decimal Total { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "Pending";

        [Required(ErrorMessage = "Payment status is required")]
        public string PaymentStatus { get; set; } = "Pending";

        [Required(ErrorMessage = "Order status is required")]
        public string OrderStatus { get; set; } = "Pending";

        // Adding PaymentDetails back with a default value to match the database schema
        public string PaymentDetails { get; set; } = "None";

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int MenuItemId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal Total { get; set; }

        public Order Order { get; set; }
    }
}
