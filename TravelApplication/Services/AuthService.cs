using Microsoft.Maui.ApplicationModel.Communication;
using TravelApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelApplication.Services
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public class AuthService
    {
        private readonly HttpClient httpClient = ServiceHelper.GetService<HttpClient>();
        
        public async Task<AuthResult> RegisterAsync(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                return new AuthResult { Success = false, ErrorMessage = "All fields are required." };
            }
            if (password != confirmPassword)
            {
                return new AuthResult { Success = false, ErrorMessage = "Passwords do not match." };
            }
            var payload = new
            {
                Username = username,
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/auth/register", content);

            return response.IsSuccessStatusCode
                ? new AuthResult { Success = true }
                : new AuthResult { Success = false, ErrorMessage = "Registration failed." };
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResult { Success = false, ErrorMessage = "Email/Username and password cannot be empty." };
            }
            var payload = new
            {
                Username = email,
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null && !string.IsNullOrEmpty(result.Token) && result.User != null)
                {
                    await SecureStorage.SetAsync("token", result.Token);
                    await SecureStorage.SetAsync("userID", result.User.UserId.ToString());
                    await SecureStorage.SetAsync("username", result.User.Username ?? string.Empty);
                    await SecureStorage.SetAsync("email", result.User.Email ?? string.Empty);
                    await SecureStorage.SetAsync("coverImage", result.User.CoverImage ?? string.Empty);
                    await SecureStorage.SetAsync("role", result.User.Role ?? "user");

                    return new AuthResult { Success = true };
                }
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return new AuthResult { Success = false, ErrorMessage = errorMessage };
        }

        public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            try
            {
                var userId = await SecureStorage.GetAsync("userID");
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                var payload = new
                {
                    UserId = int.Parse(userId),
                    OldPassword = oldPassword,
                    NewPassword = newPassword
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/auth/change-password", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                return false;
            }
        }
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? CoverImage { get; set; }
        public string? Role { get; set; }
    }
}
