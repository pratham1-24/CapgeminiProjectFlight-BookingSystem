using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Models.DTOs.Requests
{
    public class FlightSearchRequest
    {
        [Required]
        public string Origin { get; set; } = string.Empty;

        [Required]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime FlightDate { get; set; }
    }

    public class FlightCreateRequest
    {
        [Required]
        [MaxLength(10)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Origin { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime FlightDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fare must be greater than 0")]
        public decimal Fare { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Available seats must be greater than 0")]
        public int AvailableSeats { get; set; } = 150;
    }

    public class FlightUpdateRequest
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        [MaxLength(10)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Origin { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime FlightDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Fare { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableSeats { get; set; }
    }

    public class FareRequest
    {
        [Required]
        public string FlightNumber { get; set; } = string.Empty;
    }
}
