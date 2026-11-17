namespace CEMIS.DTOs;

public class DanhMucNguonPhatThaiDto
{
    public long Id { get; set; }
    public string MaNguon { get; set; } = string.Empty;
    public string TenNguon { get; set; } = string.Empty;
    public string PhamVi { get; set; } = string.Empty; // scope_1, scope_2, scope_3
    public string PhanLoai { get; set; } = string.Empty;
    public string DonViTinhChuan { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

public class HeSoGwpDto
{
    public long Id { get; set; }
    public string PhienBanIpcc { get; set; } = string.Empty;
    public decimal? GwpCo2 { get; set; }
    public decimal GwpCh4 { get; set; }
    public decimal GwpN2o { get; set; }
    public string? GhiChu { get; set; }
}

public class HeSoPhatThaiDto
{
    public long Id { get; set; }
    public long IdNguonPhatThai { get; set; }
    public string MaNguon { get; set; } = string.Empty;
    public string TenNguon { get; set; } = string.Empty;
    public string PhamVi { get; set; } = string.Empty;
    public long? IdGwp { get; set; }
    public string? PhienBanGwp { get; set; }
    public string TenHeSo { get; set; } = string.Empty;
    public string NguonGoc { get; set; } = string.Empty;
    public int NamApDung { get; set; }
    public decimal? HeSoCo2 { get; set; }
    public decimal? HeSoCh4 { get; set; }
    public decimal? HeSoN2o { get; set; }
    public decimal TongHeSoCo2e { get; set; }
    public string DonViHeSo { get; set; } = string.Empty;
    public bool? ApDung { get; set; }
    public string? GhiChu { get; set; }
}

public class CreateHeSoPhatThaiDto
{
    public long IdNguonPhatThai { get; set; }
    public long? IdGwp { get; set; }
    public string TenHeSo { get; set; } = string.Empty;
    public string NguonGoc { get; set; } = string.Empty;
    public int NamApDung { get; set; }
    public decimal? HeSoCo2 { get; set; }
    public decimal? HeSoCh4 { get; set; }
    public decimal? HeSoN2o { get; set; }
    public decimal TongHeSoCo2e { get; set; }
    public string DonViHeSo { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
}
