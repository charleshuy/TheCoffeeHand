using FirebaseAdmin.Auth;
using Interfracture.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;
using System.Threading.Tasks;

/// <summary>
/// Controller for handling authentication and authorization.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly IUserServices _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="firebaseAuthService">The Firebase authentication service.</param>
    /// <param name="userServices">The user service.</param>
    public AuthController(IFirebaseAuthService firebaseAuthService, IUserServices userServices)
    {
        _firebaseAuthService = firebaseAuthService;
        _userService = userServices;
    }

    /// <summary>
    /// Logs in a user using Firebase authentication.
    /// </summary>
    /// <param name="request">The Firebase login request containing the ID token.</param>
    /// <returns>A JWT token if authentication is successful.</returns>
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

    /// <summary>
    /// Retrieves user authentication status.
    /// </summary>
    /// <returns>A success message if the user is authenticated.</returns>
    [HttpGet("user-auth")]
    [Authorize(AuthenticationSchemes = "Firebase,Jwt")]
    public IActionResult GetUserInfo()
    {
        return Ok("Access granted to authenticated user!");
    }

    /// <summary>
    /// Retrieves admin-only access information.
    /// </summary>
    /// <returns>A success message if the user has admin privileges.</returns>
    [HttpGet("admin-only")]
    [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok("Access granted for Admin!");
    }

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    /// <returns>The user profile if authenticated; otherwise, an unauthorized response.</returns>
    [HttpGet("profile")]
    [Authorize(AuthenticationSchemes = "Firebase,Jwt")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        return Ok(user);
    }

    /// <summary>
    /// Verifies the provided Firebase authentication token.
    /// </summary>
    /// <returns>A success message if the token is valid; otherwise, an unauthorized response.</returns>
    [HttpGet("verify-token")]
    public async Task<IActionResult> VerifyToken()
    {
        try
        {
            // Get token from Authorization header
            string? authHeader = Request.Headers["Authorization"];
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
