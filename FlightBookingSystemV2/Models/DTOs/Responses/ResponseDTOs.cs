namespace FlightBookingSystem.Models.DTOs.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class FlightResponse
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime FlightDate { get; set; }
        public decimal Fare { get; set; }
        public int AvailableSeats { get; set; }
    }

    public class FareResponse
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime FlightDate { get; set; }
        public decimal BaseFare { get; set; }
        public decimal GstAmount { get; set; }
        public decimal FinalFare { get; set; }
    }

    public class BookingResponse
    {
        public int BookingId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime FlightDate { get; set; }
        public decimal BaseFare { get; set; }
        public decimal FinalFare { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public bool IsCheckedIn { get; set; }
        public string? SeatNumber { get; set; }
        public DateTime? CheckInDate { get; set; }
    }

    public class CheckInResponse
    {
        public int CheckInId { get; set; }
        public string CheckInReference { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string CheckInStatus { get; set; } = "Completed";
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime FlightDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public string Message { get; set; } = "Check-in successful!";
    }

    public class CheckInStatusResponse
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public bool IsCheckedIn { get; set; }
        public string? SeatNumber { get; set; }
        public DateTime? CheckInDate { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string CheckInStatus { get; set; } = string.Empty;
    }

    public class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class SuccessResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
