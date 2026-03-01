using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    // Dữ liệu App gửi lên khi Đăng ký
    public class RegisterRequest
    {
        [Required]
        public string Phone { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = "CUSTOMER"; // Mặc định là khách hàng
    }

    // Dữ liệu App gửi lên khi Đăng nhập
    public class LoginRequest
    {
        [Required]
        public string Phone { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}