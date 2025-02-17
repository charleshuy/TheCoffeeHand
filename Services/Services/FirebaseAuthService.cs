using FirebaseAdmin.Auth;
using Interfracture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Services
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> SignInWithFirebaseAsync(string idToken)
        {
            try
            {
                // Verify Firebase Token
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string firebaseUserId = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;
                string firstName = decodedToken.Claims.ContainsKey("given_name") ? decodedToken.Claims["given_name"].ToString() : "";
                string lastName = decodedToken.Claims.ContainsKey("family_name") ? decodedToken.Claims["family_name"].ToString() : "";

                if (string.IsNullOrEmpty(email))
                {
                    throw new Exception("Firebase token does not contain an email.");
                }

                // Check if the user already exists in our database
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    // Create a new user
                    var newUser = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName
                    };

                    var result = await _userManager.CreateAsync(newUser);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Failed to create user.");
                    }
                }

                // Retrieve the user (existing or newly created)
                var user = await _userManager.FindByEmailAsync(email);
                return GenerateJwtToken(user);
            }
            catch (FirebaseAuthException)
            {
                throw new Exception("Invalid Firebase token.");
            }
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
