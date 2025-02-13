using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")] // Ensure this matches your API request URL
public class AuthController : ControllerBase
{
    [HttpGet("verify-token")]
    public async Task<IActionResult> VerifyToken()
    {
        try
        {
            // Get token from Authorization header
            string authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Missing or invalid token" });
            }

            string token = authHeader.Substring(7); // Remove "Bearer " prefix

            // Verify Firebase Token
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            string userId = decodedToken.Uid;

            return Ok(new { message = "Token is valid", userId });
        }
        catch (FirebaseAuthException)
        {
            return Unauthorized(new { message = "Invalid or expired token" });
        }
    }
}
