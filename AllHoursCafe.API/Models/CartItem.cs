using System;

namespace AllHoursCafe.API.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string SessionId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
