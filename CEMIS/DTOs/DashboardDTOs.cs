namespace CEMIS.DTOs;

public class DashboardSummaryDto
{
    public decimal TongPhatThaiTanCo2e { get; set; }
    public decimal Scope1TanCo2e { get; set; }
    public decimal Scope2TanCo2e { get; set; }
    public decimal Scope3TanCo2e { get; set; }
    public decimal PhanTramScope1 => TongPhatThaiTanCo2e > 0 ? Math.Round(Scope1TanCo2e / TongPhatThaiTanCo2e * 100, 2) : 0;
    public decimal PhanTramScope2 => TongPhatThaiTanCo2e > 0 ? Math.Round(Scope2TanCo2e / TongPhatThaiTanCo2e * 100, 2) : 0;
    public decimal PhanTramScope3 => TongPhatThaiTanCo2e > 0 ? Math.Round(Scope3TanCo2e / TongPhatThaiTanCo2e * 100, 2) : 0;
    
    public decimal TongDienTieuThuKWh { get; set; }
    public decimal TongNhienLieuLit { get; set; }
    public int TongSoCoSo { get; set; }
    public int SoLuongCanhBaoChuaXuLy { get; set; }
    public decimal CuongDoPhatThaiTheoSanPham { get; set; } // tCO2e / sản phẩm
    public decimal CuongDoPhatThaiTheoDoanhThu { get; set; } // tCO2e / tỷ đồng
    public decimal CuongDoPhatThaiTheoDienTich { get; set; } // tCO2e / m2 sàn
}

public class MonthlyTrendDto
{
    public string Thang { get; set; } = string.Empty; // "T1", "T2", ...
    public decimal Scope1 { get; set; }
    public decimal Scope2 { get; set; }
    public decimal Scope3 { get; set; }
    public decimal TongPhatThai => Scope1 + Scope2 + Scope3;
}

public class EmissionBySourceDto
{
    public string TenNguon { get; set; } = string.Empty;
    public string PhamVi { get; set; } = string.Empty;
    public decimal TongTanCo2e { get; set; }
    public decimal PhanTram { get; set; }
}

public class FacilityComparisonDto
{
    public long IdToChuc { get; set; }
    public string TenToChuc { get; set; } = string.Empty;
    public decimal Scope1 { get; set; }
    public decimal Scope2 { get; set; }
    public decimal Scope3 { get; set; }
    public decimal TongPhatThai => Scope1 + Scope2 + Scope3;
    public decimal CuongDoM2 { get; set; }
}

public class AlertDto
{
    public long Id { get; set; }
    public string TenToChuc { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public decimal GiaTriThucTe { get; set; }
    public decimal GiaTriDinhMuc { get; set; }
    public decimal PhanTramVuot { get; set; }
    public string MucDo { get; set; } = string.Empty;
    public DateTime NgayPhatHien { get; set; }
    public bool? DaXuLy { get; set; }
}
