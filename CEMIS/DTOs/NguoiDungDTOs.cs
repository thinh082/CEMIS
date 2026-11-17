namespace CEMIS.DTOs;

public class NguoiDungDto
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
    public string TrangThai { get; set; } = "hoat_dong";
    public string? DuongDanChuKyAnh { get; set; }
}

public class VaiTroDto
{
    public long Id { get; set; }
    public string MaVaiTro { get; set; } = string.Empty;
    public string TenVaiTro { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

public class CreateNguoiDungDto
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public long IdVaiTro { get; set; }
    public long? IdToChuc { get; set; }
    public string? DuongDanChuKyAnh { get; set; }
}

public class UpdateNguoiDungDto
{
    public string? TenDangNhap { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public long IdVaiTro { get; set; }
    public long? IdToChuc { get; set; }
    public string TrangThai { get; set; } = "hoat_dong";
    public string? MatKhauMoi { get; set; }
    public string? DuongDanChuKyAnh { get; set; }
}

public class UpdateSignatureDto
{
    public string? DuongDanChuKyAnh { get; set; }
}
