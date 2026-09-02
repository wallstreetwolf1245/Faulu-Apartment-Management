namespace FauluApartmentAPI.Models.Dtos;

/// <summary>
/// DTO for login request
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for login response
/// </summary>
public class LoginResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// DTO for registration request
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string UserType { get; set; } = "Tenant"; // Tenant, Owner, Manager
}

/// <summary>
/// DTO for registration response
/// </summary>
public class RegisterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
}
