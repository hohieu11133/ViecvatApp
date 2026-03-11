using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using System.Security.Claims;
using UserService.Data;
using UserService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace UserService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	// Ổ KHÓA NÂNG CẤP: Chỉ những ai cầm thẻ Token có ghi chữ "WORKER" mới được vào!
	[Authorize(Roles = "WORKER")]
	public class WorkerController : ControllerBase
	{
		private readonly UserDbContext _context;

		public WorkerController(UserDbContext context)
		{
			_context = context;
		}

		[HttpPut("location")]
		public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
		{
			// 1. Lấy ID của thợ từ JWT Token
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

			// 2. Tìm hồ sơ thợ trong database
			var worker = await _context.Workers.FindAsync(userId);
			if (worker == null) return NotFound(new { Message = "Không tìm thấy hồ sơ thợ!" });

			// 3. Cập nhật tọa độ
		
			worker.CurrentLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

			await _context.SaveChangesAsync();

			return Ok(new { Message = "Cập nhật vị trí thành công!" });
		}

		// API Tìm thợ quanh đây (Bất kỳ user nào đăng nhập cũng gọi được)
		[HttpGet("nearby")]
		[AllowAnonymous] // Tạm thời mở cửa cho dễ test, thực tế bạn có thể đổi thành [Authorize]
		public async Task<IActionResult> FindNearbyWorkers([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double radiusKm = 5)
		{
			// 1. Tạo tọa độ điểm đứng của khách hàng (Nhớ quy tắc: Kinh độ X trước, Vĩ độ Y sau)
			var customerLocation = new Point(lon, lat) { SRID = 4326 };

			// Đổi km ra mét vì SQL Server tính toán GEOGRAPHY theo đơn vị mét
			double radiusMeters = radiusKm * 1000;

			// 2. Truy vấn Database để tìm thợ
			var nearbyWorkers = await _context.Workers
				.Include(w => w.Account) // Kéo theo thông tin Tên, SĐT từ bảng Account
				.Where(w => w.CurrentLocation != null && w.CurrentLocation.Distance(customerLocation) <= radiusMeters)
				.Select(w => new
				{
					WorkerId = w.AccountId,
					FullName = w.Account.FullName,
					Phone = w.Account.Phone,
					AvatarUrl = w.Account.AvatarUrl,
					// Tính lại ra Km để hiển thị cho đẹp
					DistanceKm = Math.Round(w.CurrentLocation.Distance(customerLocation) / 1000, 2)
				})
				.OrderBy(w => w.DistanceKm) // Xếp thợ ở gần lên đầu
				.ToListAsync();

			return Ok(nearbyWorkers);
		}
	}
}