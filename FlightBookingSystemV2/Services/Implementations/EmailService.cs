using System.Net;
using System.Net.Mail;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private SmtpClient CreateSmtpClient()
        {
            var smtp = _config.GetSection("SmtpSettings");
            return new SmtpClient(smtp["Host"])
            {
                Port = int.Parse(smtp["Port"]!),
                Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                EnableSsl = bool.Parse(smtp["EnableSsl"]!)
            };
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var fromAddress = _config["SmtpSettings:FromAddress"]!;
                using var client = CreateSmtpClient();
                var mail = new MailMessage(fromAddress, toEmail, subject, body)
                {
                    IsBodyHtml = true
                };
                await client.SendMailAsync(mail);
                _logger.LogInformation("Email sent to {Email} — Subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                // Log but don't fail the request if email fails
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string username)
        {
            var subject = "Welcome to Flight Booking System";
            var body = $@"
                <h2>Welcome, {username}!</h2>
                <p>Your account has been created successfully at <strong>Flight Booking System</strong>.</p>
                <p>You can now search and book flights, and check in online.</p>
                <br/>
                <p>Happy Flying! ✈️</p>
                <p><strong>Flight Booking System Team</strong></p>";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendBookingConfirmationEmailAsync(string toEmail, string passengerName,
            string bookingReference, string flightNumber,
            string origin, string destination, DateTime flightDate, decimal finalFare)
        {
            var subject = $"Booking Confirmed — {bookingReference}";
            var body = $@"
                <h2>Booking Confirmed! ✅</h2>
                <p>Dear <strong>{passengerName}</strong>,</p>
                <p>Your flight booking is confirmed. Here are your details:</p>
                <table border='1' cellpadding='8' cellspacing='0'>
                    <tr><td><strong>Booking Reference</strong></td><td>{bookingReference}</td></tr>
                    <tr><td><strong>Flight Number</strong></td><td>{flightNumber}</td></tr>
                    <tr><td><strong>From</strong></td><td>{origin}</td></tr>
                    <tr><td><strong>To</strong></td><td>{destination}</td></tr>
                    <tr><td><strong>Date</strong></td><td>{flightDate:dd MMM yyyy}</td></tr>
                    <tr><td><strong>Total Fare</strong></td><td>₹{finalFare:F2}</td></tr>
                </table>
                <br/>
                <p>Please check in online before your flight.</p>
                <p><strong>Flight Booking System Team</strong></p>";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendCheckInConfirmationEmailAsync(string toEmail, string passengerName,
            string checkInReference, string seatNumber,
            string flightNumber, string origin, string destination, DateTime flightDate)
        {
            var subject = $"Check-in Confirmed — {checkInReference}";
            var body = $@"
                <h2>Check-in Confirmed! 🛫</h2>
                <p>Dear <strong>{passengerName}</strong>,</p>
                <p>You have successfully checked in. Here are your details:</p>
                <table border='1' cellpadding='8' cellspacing='0'>
                    <tr><td><strong>Check-in Reference</strong></td><td>{checkInReference}</td></tr>
                    <tr><td><strong>Seat Number</strong></td><td><strong style='font-size:18px'>{seatNumber}</strong></td></tr>
                    <tr><td><strong>Flight Number</strong></td><td>{flightNumber}</td></tr>
                    <tr><td><strong>From</strong></td><td>{origin}</td></tr>
                    <tr><td><strong>To</strong></td><td>{destination}</td></tr>
                    <tr><td><strong>Date</strong></td><td>{flightDate:dd MMM yyyy}</td></tr>
                </table>
                <br/>
                <p>Please arrive at the airport at least 2 hours before departure.</p>
                <p><strong>Flight Booking System Team</strong></p>";
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
