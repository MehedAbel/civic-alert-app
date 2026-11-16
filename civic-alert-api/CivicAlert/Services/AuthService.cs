using CivicAlert.Data;
using CivicAlert.DTOs.Auth;
using CivicAlert.Entities;
using CivicAlert.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CivicAlert.Services;

public class AuthService(CivicAlertDbContext db, IJwtService jwt) : IAuthService
{
    private static readonly Regex NameRegex = new("^[a-zA-Zà-žÀ-Ž-]{2,50}$", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || !NameRegex.IsMatch(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName) || !NameRegex.IsMatch(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email))
        {
            throw new InvalidRegisterCredentialsException("Invalid user registration credentials format");
        }

        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
        {
            throw new UserAlreadyExistsException("Email already in use by another user");
        }

        var user = new AppUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
            Roles = [Role.CLIENT]
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return new RegisterResponseDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = user.Roles
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email))
        {
            throw new InvalidLoginCredentialsException("Invalid email or password");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            throw new InvalidLoginCredentialsException("Invalid email or password");
        }

        var token = jwt.GenerateToken(user.Email, user.Roles);
        return new LoginResponseDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AccessToken = token,
            Roles = user.Roles
        };
    }
}
