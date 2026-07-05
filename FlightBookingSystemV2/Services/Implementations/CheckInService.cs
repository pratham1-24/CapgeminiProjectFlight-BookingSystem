using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Services.Implementations
{
    public class CheckInService : ICheckInService
    {
        private readonly IBookingRepository  _bookingRepo;
        private readonly ICheckInRepository  _checkInRepo;
        private readonly IUserRepository     _userRepo;
        private readonly IEmailService       _emailService;

        public CheckInService(IBookingRepository bookingRepo, ICheckInRepository checkInRepo,
            IUserRepository userRepo, IEmailService emailService)
        {
            _bookingRepo  = bookingRepo;
            _checkInRepo  = checkInRepo;
            _userRepo     = userRepo;
            _emailService = emailService;
        }

        public async Task<BookingResponse> SearchBookingAsync(string referenceNumber)
        {
            var booking = await _bookingRepo.GetByReferenceAsync(referenceNumber)
                ?? throw new KeyNotFoundException($"Booking '{referenceNumber}' not found.");

            return MapToBookingResponse(booking);
        }

        public async Task<CheckInResponse> PerformCheckInAsync(CheckInRequest request)
        {
            var booking = await _bookingRepo.GetByReferenceAsync(request.ReferenceNumber)
                ?? throw new KeyNotFoundException($"Booking '{request.ReferenceNumber}' not found.");

            if (booking.IsCheckedIn)
                throw new ArgumentException("Passenger is already checked in.");

            if (booking.BookingStatus == "Cancelled")
                throw new ArgumentException("Cannot check in a cancelled booking.");

            var seatNumber   = GenerateSeatNumber();
            var checkInDate  = DateTime.UtcNow;
            var checkInRef   = $"CI{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            // Update booking record
            await _checkInRepo.UpdateBookingCheckInAsync(booking.BookingId, seatNumber, checkInDate);

            // Create CheckIn record
            var checkIn = new CheckIn
            {
                BookingId        = booking.BookingId,
                SeatNumber       = seatNumber,
                CheckInReference = checkInRef,
                CheckInStatus    = "Completed"
            };
            var created = await _checkInRepo.CreateAsync(checkIn);

            // Send check-in confirmation email
            var customer = await _userRepo.GetByIdAsync(booking.CustomerId);
            if (customer != null)
            {
                _ = _emailService.SendCheckInConfirmationEmailAsync(
                    customer.Email,
                    $"{booking.FirstName} {booking.LastName}",
                    checkInRef,
                    seatNumber,
                    booking.Flight!.FlightNumber,
                    booking.Flight.Origin,
                    booking.Flight.Destination,
                    booking.Flight.FlightDate);
            }

            return new CheckInResponse
            {
                CheckInId        = created.CheckInId,
                CheckInReference = checkInRef,
                BookingReference = booking.ReferenceNumber,
                SeatNumber       = seatNumber,
                CheckInStatus    = "Completed",
                PassengerName    = $"{booking.FirstName} {booking.LastName}",
                FlightNumber     = booking.Flight!.FlightNumber,
                Origin           = booking.Flight.Origin,
                Destination      = booking.Flight.Destination,
                FlightDate       = booking.Flight.FlightDate,
                CheckInDate      = checkInDate,
                Message          = "Check-in successful!"
            };
        }

        public async Task<CheckInStatusResponse> GetCheckInStatusAsync(string referenceNumber)
        {
            var booking = await _bookingRepo.GetByReferenceAsync(referenceNumber)
                ?? throw new KeyNotFoundException($"Booking '{referenceNumber}' not found.");

            return new CheckInStatusResponse
            {
                ReferenceNumber = booking.ReferenceNumber,
                IsCheckedIn     = booking.IsCheckedIn,
                SeatNumber      = booking.SeatNumber,
                CheckInDate     = booking.CheckInDate,
                PassengerName   = $"{booking.FirstName} {booking.LastName}",
                FlightNumber    = booking.Flight?.FlightNumber ?? string.Empty,
                CheckInStatus   = booking.BookingStatus
            };
        }

        private static string GenerateSeatNumber()
        {
            var row = new Random().Next(1, 36);
            var col = (char)('A' + new Random().Next(0, 6));
            return $"{row}{col}";
        }

        private static BookingResponse MapToBookingResponse(Booking b) => new()
        {
            BookingId       = b.BookingId,
            ReferenceNumber = b.ReferenceNumber,
            FlightId        = b.Flight!.FlightId,
            FlightNumber    = b.Flight.FlightNumber,
            Origin          = b.Flight.Origin,
            Destination     = b.Flight.Destination,
            FlightDate      = b.Flight.FlightDate,
            BaseFare        = b.BaseFare,
            FinalFare       = b.FinalFare,
            FirstName       = b.FirstName,
            LastName        = b.LastName,
            Gender          = b.Gender,
            BookingDate     = b.BookingDate,
            BookingStatus   = b.BookingStatus,
            IsCheckedIn     = b.IsCheckedIn,
            SeatNumber      = b.SeatNumber,
            CheckInDate     = b.CheckInDate
        };
    }
}
