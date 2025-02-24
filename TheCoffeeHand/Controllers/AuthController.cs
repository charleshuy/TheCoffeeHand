using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly IUserServices _userService;

    public AuthController(IFirebaseAuthService firebaseAuthService, IUserServices userServices)
    {
        _firebaseAuthService = firebaseAuthService;
        _userService = userServices;
    }

    [HttpPost("firebase-login")]
    public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.IdToken))
        {
            return BadRequest(new { message = "ID token is required." });
        }

        var jwtToken = await _firebaseAuthService.SignInWithFirebaseAsync(request.IdToken, request.fmcToken);
        return Ok(new { token = jwtToken });
    }

    [HttpGet("user-auth")]
    [Authorize(AuthenticationSchemes = "Firebase,Jwt")]
    public IActionResult GetUserInfo()
    {
        return Ok("Access granted to authenticated user!");
    }

    [HttpGet("admin-only")] 
    [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok("Access granted for Admin!");
    }

    [HttpGet("profile")]
    [Authorize(AuthenticationSchemes = "Firebase,Jwt")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        return Ok(user);
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
