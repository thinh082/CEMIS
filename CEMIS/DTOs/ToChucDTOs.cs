namespace CEMIS.DTOs;

public class ToChucDto
{
    public long Id { get; set; }
    public string MaToChuc { get; set; } = string.Empty;
    public string TenToChuc { get; set; } = string.Empty;
    public string LoaiToChuc { get; set; } = string.Empty;
    public long? IdToChucCha { get; set; }
    public string? TenToChucCha { get; set; }
    public string? DiaChi { get; set; }
    public decimal? DienTichSanM2 { get; set; }
    public int? SoNhanVien { get; set; }
    public string? TrangThai { get; set; }
    public List<ToChucDto> ChiNhanhCon { get; set; } = new();
}

public class CreateToChucDto
{
    public string MaToChuc { get; set; } = string.Empty;
    public string TenToChuc { get; set; } = string.Empty;
    public string LoaiToChuc { get; set; } = string.Empty;
    public long? IdToChucCha { get; set; }
    public string? DiaChi { get; set; }
    public decimal? DienTichSanM2 { get; set; }
    public int? SoNhanVien { get; set; }
}

public class UpdateToChucDto
{
    public string TenToChuc { get; set; } = string.Empty;
    public string LoaiToChuc { get; set; } = string.Empty;
    public long? IdToChucCha { get; set; }
    public string? DiaChi { get; set; }
    public decimal? DienTichSanM2 { get; set; }
    public int? SoNhanVien { get; set; }
    public string? TrangThai { get; set; }
}
