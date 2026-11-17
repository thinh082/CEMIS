using System.Security.Claims;
using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEMIS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Đăng nhập thành công"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail(ex.Message, 401));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "super_admin")]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<UserProfileDto>.Ok(result, "Tạo tài khoản thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserProfileDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetCurrentUser()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId))
        {
            return Unauthorized(ApiResponse<UserProfileDto>.Fail("Không xác thực được danh tính.", 401));
        }

        var profile = await _authService.GetProfileAsync(userId);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }
}
