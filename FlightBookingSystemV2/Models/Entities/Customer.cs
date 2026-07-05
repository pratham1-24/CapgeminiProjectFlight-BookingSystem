using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Models.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty; // BCrypt hashed

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
