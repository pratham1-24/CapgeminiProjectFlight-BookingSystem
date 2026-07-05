using FlightBookingSystem.Configuration;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtTokenService _jwtService;
        private readonly IEmailService _emailService;

        public AuthService(IAuthRepository authRepo, IJwtTokenService jwtService, IEmailService emailService)
        {
            _authRepo = authRepo;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        public async Task<AuthResponse> RegisterAsync(UserRegisterRequest request)
        {
            if (await _authRepo.UsernameExistsAsync(request.Username))
                throw new ArgumentException("Username already exists.");

            if (await _authRepo.EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already registered.");

            // BCrypt hash the password — never store plain text
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var customer = await _authRepo.RegisterCustomerAsync(request.Username, request.Email, hashedPassword);

            // Send welcome email (non-blocking — won't fail registration if email fails)
            _ = _emailService.SendWelcomeEmailAsync(customer.Email, customer.Username);

            var token = _jwtService.GenerateCustomerToken(customer.Id, customer.Username);
            return new AuthResponse
            {
                Token = token,
                Message = "User registered successfully.",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<AuthResponse> LoginAsync(UserLoginRequest request)
        {
            var customer = await _authRepo.ValidateCustomerAsync(request.Username, request.Password)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            // Verify password against BCrypt hash
            if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.Password))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var token = _jwtService.GenerateCustomerToken(customer.Id, customer.Username);
            return new AuthResponse
            {
                Token = token,
                Message = "Login successful.",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }
    }
}
