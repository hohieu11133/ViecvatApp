using JobService.Data;
using JobService.DTOs;
using JobService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Security.Claims;

namespace JobService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly JobDbContext _context;

        public JobController(JobDbContext context)
        {
            _context = context;
        }

        // 1. API ĐĂNG VIỆC (Chỉ Khách hàng mới được gọi)
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
        {
            // Lấy ID của Khách hàng từ Token
            var customerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(customerIdString, out Guid customerId)) return Unauthorized();

            // Tạo công việc mới
            var job = new Job
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                // Chuyển Lat/Lon thành kiểu GEOGRAPHY (Kinh độ X trước, Vĩ độ Y sau)
                Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 },
                Status = "PENDING" // Vừa đăng xong thì chờ thợ nhận
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, new { Message = "Đăng việc thành công!", JobId = job.Id });
        }

        // 2. API TÌM VIỆC QUANH ĐÂY (Chỉ Thợ mới được gọi)
        [HttpGet("nearby")]
        [Authorize(Roles = "WORKER")]
        public async Task<IActionResult> FindNearbyJobs([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double radiusKm = 10)
        {
            var workerLocation = new Point(lon, lat) { SRID = 4326 };
            double radiusMeters = radiusKm * 1000;

            // Tìm các công việc đang PENDING và nằm trong bán kính
            var nearbyJobs = await _context.Jobs
                .Where(j => j.Status == "PENDING" && j.Location.Distance(workerLocation) <= radiusMeters)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Description,
                    j.Price,
                    // Tính khoảng cách từ Thợ đến chỗ làm
                    DistanceKm = Math.Round(j.Location.Distance(workerLocation) / 1000, 2),
                    j.CreatedAt
                })
                .OrderBy(j => j.DistanceKm) // Ưu tiên việc gần nhất lên đầu
                .ToListAsync();

            return Ok(nearbyJobs);
        }

        // 3. API NHẬN VIỆC (Chỉ Thợ mới được gọi)
        [HttpPut("{id}/accept")]
        // Mẹo nhỏ: Ta bao lô cả chữ hoa lẫn chữ thường đề phòng bạn đăng ký nhầm case
        [Authorize(Roles = "WORKER,Worker")]
        public async Task<IActionResult> AcceptJob(Guid id)
        {
            // 1. Lấy ID của Thợ từ Token
            var workerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(workerIdString, out Guid workerId)) return Unauthorized();

            // 2. Tìm công việc trong Database
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound(new { Message = "Không tìm thấy công việc này!" });

            // 3. Kiểm tra xem việc còn trống không (nhỡ có thợ khác nhanh tay nhận mất rồi)
            if (job.Status != "PENDING")
                return BadRequest(new { Message = "Rất tiếc, công việc này đã có người nhận hoặc bị hủy!" });

            // 4. "Chốt đơn!" - Gắn ID thợ vào và đổi trạng thái
            job.WorkerId = workerId;
            job.Status = "ACCEPTED";

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Nhận việc thành công! Hãy chuẩn bị đồ nghề lên đường nhé.", JobId = job.Id });
        }

        // Hàm phụ trợ để API CreateJob trả về thông tin sau khi đăng thành công
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(Guid id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        // API dùng để gỡ lỗi xem Token đang chứa gì
        [HttpGet("test-token")]
        [Authorize] // Ổ khóa này KHÔNG xét Role, cứ có Token thật là cho vào
        public IActionResult TestToken()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Ok(new
            {
                Message = "Bảo vệ đã cho qua!",
                YourUserId = userId,
                YourRole = role ?? "KHÔNG TÌM THẤY ROLE"
            });
        }
    }
}