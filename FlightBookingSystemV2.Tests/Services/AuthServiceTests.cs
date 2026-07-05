using Moq;
using NUnit.Framework;
using FlightBookingSystem.Configuration;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Implementations;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Tests.Services
{
    /// <summary>
    /// Unit tests for AuthService.
    /// Tests: Register (success, duplicate username, duplicate email)
    ///        Login    (success, wrong password, user not found)
    /// Verifies BCrypt password hashing and verification.
    /// </summary>
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> _mockAuthRepo    = null!;
        private Mock<IJwtTokenService> _mockJwtService = null!;
        private Mock<IEmailService>    _mockEmail      = null!;
        private AuthService            _authService    = null!;

        [SetUp]
        public void SetUp()
        {
            _mockAuthRepo   = new Mock<IAuthRepository>();
            _mockJwtService = new Mock<IJwtTokenService>();
            _mockEmail      = new Mock<IEmailService>();
            _authService    = new AuthService(_mockAuthRepo.Object, _mockJwtService.Object, _mockEmail.Object);
        }

        // ── REGISTER ──────────────────────────────────────────────────────────

        [Test]
        public async Task Register_NewUser_ReturnsTokenAndSuccessMessage()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                Username = "john",
                Email    = "john@gmail.com",
                Password = "Pass@123"
            };
            _mockAuthRepo.Setup(r => r.UsernameExistsAsync("john")).ReturnsAsync(false);
            _mockAuthRepo.Setup(r => r.EmailExistsAsync("john@gmail.com")).ReturnsAsync(false);
            _mockAuthRepo
                .Setup(r => r.RegisterCustomerAsync("john", "john@gmail.com", It.IsAny<string>()))
                .ReturnsAsync(new Customer { Id = 1, Username = "john", Email = "john@gmail.com" });
            _mockJwtService
                .Setup(j => j.GenerateCustomerToken(1, "john"))
                .Returns("fake.jwt.token");

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.That(result.Token,   Is.EqualTo("fake.jwt.token"));
            Assert.That(result.Message, Is.EqualTo("User registered successfully."));

            // Verify BCrypt hash was passed (not plain text)
            _mockAuthRepo.Verify(r => r.RegisterCustomerAsync(
                "john",
                "john@gmail.com",
                It.Is<string>(p => p != "Pass@123")), // must NOT be plain text
                Times.Once);
        }

        [Test]
        public async Task Register_PasswordIsHashed_NotStoredAsPlainText()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                Username = "alice",
                Email    = "alice@gmail.com",
                Password = "MySecret@123"
            };
            _mockAuthRepo.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockAuthRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            string? capturedHash = null;
            _mockAuthRepo
                .Setup(r => r.RegisterCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((u, e, p) => capturedHash = p)
                .ReturnsAsync(new Customer { Id = 2, Username = "alice", Email = "alice@gmail.com" });
            _mockJwtService.Setup(j => j.GenerateCustomerToken(It.IsAny<int>(), It.IsAny<string>())).Returns("token");

            // Act
            await _authService.RegisterAsync(request);

            // Assert — captured hash must be a BCrypt hash (starts with $2a$ or $2b$)
            Assert.That(capturedHash, Is.Not.Null);
            Assert.That(capturedHash, Does.StartWith("$2"));
            Assert.That(capturedHash, Is.Not.EqualTo("MySecret@123")); // NEVER plain text
            Assert.That(BCrypt.Net.BCrypt.Verify("MySecret@123", capturedHash!), Is.True);
        }

        [Test]
        public void Register_DuplicateUsername_ThrowsArgumentException()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                Username = "existinguser",
                Email    = "new@gmail.com",
                Password = "Pass@123"
            };
            _mockAuthRepo.Setup(r => r.UsernameExistsAsync("existinguser")).ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _authService.RegisterAsync(request));
            Assert.That(ex!.Message, Is.EqualTo("Username already exists."));
        }

        [Test]
        public void Register_DuplicateEmail_ThrowsArgumentException()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                Username = "newuser",
                Email    = "existing@gmail.com",
                Password = "Pass@123"
            };
            _mockAuthRepo.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockAuthRepo.Setup(r => r.EmailExistsAsync("existing@gmail.com")).ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _authService.RegisterAsync(request));
            Assert.That(ex!.Message, Is.EqualTo("Email already registered."));
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        [Test]
        public async Task Login_CorrectPassword_ReturnsToken()
        {
            // Arrange — store a BCrypt hash as if it came from the database
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Pass@123");
            var request = new UserLoginRequest { Username = "john", Password = "Pass@123" };

            _mockAuthRepo
                .Setup(r => r.ValidateCustomerAsync("john", "Pass@123"))
                .ReturnsAsync(new Customer
                {
                    Id       = 1,
                    Username = "john",
                    Password = hashedPassword
                });
            _mockJwtService.Setup(j => j.GenerateCustomerToken(1, "john")).Returns("fake.token");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.That(result.Token,   Is.EqualTo("fake.token"));
            Assert.That(result.Message, Is.EqualTo("Login successful."));
        }

        [Test]
        public void Login_WrongPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange — hash is for "Pass@123" but user types "WrongPass"
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Pass@123");
            var request = new UserLoginRequest { Username = "john", Password = "WrongPass" };

            _mockAuthRepo
                .Setup(r => r.ValidateCustomerAsync("john", "WrongPass"))
                .ReturnsAsync(new Customer
                {
                    Id       = 1,
                    Username = "john",
                    Password = hashedPassword
                });

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _authService.LoginAsync(request));
            Assert.That(ex!.Message, Is.EqualTo("Invalid credentials."));
        }

        [Test]
        public void Login_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange — repository returns null (user doesn't exist)
            var request = new UserLoginRequest { Username = "ghost", Password = "Pass@123" };
            _mockAuthRepo
                .Setup(r => r.ValidateCustomerAsync("ghost", "Pass@123"))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _authService.LoginAsync(request));
            Assert.That(ex!.Message, Is.EqualTo("Invalid credentials."));
        }
    }
}
