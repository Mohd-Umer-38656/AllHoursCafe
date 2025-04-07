using System;
using System.ComponentModel.DataAnnotations;

namespace AllHoursCafe.API.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Number of guests is required")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; }
        
        [Required(ErrorMessage = "Reservation date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Reservation Date")]
        public DateTime ReservationDate { get; set; }
        
        [Required(ErrorMessage = "Reservation time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Reservation Time")]
        public DateTime ReservationTime { get; set; }
        
        [StringLength(500, ErrorMessage = "Special requests cannot be longer than 500 characters")]
        [Display(Name = "Special Requests")]
        public string SpecialRequests { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsConfirmed { get; set; } = false;
    }
}
