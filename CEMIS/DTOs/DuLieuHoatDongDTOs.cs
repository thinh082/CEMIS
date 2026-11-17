namespace CEMIS.DTOs;

public class KyBaoCaoDto
{
    public long Id { get; set; }
    public long IdToChuc { get; set; }
    public string TenToChuc { get; set; } = string.Empty;
    public string TenKy { get; set; } = string.Empty;
    public int? Thang { get; set; }
    public int? Quy { get; set; }
    public int Nam { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public string? TrangThaiKhoaSo { get; set; }
    public decimal TongPhatThaiTanCo2e { get; set; }
}

public class CreateKyBaoCaoDto
{
    public long IdToChuc { get; set; }
    public string TenKy { get; set; } = string.Empty;
    public int? Thang { get; set; }
    public int? Quy { get; set; }
    public int Nam { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
}

public class DuLieuHoatDongDto
{
    public long Id { get; set; }
    public long IdToChuc { get; set; }
    public string TenToChuc { get; set; } = string.Empty;
    public long IdKyBaoCao { get; set; }
    public string TenKyBaoCao { get; set; } = string.Empty;
    public long IdNguonPhatThai { get; set; }
    public string MaNguon { get; set; } = string.Empty;
    public string TenNguon { get; set; } = string.Empty;
    public string PhamVi { get; set; } = string.Empty;
    public long IdNguoiNhap { get; set; }
    public string TenNguoiNhap { get; set; } = string.Empty;
    public decimal SoLuongNhap { get; set; }
    public string DonViNhap { get; set; } = string.Empty;
    public decimal? HeSoQuyDoi { get; set; }
    public decimal SoLuongQuyChuan { get; set; }
    public DateOnly? NgaySuDung { get; set; }
    public string? SoHoaDon { get; set; }
    public DateOnly? NgayHoaDon { get; set; }
    public string? NhaCungCap { get; set; }
    public string? TrangThaiDuyet { get; set; }
    public string? GhiChu { get; set; }
    public decimal? KetQuaPhatThaiTanCo2e { get; set; }
    public string? CongThucApDung { get; set; }
    public decimal? HeSoPhatThai { get; set; }
    public string? DonViHeSo { get; set; }
    public List<FileMinhChungDto> MinhChungs { get; set; } = new();
}

public class CreateDuLieuHoatDongDto
{
    public long IdToChuc { get; set; }
    public long IdKyBaoCao { get; set; }
    public long IdNguonPhatThai { get; set; }
    public decimal SoLuongNhap { get; set; }
    public string DonViNhap { get; set; } = string.Empty;
    public decimal? HeSoQuyDoi { get; set; } = 1.0m;
    public DateOnly? NgaySuDung { get; set; }
    public string? SoHoaDon { get; set; }
    public DateOnly? NgayHoaDon { get; set; }
    public string? NhaCungCap { get; set; }
    public string? GhiChu { get; set; }
}

public class UpdateDuLieuHoatDongDto
{
    public decimal SoLuongNhap { get; set; }
    public string DonViNhap { get; set; } = string.Empty;
    public decimal? HeSoQuyDoi { get; set; } = 1.0m;
    public DateOnly? NgaySuDung { get; set; }
    public string? SoHoaDon { get; set; }
    public DateOnly? NgayHoaDon { get; set; }
    public string? NhaCungCap { get; set; }
    public string? GhiChu { get; set; }
}

public class PheDuyetRequestDto
{
    public string HanhDong { get; set; } = string.Empty; // 'phe_duyet', 'tu_choi', 'gui_duyet'
    public string? LyDoTuChoi { get; set; }
    public string? GhiChu { get; set; }
}

public class FileMinhChungDto
{
    public long Id { get; set; }
    public long IdDuLieuHoatDong { get; set; }
    public string TenFile { get; set; } = string.Empty;
    public string DuongDanFile { get; set; } = string.Empty;
    public string? LoaiFile { get; set; }
    public long? DungLuongKb { get; set; }
    public string? GhiChu { get; set; }
}
