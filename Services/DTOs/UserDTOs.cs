namespace Services.DTOs
{
    public class UserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? FcmToken { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
