using FlightBookingSystem.Configuration;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminFlightRepository _adminFlightRepo;
        private readonly IUserRepository _userRepo;
        private readonly IJwtTokenService _jwtService;

        public AdminService(IAdminFlightRepository adminFlightRepo, IUserRepository userRepo, IJwtTokenService jwtService)
        {
            _adminFlightRepo = adminFlightRepo;
            _userRepo = userRepo;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request)
        {
            var admin = await _adminFlightRepo.ValidateAdminAsync(request.Username, request.Password)
                ?? throw new UnauthorizedAccessException("Invalid admin credentials.");

            var token = _jwtService.GenerateAdminToken(admin.Username);
            return new AuthResponse
            {
                Token = token,
                Message = "Admin login successful.",
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        public async Task<FlightResponse> CreateFlightAsync(FlightCreateRequest request)
        {
            var flight = new Flight
            {
                FlightNumber = request.FlightNumber,
                Origin      = request.Origin,
                Destination = request.Destination,
                FlightDate  = request.FlightDate,
                Fare        = request.Fare,
                AvailableSeats = request.AvailableSeats
            };
            var created = await _adminFlightRepo.AddFlightAsync(flight);
            return MapToFlightResponse(created);
        }

        public async Task<FlightResponse> UpdateFlightAsync(FlightUpdateRequest request)
        {
            var flight = new Flight
            {
                FlightId    = request.FlightId,
                FlightNumber = request.FlightNumber,
                Origin      = request.Origin,
                Destination = request.Destination,
                FlightDate  = request.FlightDate,
                Fare        = request.Fare,
                AvailableSeats = request.AvailableSeats
            };
            var updated = await _adminFlightRepo.UpdateFlightAsync(flight);
            return MapToFlightResponse(updated);
        }

        public async Task DeleteFlightAsync(int flightId)
            => await _adminFlightRepo.DeleteFlightAsync(flightId);

        public async Task<SuccessResponse> UpdateUserAsync(UserUpdateRequest request)
        {
            var user = await _userRepo.GetByIdAsync(request.UserId)
                ?? throw new KeyNotFoundException($"User with ID {request.UserId} not found.");

            user.Username = request.Username;
            user.Email    = request.Email;
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            await _userRepo.UpdateAsync(user);

            return new SuccessResponse { Message = "User updated successfully." };
        }

        private static FlightResponse MapToFlightResponse(Flight f) => new()
        {
            FlightId       = f.FlightId,
            FlightNumber   = f.FlightNumber,
            Origin         = f.Origin,
            Destination    = f.Destination,
            FlightDate     = f.FlightDate,
            Fare           = f.Fare,
            AvailableSeats = f.AvailableSeats
        };
    }
}
