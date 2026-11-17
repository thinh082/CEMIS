using ClosedXML.Excel;
using CEMIS.DTOs;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Services.Implementations;

public class BaoCaoService : IBaoCaoService
{
    private readonly CemisDbContext _context;

    public BaoCaoService(CemisDbContext context)
    {
        _context = context;
    }

    public async Task<(byte[] FileBytes, string FileName, string ContentType)> ExportReportAsync(ExportReportRequestDto request, long nguoiXuatId)
    {
        var toChuc = await _context.ToChucs.FindAsync(request.IdToChuc);
        var kyBaoCao = await _context.KyBaoCaos.FindAsync(request.IdKyBaoCao);

        if (toChuc == null || kyBaoCao == null)
            throw new KeyNotFoundException("Không tìm thấy thông tin Cơ sở hoặc Kỳ báo cáo.");

        var calculations = await _context.KetQuaTinhToans
            .Include(k => k.IdDuLieuHoatDongNavigation)
                .ThenInclude(d => d.IdNguonPhatThaiNavigation)
            .Include(k => k.IdDuLieuHoatDongNavigation)
                .ThenInclude(d => d.IdNguoiNhapNavigation)
            .Include(k => k.IdDuLieuHoatDongNavigation)
                .ThenInclude(d => d.FileMinhChungs)
            .Include(k => k.IdDuLieuHoatDongNavigation)
                .ThenInclude(d => d.LichSuPheDuyets)
                    .ThenInclude(l => l.IdNguoiThucHienNavigation)
            .Include(k => k.IdHeSoPhatThaiNavigation)
                .ThenInclude(h => h.IdGwpNavigation)
            .Where(k => k.IdToChuc == request.IdToChuc && k.IdKyBaoCao == request.IdKyBaoCao)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        string reportType = request.LoaiBaoCao?.ToLower().Trim() ?? "kiem_ke_knk_vn";
        string fileName;

        switch (reportType)
        {
            case "gri_302_nang_luong":
                GenerateGri302Sheet(workbook, toChuc, kyBaoCao, calculations);
                fileName = $"BaoCao_GRI302_NangLuong_{toChuc.MaToChuc}_{kyBaoCao.Nam}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                break;

            case "gri_305_phat_thai":
                await GenerateGri305Sheet(workbook, toChuc, kyBaoCao, calculations);
                fileName = $"BaoCao_GRI305_PhatThai_{toChuc.MaToChuc}_{kyBaoCao.Nam}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                break;

            case "audit_trail":
                GenerateAuditTrailSheet(workbook, toChuc, kyBaoCao, calculations);
                fileName = $"Bang_DoiChieu_KiemToan_{toChuc.MaToChuc}_{kyBaoCao.Nam}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                break;

            default:
                GenerateKiemKeKnkSheet(workbook, toChuc, kyBaoCao, calculations);
                fileName = $"BaoCao_KNK_{toChuc.MaToChuc}_{kyBaoCao.Nam}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                break;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        var bytes = ms.ToArray();

        // Ghi lịch sử xuất báo cáo
        var history = new BaoCaoXuat
        {
            IdToChuc = request.IdToChuc,
            IdKyBaoCao = request.IdKyBaoCao,
            IdNguoiXuat = nguoiXuatId,
            TenBaoCao = fileName,
            LoaiBaoCao = request.LoaiBaoCao ?? "kiem_ke_knk_vn",
            DinhDang = "excel",
            DuongDanFile = $"/reports/{fileName}",
            NgayXuat = DateTime.UtcNow
        };
        _context.BaoCaoXuats.Add(history);
        await _context.SaveChangesAsync();

        return (bytes, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private void GenerateKiemKeKnkSheet(XLWorkbook workbook, ToChuc toChuc, KyBaoCao kyBaoCao, List<KetQuaTinhToan> calculations)
    {
        var ws = workbook.Worksheets.Add("BaoCao_KiemKe_KNK");
        ws.Cell(1, 1).Value = "HỆ THỐNG QUẢN LÝ VÀ KIỂM KÊ PHÁT THẢI KHÍ NHÀ KÍNH (CEMIS)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 9).Merge();

        ws.Cell(2, 1).Value = $"Đơn vị báo cáo: {toChuc.TenToChuc} (Mã cơ sở: {toChuc.MaToChuc})";
        ws.Cell(3, 1).Value = $"Kỳ kiểm kê: {kyBaoCao.TenKy} (Năm: {kyBaoCao.Nam}) - Chuẩn ISO 14064-1 & GHG Protocol";
        ws.Cell(4, 1).Value = $"Thời gian xuất: {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm:ss} (GMT+7)";

        int row = 6;
        ws.Cell(row, 1).Value = "STT";
        ws.Cell(row, 2).Value = "Phạm vi (Scope)";
        ws.Cell(row, 3).Value = "Nguồn phát thải";
        ws.Cell(row, 4).Value = "Số lượng tiêu thụ";
        ws.Cell(row, 5).Value = "Đơn vị";
        ws.Cell(row, 6).Value = "Hệ số phát thải (EF)";
        ws.Cell(row, 7).Value = "Đơn vị EF";
        ws.Cell(row, 8).Value = "Tổng phát thải (tCO2e)";
        ws.Cell(row, 9).Value = "Công thức áp dụng";

        var headerRange = ws.Range(row, 1, row, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.Black;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        int stt = 1;
        decimal totalScope1 = 0, totalScope2 = 0, totalScope3 = 0;

        foreach (var c in calculations.OrderBy(c => c.PhamVi))
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = c.PhamVi.ToUpper().Replace("_", " ");
            ws.Cell(row, 3).Value = c.IdDuLieuHoatDongNavigation?.IdNguonPhatThaiNavigation?.TenNguon ?? "";
            ws.Cell(row, 4).Value = c.SoLuongHoatDong;
            ws.Cell(row, 5).Value = c.DonViTinh;
            ws.Cell(row, 6).Value = c.IdHeSoPhatThaiNavigation?.TongHeSoCo2e ?? 0;
            ws.Cell(row, 7).Value = c.IdHeSoPhatThaiNavigation?.DonViHeSo ?? "";
            ws.Cell(row, 8).Value = c.TongPhatThaiTanCo2e;
            ws.Cell(row, 9).Value = c.CongThucApDung ?? "";

            if (c.PhamVi == "scope_1") totalScope1 += c.TongPhatThaiTanCo2e;
            else if (c.PhamVi == "scope_2") totalScope2 += c.TongPhatThaiTanCo2e;
            else if (c.PhamVi == "scope_3") totalScope3 += c.TongPhatThaiTanCo2e;
        }

        row++;
        ws.Cell(row, 2).Value = "TỔNG SCOPE 1 (Trực tiếp):";
        ws.Cell(row, 8).Value = totalScope1;
        ws.Range(row, 2, row, 7).Merge().Style.Font.Bold = true;

        row++;
        ws.Cell(row, 2).Value = "TỔNG SCOPE 2 (Điện lưới mua ngoài):";
        ws.Cell(row, 8).Value = totalScope2;
        ws.Range(row, 2, row, 7).Merge().Style.Font.Bold = true;

        row++;
        ws.Cell(row, 2).Value = "TỔNG SCOPE 3 (Gián tiếp khác):";
        ws.Cell(row, 8).Value = totalScope3;
        ws.Range(row, 2, row, 7).Merge().Style.Font.Bold = true;

        row++;
        ws.Cell(row, 2).Value = "TỔNG PHÁT THẢI TOÀN DOANH NGHIỆP (tCO2e):";
        ws.Cell(row, 8).Value = totalScope1 + totalScope2 + totalScope3;
        ws.Range(row, 2, row, 7).Merge().Style.Font.Bold = true;

        var dataTableRange = ws.Range(6, 1, row, 9);
        dataTableRange.Style.Font.FontColor = XLColor.Black;
        dataTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Columns().AdjustToContents();
    }

    private void GenerateGri302Sheet(XLWorkbook workbook, ToChuc toChuc, KyBaoCao kyBaoCao, List<KetQuaTinhToan> calculations)
    {
        var ws = workbook.Worksheets.Add("GRI_302_Nang_Luong");
        ws.Cell(1, 1).Value = "BÁO CÁO PHÂN TÍCH TIÊU THỤ NĂNG LƯỢNG - TIÊU CHUẨN GRI 302: ENERGY";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 13;
        ws.Range(1, 1, 1, 8).Merge();

        ws.Cell(2, 1).Value = $"Doanh nghiệp: {toChuc.TenToChuc} | Kỳ kiểm kê: {kyBaoCao.TenKy} ({kyBaoCao.Nam})";
        ws.Cell(3, 1).Value = "Tiêu chuẩn áp dụng: GRI 302-1 (Energy consumption within the organization)";

        int row = 5;
        ws.Cell(row, 1).Value = "Mã Chỉ Số GRI";
        ws.Cell(row, 2).Value = "Phân Loại Năng Lượng";
        ws.Cell(row, 3).Value = "Nguồn Tiêu Thụ";
        ws.Cell(row, 4).Value = "Lượng Tiêu Thụ Gốc";
        ws.Cell(row, 5).Value = "Đơn Vị Gốc";
        ws.Cell(row, 6).Value = "Quy Đổi Năng Lượng (GJ)";
        ws.Cell(row, 7).Value = "Tỷ Trọng Năng Lượng (%)";
        ws.Cell(row, 8).Value = "Ghi Chú Kỹ Thuật";

        var headerRange = ws.Range(row, 1, row, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.Black;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        decimal totalGJ = 0;
        var energyRows = new List<(string GriCode, string Category, string Source, decimal Qty, string Unit, decimal GjValue, string Note)>();

        foreach (var c in calculations)
        {
            var maNguon = c.IdDuLieuHoatDongNavigation?.IdNguonPhatThaiNavigation?.MaNguon ?? "";
            var tenNguon = c.IdDuLieuHoatDongNavigation?.IdNguonPhatThaiNavigation?.TenNguon ?? "";
            decimal qty = c.SoLuongHoatDong;
            decimal gj = 0;
            string category = "Nhiên liệu không tái tạo";
            string griCode = "GRI 302-1";
            string note = "";

            if (maNguon == "DIEN_LUOI")
            {
                category = "Điện năng mua ngoài (Electricity purchased)";
                gj = qty * 0.0036m; // 1 kWh = 0.0036 GJ
                note = "Hệ số chuẩn: 1 kWh = 3.6 MJ = 0.0036 GJ";
            }
            else if (maNguon == "DAU_DO")
            {
                category = "Nhiên liệu tĩnh (Dầu Diesel DO)";
                gj = qty * 0.0386m; // 1 lít DO ~ 38.6 MJ = 0.0386 GJ
                note = "Nhiệt trị ròng NCV (IPCC 2006): ~38.6 MJ/lít";
            }
            else if (maNguon == "XANG_RON95")
            {
                category = "Nhiên liệu di động (Xăng RON 95)";
                gj = qty * 0.0342m; // 1 lít xăng ~ 34.2 MJ = 0.0342 GJ
                note = "Nhiệt trị ròng NCV: ~34.2 MJ/lít";
            }
            else if (maNguon == "GAS_LPG")
            {
                category = "Khí hóa lỏng (LPG)";
                gj = qty * 0.0461m; // 1 kg LPG ~ 46.1 MJ = 0.0461 GJ
                note = "Nhiệt trị ròng: ~46.1 MJ/kg";
            }
            else
            {
                continue; // Không phải dạng năng lượng (vd: nước sạch)
            }

            totalGJ += gj;
            energyRows.Add((griCode, category, tenNguon, qty, c.DonViTinh, Math.Round(gj, 4), note));
        }

        foreach (var e in energyRows)
        {
            row++;
            decimal percentage = totalGJ > 0 ? Math.Round((e.GjValue / totalGJ) * 100, 2) : 0;
            ws.Cell(row, 1).Value = e.GriCode;
            ws.Cell(row, 2).Value = e.Category;
            ws.Cell(row, 3).Value = e.Source;
            ws.Cell(row, 4).Value = e.Qty;
            ws.Cell(row, 5).Value = e.Unit;
            ws.Cell(row, 6).Value = e.GjValue;
            ws.Cell(row, 7).Value = $"{percentage}%";
            ws.Cell(row, 8).Value = e.Note;
        }

        row++;
        ws.Cell(row, 2).Value = "TỔNG NĂNG LƯỢNG TIÊU THỤ NỘI BỘ (GJ):";
        ws.Cell(row, 6).Value = totalGJ;
        ws.Cell(row, 7).Value = "100%";
        ws.Range(row, 2, row, 5).Merge().Style.Font.Bold = true;

        var dataTableRange = ws.Range(5, 1, row, 8);
        dataTableRange.Style.Font.FontColor = XLColor.Black;
        dataTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Columns().AdjustToContents();
    }

    private async Task GenerateGri305Sheet(XLWorkbook workbook, ToChuc toChuc, KyBaoCao kyBaoCao, List<KetQuaTinhToan> calculations)
    {
        var ws = workbook.Worksheets.Add("GRI_305_Phat_Thai");
        ws.Cell(1, 1).Value = "BÁO CÁO PHÁT THẢI KHÍ NHÀ KÍNH - TIÊU CHUẨN BỀN VỮNG GRI 305: EMISSIONS";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 13;
        ws.Range(1, 1, 1, 9).Merge();

        ws.Cell(2, 1).Value = $"Đơn vị báo cáo: {toChuc.TenToChuc} | Kỳ kiểm kê: {kyBaoCao.TenKy} ({kyBaoCao.Nam})";
        ws.Cell(3, 1).Value = "Phân loại theo tiêu chuẩn: GRI 305-1 (Scope 1), GRI 305-2 (Scope 2), GRI 305-3 (Scope 3), GRI 305-4 (Intensity)";

        int row = 5;
        ws.Cell(row, 1).Value = "Mã Chỉ Số GRI";
        ws.Cell(row, 2).Value = "Phạm Vi Phát Thải";
        ws.Cell(row, 3).Value = "Nguồn Phát Thải";
        ws.Cell(row, 4).Value = "Khí CO₂ (kg)";
        ws.Cell(row, 5).Value = "Khí CH₄ (kg)";
        ws.Cell(row, 6).Value = "Khí N₂O (kg)";
        ws.Cell(row, 7).Value = "Tổng Tương Đương (tCO₂e)";
        ws.Cell(row, 8).Value = "Tỷ Trọng (%)";
        ws.Cell(row, 9).Value = "Phương Pháp & Chuẩn GWP";

        var headerRange = ws.Range(row, 1, row, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.Black;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        decimal totalScope1 = calculations.Where(c => c.PhamVi == "scope_1").Sum(c => c.TongPhatThaiTanCo2e);
        decimal totalScope2 = calculations.Where(c => c.PhamVi == "scope_2").Sum(c => c.TongPhatThaiTanCo2e);
        decimal totalScope3 = calculations.Where(c => c.PhamVi == "scope_3").Sum(c => c.TongPhatThaiTanCo2e);
        decimal grandTotal = totalScope1 + totalScope2 + totalScope3;

        foreach (var c in calculations.OrderBy(c => c.PhamVi))
        {
            row++;
            string griCode = c.PhamVi == "scope_1" ? "GRI 305-1" : c.PhamVi == "scope_2" ? "GRI 305-2" : "GRI 305-3";
            decimal pct = grandTotal > 0 ? Math.Round((c.TongPhatThaiTanCo2e / grandTotal) * 100, 2) : 0;

            ws.Cell(row, 1).Value = griCode;
            ws.Cell(row, 2).Value = c.PhamVi.ToUpper().Replace("_", " ");
            ws.Cell(row, 3).Value = c.IdDuLieuHoatDongNavigation?.IdNguonPhatThaiNavigation?.TenNguon ?? "";
            ws.Cell(row, 4).Value = c.PhatThaiCo2Kg ?? 0;
            ws.Cell(row, 5).Value = c.PhatThaiCh4Kg ?? 0;
            ws.Cell(row, 6).Value = c.PhatThaiN2oKg ?? 0;
            ws.Cell(row, 7).Value = c.TongPhatThaiTanCo2e;
            ws.Cell(row, 8).Value = $"{pct}%";
            ws.Cell(row, 9).Value = $"IPCC {c.IdHeSoPhatThaiNavigation?.IdGwpNavigation?.PhienBanIpcc ?? "AR5"} ({c.IdHeSoPhatThaiNavigation?.NguonGoc ?? "IPCC"})";
        }

        var dataTableRange = ws.Range(5, 1, row, 9);
        dataTableRange.Style.Font.FontColor = XLColor.Black;
        dataTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        row += 2;
        ws.Cell(row, 1).Value = "GRI 305-4: CƯỜNG ĐỘ PHÁT THẢI (GHG EMISSIONS INTENSITY)";
        ws.Range(row, 1, row, 9).Merge().Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.Black;

        var bizData = await _context.DuLieuSanXuatKinhDoanhs
            .FirstOrDefaultAsync(b => b.IdToChuc == toChuc.Id && b.IdKyBaoCao == kyBaoCao.Id);

        decimal sanLuong = bizData?.SanLuongSanPham ?? 0;
        decimal doanhThu = bizData?.DoanhThuTrieuDong ?? 0;
        decimal dienTich = toChuc.DienTichSanM2 ?? 0;

        int intensityStartRow = row;
        row++;
        ws.Cell(row, 2).Value = "Cường độ theo diện tích sàn:";
        ws.Cell(row, 7).Value = dienTich > 0 ? $"{Math.Round(grandTotal / dienTich, 4)} tCO₂e/m²" : "Chưa có dữ liệu";
        ws.Range(row, 2, row, 6).Merge();

        row++;
        ws.Cell(row, 2).Value = "Cường độ theo sản lượng sản phẩm:";
        ws.Cell(row, 7).Value = sanLuong > 0 ? $"{Math.Round(grandTotal / sanLuong, 4)} tCO₂e/sản phẩm" : "Chưa có dữ liệu";
        ws.Range(row, 2, row, 6).Merge();

        row++;
        ws.Cell(row, 2).Value = "Cường độ theo doanh thu:";
        ws.Cell(row, 7).Value = doanhThu > 0 ? $"{Math.Round(grandTotal / (doanhThu / 1000m), 4)} tCO₂e/tỷ đồng" : "Chưa có dữ liệu";
        ws.Range(row, 2, row, 6).Merge();

        var intensityRange = ws.Range(intensityStartRow, 1, row, 9);
        intensityRange.Style.Font.FontColor = XLColor.Black;
        intensityRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        intensityRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Columns().AdjustToContents();
    }

    private void GenerateAuditTrailSheet(XLWorkbook workbook, ToChuc toChuc, KyBaoCao kyBaoCao, List<KetQuaTinhToan> calculations)
    {
        var ws = workbook.Worksheets.Add("Doi_Chieu_Kiem_Toan");
        ws.Cell(1, 1).Value = "BẢNG ĐỐI CHIẾU KIỂM TOÁN VÀ TRUY VẾT DỮ LIỆU PHÁT THẢI (AUDIT TRAIL LOG)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 13;
        ws.Range(1, 1, 1, 12).Merge();

        ws.Cell(2, 1).Value = $"Cơ sở kiểm tra: {toChuc.TenToChuc} | Kỳ: {kyBaoCao.TenKy} ({kyBaoCao.Nam}) - Tiêu chuẩn xác thực ISO 14064-3";

        int row = 5;
        ws.Cell(row, 1).Value = "Mã Dữ Liệu";
        ws.Cell(row, 2).Value = "Nguồn Phát Thải";
        ws.Cell(row, 3).Value = "Lượng Nhập Gốc";
        ws.Cell(row, 4).Value = "Hệ Số Quy Đổi";
        ws.Cell(row, 5).Value = "Lượng Chuẩn Hóa";
        ws.Cell(row, 6).Value = "Hệ Số EF Áp Dụng";
        ws.Cell(row, 7).Value = "Nguồn Gốc EF";
        ws.Cell(row, 8).Value = "Tổng tCO₂e";
        ws.Cell(row, 9).Value = "Số Hóa Đơn VAT";
        ws.Cell(row, 10).Value = "File Minh Chứng";
        ws.Cell(row, 11).Value = "Người Phê Duyệt";
        ws.Cell(row, 12).Value = "Thời Gian Duyệt";

        var headerRange = ws.Range(row, 1, row, 12);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.Black;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        foreach (var c in calculations)
        {
            row++;
            var d = c.IdDuLieuHoatDongNavigation;
            var heSo = c.IdHeSoPhatThaiNavigation;
            var approval = d?.LichSuPheDuyets?.OrderByDescending(l => l.NgayThucHien).FirstOrDefault(l => l.HanhDong == "da_duyet");
            var proof = d?.FileMinhChungs?.FirstOrDefault();

            ws.Cell(row, 1).Value = $"ACT-{c.IdDuLieuHoatDong}";
            ws.Cell(row, 2).Value = d?.IdNguonPhatThaiNavigation?.TenNguon ?? "";
            ws.Cell(row, 3).Value = $"{d?.SoLuongNhap} {d?.DonViNhap}";
            ws.Cell(row, 4).Value = d?.HeSoQuyDoi ?? 1.0m;
            ws.Cell(row, 5).Value = $"{d?.SoLuongQuyChuan} {c.DonViTinh}";
            ws.Cell(row, 6).Value = $"{heSo?.TongHeSoCo2e} {heSo?.DonViHeSo}";
            ws.Cell(row, 7).Value = heSo?.NguonGoc ?? "";
            ws.Cell(row, 8).Value = c.TongPhatThaiTanCo2e;
            ws.Cell(row, 9).Value = d?.SoHoaDon ?? "Không có";
            ws.Cell(row, 10).Value = proof != null ? proof.TenFile : "Chưa đính kèm";
            ws.Cell(row, 11).Value = approval?.IdNguoiThucHienNavigation?.HoTen ?? "Hệ thống duyệt";
            ws.Cell(row, 12).Value = approval != null ? approval.NgayThucHien.AddHours(7).ToString("dd/MM/yyyy HH:mm") : "—";
        }

        var dataTableRange = ws.Range(5, 1, row, 12);
        dataTableRange.Style.Font.FontColor = XLColor.Black;
        dataTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Columns().AdjustToContents();
    }

    public async Task<List<BaoCaoLichSuDto>> GetReportHistoryAsync(long? toChucId = null)
    {
        var query = _context.BaoCaoXuats
            .Include(b => b.IdToChucNavigation)
            .Include(b => b.IdKyBaoCaoNavigation)
            .Include(b => b.IdNguoiXuatNavigation)
            .AsQueryable();

        if (toChucId.HasValue)
            query = query.Where(b => b.IdToChuc == toChucId.Value);

        var list = await query.OrderByDescending(b => b.NgayXuat).ToListAsync();

        return list.Select(b => new BaoCaoLichSuDto
        {
            Id = b.Id,
            TenToChuc = b.IdToChucNavigation?.TenToChuc ?? "",
            TenKyBaoCao = b.IdKyBaoCaoNavigation?.TenKy ?? "",
            TenNguoiXuat = b.IdNguoiXuatNavigation?.HoTen ?? "Quản trị viên",
            TenBaoCao = b.TenBaoCao,
            LoaiBaoCao = b.LoaiBaoCao,
            DinhDang = b.DinhDang,
            DuongDanFile = b.DuongDanFile,
            NgayXuat = b.NgayXuat
        }).ToList();
    }
}
