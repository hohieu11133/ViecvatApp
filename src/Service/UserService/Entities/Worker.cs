using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;
namespace UserService.Entities
{
	public class Worker
	{
		[Key]
		public Guid AccountId { get; set; }
		public string? IdCardFront { get; set; }
		public string? IdCardBack { get; set; }
		public double AverageRating { get; set; } = 0;
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = null!;
        // Đây chính là "vũ khí" để tính khoảng cách 5km
        public Point? CurrentLocation { get; set; }
	}
}