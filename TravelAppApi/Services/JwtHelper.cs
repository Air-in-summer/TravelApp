using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TravelAppApi.Models;

namespace TravelAppApi.Services
{
    public class JwtHelper
    {
        // Nên lưu ở cấu hình thực tế
        private readonly IConfiguration config;

        public JwtHelper(IConfiguration _config)
        {
            config = _config;
        }
        public string GenerateToken(User user)
        {
            var jwtKey = config["Jwt:Key"] ?? "super_secret_key";
            var jwtIssuer = config["Jwt:Issuer"] ?? "MusicAppIssuer";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                //claim user_id và các thông tin khác cần thiết
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Username", user.Username ?? string.Empty),
                new Claim("email", user.Email ?? string.Empty),
                new Claim("Role", user.Role ?? "user"),
            };
            Console.WriteLine($"Generating token for user: {user.Username}, UserId: {user.UserId}, Email: {user.Email}, Role: {user.Role}");

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Thời gian hết hạn token
                signingCredentials: credentials
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;

        }
    }
}
