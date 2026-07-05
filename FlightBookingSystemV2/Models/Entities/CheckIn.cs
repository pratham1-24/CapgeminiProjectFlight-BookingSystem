using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBookingSystem.Models.Entities
{
    public class CheckIn
    {
        [Key]
        public int CheckInId { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        [Required]
        [MaxLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string CheckInReference { get; set; } = string.Empty;

        [MaxLength(20)]
        public string CheckInStatus { get; set; } = "Completed"; // Completed / Pending
    }
}
