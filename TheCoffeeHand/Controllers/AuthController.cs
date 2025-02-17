using FirebaseAdmin.Auth;
using Interfracture.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.Interfaces;

[ApiController]
[Route("auth")] // Ensure this matches your API request URL
public class AuthController : ControllerBase
{
    private readonly IFirebaseAuthService _firebaseAuthService;

    public AuthController(IFirebaseAuthService firebaseAuthService)
    {
        _firebaseAuthService = firebaseAuthService;
    }

    [HttpPost("firebase-login")]
    public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.IdToken))
        {
            return BadRequest(new { message = "ID token is required." });
        }

        try
        {
            var jwtToken = await _firebaseAuthService.SignInWithFirebaseAsync(request.IdToken);
            return Ok(new { token = jwtToken });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }


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
