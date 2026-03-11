using System.ComponentModel.DataAnnotations;

namespace JobService.DTOs
{
    // Dữ liệu Khách hàng gửi lên khi Đăng việc
    public class CreateJobRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public double Latitude { get; set; } // Vĩ độ nhà khách

        [Required]
        public double Longitude { get; set; } // Kinh độ nhà khách
    }
}