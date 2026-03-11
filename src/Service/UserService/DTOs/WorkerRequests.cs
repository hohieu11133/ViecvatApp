using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UpdateLocationRequest
    {
        [Required]
        public double Latitude { get; set; } // Vĩ độ (Vd: 10.762622)

        [Required]
        public double Longitude { get; set; } // Kinh độ (Vd: 106.660172)
    }
}