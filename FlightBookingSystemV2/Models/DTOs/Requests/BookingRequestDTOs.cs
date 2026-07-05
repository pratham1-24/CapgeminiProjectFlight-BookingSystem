using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Models.DTOs.Requests
{
    public class BookingCreateRequest
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;
    }

    public class BookingSearchRequest
    {
        [Required]
        public string ReferenceNumber { get; set; } = string.Empty;
    }

    public class CheckInRequest
    {
        [Required]
        public string ReferenceNumber { get; set; } = string.Empty;
    }

    public class UserUpdateRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
