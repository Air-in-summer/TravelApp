using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TravelAppApi.Models;
using TravelAppApi.Services;

namespace TravelAppApi.Controllers
{
    // controller kiểm soát việc đăng ký, đăng nhập và đổi pass
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // đầu vào của request gồm 3 thành phần
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username, Email và Password không được để trống.");
            }

            var result = await _authService.RegisterAsync(request.Username!, request.Email!, request.Password!);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // đầu vào của request gồm 2 thành phần
            var result = await _authService.LoginAsync(request.Username, request.Email, request.Password);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return Ok(new { token = result.Token, user = result.User });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] TravelAppApi.Data.ChangePasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("Old password and new password are required.");
            }

            if (request.UserId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var result = await _authService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(new { message = "Password changed successfully" });
        }

    }

    // class hỗ trợ gửi request / trả về token
    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
