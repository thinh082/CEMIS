using CEMIS.DTOs;

namespace CEMIS.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserProfileDto> RegisterAsync(RegisterRequestDto request);
    Task<UserProfileDto> GetProfileAsync(long userId);
}
