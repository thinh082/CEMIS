using System.ComponentModel.DataAnnotations;

namespace CEMIS.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    public string MatKhau { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserProfileDto User { get; set; } = null!;
}

public class RegisterRequestDto
{
    [Required]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string MatKhau { get; set; } = string.Empty;

    [Required]
    public string HoTen { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? SoDienThoai { get; set; }
    public long IdVaiTro { get; set; }
    public long? IdToChuc { get; set; }
}

public class UserProfileDto
{
    public long Id { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public long IdVaiTro { get; set; }
    public string MaVaiTro { get; set; } = string.Empty;
    public string TenVaiTro { get; set; } = string.Empty;
    public long? IdToChuc { get; set; }
    public string? TenToChuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? DuongDanChuKyAnh { get; set; }
}
