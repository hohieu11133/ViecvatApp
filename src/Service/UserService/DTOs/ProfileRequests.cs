namespace UserService.DTOs
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}