using CivicAlert.DTOs.Auth;

namespace CivicAlert.Services;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
}
