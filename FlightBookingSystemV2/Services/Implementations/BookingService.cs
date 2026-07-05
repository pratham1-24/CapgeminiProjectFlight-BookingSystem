using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IFlightRepository  _flightRepo;
        private readonly IUserRepository    _userRepo;
        private readonly IEmailService      _emailService;
        private const decimal GstRate = 0.18m;

        public BookingService(IBookingRepository bookingRepo, IFlightRepository flightRepo,
            IUserRepository userRepo, IEmailService emailService)
        {
            _bookingRepo  = bookingRepo;
            _flightRepo   = flightRepo;
            _userRepo     = userRepo;
            _emailService = emailService;
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingCreateRequest request, int customerId)
        {
            // Validate flight exists and has seats
            var flight = await _flightRepo.GetByIdAsync(request.FlightId)
                ?? throw new KeyNotFoundException($"Flight with ID {request.FlightId} not found.");

            if (flight.AvailableSeats <= 0)
                throw new InvalidOperationException("No available seats on this flight.");

            var baseFare  = flight.Fare;
            var finalFare = Math.Round(baseFare + (baseFare * GstRate), 2);

            var booking = new Booking
            {
                ReferenceNumber = GenerateReference(),
                FlightId        = request.FlightId,
                CustomerId      = customerId,
                FirstName       = request.FirstName,
                LastName        = request.LastName,
                Gender          = request.Gender,
                BookingDate     = DateTime.UtcNow,
                BookingStatus   = "Confirmed",
                BaseFare        = baseFare,
                FinalFare       = finalFare
            };

            var created = await _bookingRepo.CreateAsync(booking);

            // Decrement available seats
            await _flightRepo.DecrementSeatsAsync(request.FlightId);

            // Send confirmation email
            var customer = await _userRepo.GetByIdAsync(customerId);
            if (customer != null)
            {
                _ = _emailService.SendBookingConfirmationEmailAsync(
                    customer.Email,
                    $"{request.FirstName} {request.LastName}",
                    created.ReferenceNumber,
                    flight.FlightNumber,
                    flight.Origin,
                    flight.Destination,
                    flight.FlightDate,
                    finalFare);
            }

            return MapToBookingResponse(created, flight);
        }

        public async Task<BookingResponse> GetBookingByReferenceAsync(string referenceNumber)
        {
            var booking = await _bookingRepo.GetByReferenceAsync(referenceNumber)
                ?? throw new KeyNotFoundException($"Booking '{referenceNumber}' not found.");
            return MapToBookingResponse(booking, booking.Flight!);
        }

        private static string GenerateReference()
            => $"BK{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        private static BookingResponse MapToBookingResponse(Booking b, Models.Entities.Flight f) => new()
        {
            BookingId       = b.BookingId,
            ReferenceNumber = b.ReferenceNumber,
            FlightId        = f.FlightId,
            FlightNumber    = f.FlightNumber,
            Origin          = f.Origin,
            Destination     = f.Destination,
            FlightDate      = f.FlightDate,
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
