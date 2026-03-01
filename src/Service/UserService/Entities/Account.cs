using System.ComponentModel.DataAnnotations;

namespace UserService.Entities
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(20)]
        public string Phone { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "CUSTOMER"; // CUSTOMER, WORKER, ADMIN

        [Required, MaxLength(20)]
        public string Status { get; set; } = "ACTIVE";
    }
}