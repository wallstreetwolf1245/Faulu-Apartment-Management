using FauluApartmentAPI.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FauluApartmentAPI.Services;

/// <summary>
/// Authentication service interface for JWT token operations
/// </summary>
public interface IAuthenticationService
{
    Task<(bool Success, string Token, string RefreshToken, string? Error)> AuthenticateAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string firstName, string lastName, string password);
    Task<bool> AddToRoleAsync(User user, string role);
}

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string Token, string RefreshToken, string? Error)> AuthenticateAsync(string email, string password)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                return (false, string.Empty, string.Empty, "Invalid email or password");

            if (!user.IsActive)
                return (false, string.Empty, string.Empty, "User account is inactive");

            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            return (true, token, refreshToken, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return (false, string.Empty, string.Empty, "Authentication failed");
        }
    }

    public async Task<bool> RegisterAsync(string email, string firstName, string lastName, string password)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt for existing email: {Email}", email);
                return false;
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for email {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            // Assign Tenant role by default
            await _userManager.AddToRoleAsync(user, "Tenant");
            _logger.LogInformation("User registered successfully: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return false;
        }
    }

    public async Task<bool> AddToRoleAsync(User user, string role)
    {
        try
        {
            if (!await _roleManager.RoleExistsAsync(role))
                return false;

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user to role");
            return false;
        }
    }

    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
