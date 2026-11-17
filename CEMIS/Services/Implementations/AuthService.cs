using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CEMIS.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly CemisDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(CemisDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.NguoiDungs
            .Include(u => u.IdVaiTroNavigation)
            .Include(u => u.IdToChucNavigation)
            .FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);

        if (user == null)
        {
            throw new AppUnauthorizedException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        bool isPasswordValid = false;
        try
        {
            if (user.MatKhauHash.StartsWith("$2") || user.MatKhauHash.StartsWith("$2a$") || user.MatKhauHash.StartsWith("$2b$"))
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhauHash);
            }
            else if (user.MatKhauHash == "hash_pass_123")
            {
                // Dữ liệu hạt giống ban đầu: chỉ chấp nhận đúng mật khẩu quy chuẩn "123456"
                isPasswordValid = (request.MatKhau == "123456");
                if (isPasswordValid)
                {
                    // Tự động nâng cấp hash mật khẩu lên chuẩn BCrypt an toàn
                    user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword("123456");
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                isPasswordValid = false;
            }
        }
        catch
        {
            isPasswordValid = false;
        }

        if (!isPasswordValid)
        {
            throw new AppUnauthorizedException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        if (user.TrangThai == "khoa")
        {
            throw new AppForbiddenException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "CemisSuperSecretKeyJwtAuth2026Iso14064StandardNetZero!");
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim("FullName", user.HoTen),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IdVaiTroNavigation.MaVaiTro),
                new Claim("OrganizationId", user.IdToChuc?.ToString() ?? "")
            }),
            Expires = expiresAt,
            Issuer = _configuration["Jwt:Issuer"] ?? "CEMIS_Server",
            Audience = _configuration["Jwt:Audience"] ?? "CEMIS_Client",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new LoginResponseDto
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            User = MapToProfile(user)
        };
    }

    public async Task<UserProfileDto> RegisterAsync(RegisterRequestDto request)
    {
        var exists = await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == request.TenDangNhap || u.Email == request.Email);
        if (exists)
        {
            throw new InvalidOperationException("Tên đăng nhập hoặc Email đã tồn tại trong hệ thống.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau);

        var newUser = new NguoiDung
        {
            TenDangNhap = request.TenDangNhap,
            MatKhauHash = passwordHash,
            HoTen = request.HoTen,
            Email = request.Email,
            SoDienThoai = request.SoDienThoai,
            IdVaiTro = request.IdVaiTro > 0 ? request.IdVaiTro : 3, // Mặc định: nhân viên nhập liệu
            IdToChuc = request.IdToChuc,
            TrangThai = "hoat_dong"
        };

        _context.NguoiDungs.Add(newUser);
        await _context.SaveChangesAsync();

        return await GetProfileAsync(newUser.Id);
    }

    public async Task<UserProfileDto> GetProfileAsync(long userId)
    {
        var user = await _context.NguoiDungs
            .Include(u => u.IdVaiTroNavigation)
            .Include(u => u.IdToChucNavigation)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new KeyNotFoundException("Không tìm thấy người dùng.");
        }

        if (user.TrangThai == "khoa")
        {
            throw new AppForbiddenException("Tài khoản của bạn đã bị khóa.");
        }

        return MapToProfile(user);
    }

    private static UserProfileDto MapToProfile(NguoiDung user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            TenDangNhap = user.TenDangNhap,
            HoTen = user.HoTen,
            Email = user.Email,
            SoDienThoai = user.SoDienThoai,
            IdVaiTro = user.IdVaiTro,
            MaVaiTro = user.IdVaiTroNavigation?.MaVaiTro ?? string.Empty,
            TenVaiTro = user.IdVaiTroNavigation?.TenVaiTro ?? string.Empty,
            IdToChuc = user.IdToChuc,
            TenToChuc = user.IdToChucNavigation?.TenToChuc,
            TrangThai = user.TrangThai ?? "hoat_dong",
            DuongDanChuKyAnh = user.DuongDanChuKyAnh
        };
    }
}
