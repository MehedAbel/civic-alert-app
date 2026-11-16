using CivicAlert.Entities;

namespace CivicAlert.DTOs.Auth;

public class LoginResponseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public HashSet<Role> Roles { get; set; } = [];
}
