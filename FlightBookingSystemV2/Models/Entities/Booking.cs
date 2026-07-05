using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBookingSystem.Models.Entities
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [ForeignKey("Flight")]
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string BookingStatus { get; set; } = "Confirmed"; // Confirmed / Cancelled / CheckedIn

        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseFare { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FinalFare { get; set; } // BaseFare + GST (18%)

        public bool IsCheckedIn { get; set; } = false;

        [MaxLength(10)]
        public string? SeatNumber { get; set; }

        public DateTime? CheckInDate { get; set; }

        public CheckIn? CheckIn { get; set; }
    }
}
