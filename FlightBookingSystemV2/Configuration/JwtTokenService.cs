using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FlightBookingSystem.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer    { get; set; } = string.Empty;
        public string Audience  { get; set; } = string.Empty;
    }

    public interface IJwtTokenService
    {
        string GenerateCustomerToken(int customerId, string username);
        string GenerateAdminToken(string username);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(JwtSettings settings) => _settings = settings;

        public string GenerateCustomerToken(int customerId, string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            return GenerateToken(claims, TimeSpan.FromHours(1));
        }

        public string GenerateAdminToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            return GenerateToken(claims, TimeSpan.FromHours(2));
        }

        private string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiry)
        {
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _settings.Issuer,
                audience:           _settings.Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.Add(expiry),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
