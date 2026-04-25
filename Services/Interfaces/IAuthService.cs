using Had360back.Dtos.Auth;

namespace Had360back.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterUserAsync(RegisterUserDto dto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterOrganizationAsync(RegisterOrganizationDto dto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto dto);
}
