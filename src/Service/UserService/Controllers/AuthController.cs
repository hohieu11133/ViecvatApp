using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Data;
using UserService.DTOs;
using UserService.Entities;
using UserService.Messaging;
namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly KafkaProducerService _kafkaProducer;
        // Constructor: .NET sẽ tự động tiêm DbContext và Cấu hình vào đây
        public AuthController(UserDbContext context, IConfiguration configuration, KafkaProducerService kafkaProducer)
        {
            _context = context;
            _configuration = configuration;
            _kafkaProducer = kafkaProducer; // Thêm dòng này
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // 1. Kiểm tra xem SĐT đã tồn tại chưa
            var existingUser = await _context.Accounts.FirstOrDefaultAsync(a => a.Phone == request.Phone);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "Số điện thoại đã được sử dụng!" });
            }

            // 2. Tạo tài khoản mới với mật khẩu đã được băm (Hash)
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            // 3. Nếu role là WORKER, tạo thêm thông tin rỗng trong bảng Workers
            if (request.Role == "WORKER")
            {
                var worker = new Worker { AccountId = account.Id };
                _context.Workers.Add(worker);
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            var userRegisteredEvent = new
            {
                AccountId = account.Id,
                Phone = account.Phone,
                Role = account.Role,
                RegisteredAt = DateTime.UtcNow
            };
            await _kafkaProducer.ProduceAsync("user-registered-topic", userRegisteredEvent);
            return Ok(new { Message = "Đăng ký thành công!", AccountId = account.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // 1. Tìm user theo số điện thoại
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Phone == request.Phone);
            if (account == null)
            {
                return Unauthorized(new { Message = "Sai số điện thoại hoặc mật khẩu!" });
            }

            // 2. Kiểm tra mật khẩu
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Sai số điện thoại hoặc mật khẩu!" });
            }

            // 3. Tạo JWT Token
            var token = GenerateJwtToken(account);

            return Ok(new { Token = token, Role = account.Role });
        }

        // Hàm hỗ trợ tạo JWT Token
        private string GenerateJwtToken(Account account)
        {
            // Lấy chuỗi bí mật từ appsettings.json (Lát nữa ta sẽ cấu hình sau)
            var jwtSecret = _configuration["JwtSettings:SecretKey"] ?? "DayLaChuoiBiMatSieuDaiVaPhucTapChoViecVatApp2026!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Gói thông tin (Claims) vào trong Token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim("phone", account.Phone),
                new Claim("role", account.Role)
            };

            var token = new JwtSecurityToken(
                issuer: "ViecVatApp",
                audience: "ViecVatUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token sống được 7 ngày
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}