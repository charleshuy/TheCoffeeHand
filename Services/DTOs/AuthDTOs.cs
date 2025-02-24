namespace Services.DTOs
{
    public class AuthDTOs
    {
    }
    public class FirebaseLoginRequest
    {
        public required string IdToken { get; set; }
        public string? fmcToken { get; set; }
    }
}
