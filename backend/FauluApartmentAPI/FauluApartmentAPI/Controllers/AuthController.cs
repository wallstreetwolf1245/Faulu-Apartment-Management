
using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Models;
using FauluApartmentAPI.Models.Dtos;
using FauluApartmentAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FauluApartmentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        UserManager<User> userManager,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (success, token, refreshToken, error) = await _authService.AuthenticateAsync(request.Email, request.Password);

            if (!success)
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse(error ?? "Invalid credentials"));

            var user = await _userManager.FindByEmailAsync(request.Email);
            var roles = await _userManager.GetRolesAsync(user!);

            var response = new LoginResponse
            {
                UserId = user!.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                RefreshToken = refreshToken,
                Roles = roles.ToList()
            };

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResponse("An error occurred during login"));
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register(
        [FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("All fields are required"));
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Passwords do not match"));
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Password must be at least 6 characters"));
            }

            var result = await _authService.RegisterAsync(request.Email, request.FirstName, request.LastName, request.Password);

            if (!result)
                return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Registration failed. Email may already be in use"));

            var user = await _userManager.FindByEmailAsync(request.Email);
            var response = new RegisterResponse
            {
                Success = true,
                Message = "Registration successful. You can now login",
                UserId = user?.Id.ToString()
            };

            return CreatedAtAction(nameof(Register), ApiResponse<RegisterResponse>.SuccessResponse(response, "Registration successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return StatusCode(500, ApiResponse<RegisterResponse>.ErrorResponse("An error occurred during registration"));
        }
    }
}
