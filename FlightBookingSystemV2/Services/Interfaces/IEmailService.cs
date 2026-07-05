namespace FlightBookingSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string username);
        Task SendBookingConfirmationEmailAsync(string toEmail, string passengerName,
            string bookingReference, string flightNumber,
            string origin, string destination, DateTime flightDate, decimal finalFare);
        Task SendCheckInConfirmationEmailAsync(string toEmail, string passengerName,
            string checkInReference, string seatNumber,
            string flightNumber, string origin, string destination, DateTime flightDate);
    }
}
