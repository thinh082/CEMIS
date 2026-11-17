namespace CEMIS.DTOs;

public class ExportReportRequestDto
{
    public long IdToChuc { get; set; }
    public long IdKyBaoCao { get; set; }
    public string LoaiBaoCao { get; set; } = "kiem_ke_knk_vn"; // 'kiem_ke_knk_vn', 'gri_302_nang_luong', 'gri_305_phat_thai', 'audit_trail'
    public string DinhDang { get; set; } = "excel"; // 'excel', 'pdf'
}

public class BaoCaoLichSuDto
{
    public long Id { get; set; }
    public string TenToChuc { get; set; } = string.Empty;
    public string TenKyBaoCao { get; set; } = string.Empty;
    public string TenNguoiXuat { get; set; } = string.Empty;
    public string TenBaoCao { get; set; } = string.Empty;
    public string LoaiBaoCao { get; set; } = string.Empty;
    public string DinhDang { get; set; } = string.Empty;
    public string DuongDanFile { get; set; } = string.Empty;
    public DateTime NgayXuat { get; set; }
}
