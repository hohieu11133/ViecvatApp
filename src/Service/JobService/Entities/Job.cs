using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace JobService.Entities
{
    public class Job
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CustomerId { get; set; } // ID của Khách hàng đặt việc

        public Guid? WorkerId { get; set; } // ID của Thợ nhận việc (Lúc mới đăng thì chưa có ai nhận nên để dấu ? cho phép rỗng)

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!; // Tên công việc (Vd: Sửa ống nước)

        [Required]
        public string Description { get; set; } = null!; // Mô tả chi tiết tình trạng

        [Required]
        public decimal Price { get; set; } // Giá tiền thỏa thuận

        [Required]
        public Point Location { get; set; } = null!; // Tọa độ nhà khách hàng để thợ tìm đến

        [Required, MaxLength(20)]
        public string Status { get; set; } = "PENDING"; // Trạng thái: PENDING, ACCEPTED, COMPLETED, CANCELLED

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}