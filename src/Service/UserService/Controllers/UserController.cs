using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.DTOs;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ổ KHÓA: Phải có JWT Token mới được gọi bất kỳ hàm nào trong class này!
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;

        public UserController(UserDbContext context)
        {
            _context = context;
        }

        // 1. API Xem thông tin cá nhân
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Lấy ID của user từ trong JWT Token (Token do chính mình tự giải mã)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var account = await _context.Accounts.FindAsync(userId);
            if (account == null) return NotFound(new { Message = "Không tìm thấy user" });

            return Ok(new
            {
                account.Id,
                account.Phone,
                account.FullName,
                account.AvatarUrl,
                account.Role,
                account.Status
            });
        }

        // 2. API Cập nhật thông tin cá nhân
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var account = await _context.Accounts.FindAsync(userId);
            if (account == null) return NotFound();

            // Cập nhật dữ liệu
            account.FullName = request.FullName;
            account.AvatarUrl = request.AvatarUrl;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật hồ sơ thành công!" });
        }
    }
}