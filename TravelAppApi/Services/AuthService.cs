using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TravelAppApi.Data;
using TravelAppApi.Models;

namespace TravelAppApi.Services
{
    // interface này định nghĩa các phương thức liên quan đến xác thực người dùng
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string username, string email, string password);
        Task<AuthResult> LoginAsync(string username, string email, string password);
        Task<AuthResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }

    // kết quả trả về từ các thao tác xác thực
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public User? User { get; set; }
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Đăng ký người dùng mới
        // Kiểm tra email tồn tại, hash mật khẩu với SHA256, lưu vào database
        public async Task<AuthResult> RegisterAsync(string username, string email, string password)
        {
            if (_context.Users.Any(u => u.Email == email))
                return new AuthResult { Success = false, ErrorMessage = "Email đã tồn tại." };

            //hash + salt mật khẩu
            string passwordHash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                passwordHash = System.Convert.ToBase64String(hash);
            }

            //tạo người dùng mới và lưu vào cơ sở dữ liệu
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                Role = "User",
                AvatarUrl = ""
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new AuthResult { Success = true };
        }

        // Đăng nhập người dùng
        // Tìm user theo username hoặc email, kiểm tra mật khẩu, tạo JWT token
        public async Task<AuthResult> LoginAsync(string username, string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Email, username hoặc mật khẩu không đúng." };
            
            // Hash mật khẩu nhập vào để so sánh
            string passwordHash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                passwordHash = System.Convert.ToBase64String(hash);
            }
            if (user.PasswordHash != passwordHash)
                return new AuthResult { Success = false, ErrorMessage = "Email hoặc mật khẩu không đúng." };

            // Tạo JWT token
            JwtHelper jwtHelper = new JwtHelper(_configuration);
            var token = jwtHelper.GenerateToken(user);
            // Ẩn thông tin nhạy cảm trước khi trả về
            user.PasswordHash = null;
            return new AuthResult { Success = true, Token = token, User = user };
        }

        // Đổi mật khẩu
        // Xác minh mật khẩu cũ, hash mật khẩu mới và cập nhật
        public async Task<AuthResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            // Verify old password using SHA256
            string oldPasswordHash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(oldPassword);
                var hash = sha.ComputeHash(bytes);
                oldPasswordHash = System.Convert.ToBase64String(hash);
            }

            if (user.PasswordHash != oldPasswordHash)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Old password is incorrect"
                };
            }

            // Hash new password
            string newPasswordHash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(newPassword);
                var hash = sha.ComputeHash(bytes);
                newPasswordHash = System.Convert.ToBase64String(hash);
            }

            user.PasswordHash = newPasswordHash;
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Success = true
            };
        }
    }
}
