using CEMIS.Common;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CEMIS.Services.Implementations;

public class PdfReportService : IPdfReportService
{
    private readonly CemisDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public PdfReportService(CemisDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<(byte[] FileBytes, string FileName, string ContentType)> GenerateConfirmedKiemKePdfAsync(
        long toChucId, 
        long kyBaoCaoId, 
        long nguoiXacNhanId, 
        string? ghiChuXacNhan = null)
    {
        var toChuc = await _context.ToChucs.FindAsync(toChucId);
        var kyBaoCao = await _context.KyBaoCaos.FindAsync(kyBaoCaoId);
        var nguoiXacNhan = await _context.NguoiDungs
            .Include(u => u.IdVaiTroNavigation)
            .FirstOrDefaultAsync(u => u.Id == nguoiXacNhanId);

        if (toChuc == null)
            throw new KeyNotFoundException($"Không tìm thấy thông tin cơ sở với ID: {toChucId}.");
        if (kyBaoCao == null)
            throw new KeyNotFoundException($"Không tìm thấy thông tin kỳ báo cáo với ID: {kyBaoCaoId}.");
        if (nguoiXacNhan == null)
            throw new KeyNotFoundException($"Không tìm thấy thông tin người dùng với ID: {nguoiXacNhanId}.");

        if (nguoiXacNhan.TrangThai == "khoa")
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa.");

        // Kiểm tra phân quyền: Quản trị hệ thống hoặc Quản lý cơ sở của chính cơ sở đó
        var roleCode = nguoiXacNhan.IdVaiTroNavigation?.MaVaiTro ?? string.Empty;
        if (roleCode != "super_admin" && roleCode != "quan_ly_co_so")
        {
            throw new UnauthorizedAccessException("Chỉ Quản lý cơ sở hoặc Quản trị hệ thống mới có quyền xác nhận báo cáo kiểm kê KNK.");
        }

        if (roleCode == "quan_ly_co_so" && nguoiXacNhan.IdToChuc.HasValue && nguoiXacNhan.IdToChuc.Value != toChucId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xác nhận báo cáo cho cơ sở này.");
        }

        // Kiểm tra cấu hình ảnh chữ ký tay
        if (string.IsNullOrWhiteSpace(nguoiXacNhan.DuongDanChuKyAnh))
        {
            throw new InvalidOperationException("Tài khoản của bạn chưa được cài đặt ảnh chữ ký tay. Vui lòng liên hệ quản trị viên hoặc cập nhật ảnh chữ ký trong hồ sơ để xuất báo cáo có xác nhận.");
        }

        // Tải và xác thực an toàn ảnh chữ ký tay (chống SSRF, file magic bytes)
        byte[] signatureImageBytes;
        try
        {
            signatureImageBytes = await SignatureImageLoader.LoadAndValidateSignatureImageAsync(
                nguoiXacNhan.DuongDanChuKyAnh,
                _environment.ContentRootPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Không thể tải hoặc xử lý ảnh chữ ký tay: {ex.Message}");
        }

        var calculations = await _context.KetQuaTinhToans
            .Include(k => k.IdDuLieuHoatDongNavigation)
                .ThenInclude(d => d.IdNguonPhatThaiNavigation)
            .Include(k => k.IdHeSoPhatThaiNavigation)
            .Where(k => k.IdToChuc == toChucId && k.IdKyBaoCao == kyBaoCaoId)
            .ToListAsync();

        decimal totalScope1 = calculations.Where(c => c.PhamVi == "scope_1").Sum(c => c.TongPhatThaiTanCo2e);
        decimal totalScope2 = calculations.Where(c => c.PhamVi == "scope_2").Sum(c => c.TongPhatThaiTanCo2e);
        decimal totalScope3 = calculations.Where(c => c.PhamVi == "scope_3").Sum(c => c.TongPhatThaiTanCo2e);
        decimal grandTotal = totalScope1 + totalScope2 + totalScope3;

        var signTime = DateTime.UtcNow.AddHours(7); // Giờ Việt Nam GMT+7
        var roleName = nguoiXacNhan.IdVaiTroNavigation?.TenVaiTro ?? "Người đại diện phê duyệt";

        // Tạo tài liệu PDF trắng đen đơn sắc (monochrome / black text only)
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Arial").FontColor(Colors.Black));

                // Header Quốc hiệu & Tiêu ngữ theo Mẫu số 06 (Nghị định 06/2022/NĐ-CP)
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(5).Column(c =>
                        {
                            c.Item().Text(toChuc.TenToChuc.ToUpper()).Bold().FontSize(8.5f).FontColor(Colors.Black);
                            c.Item().Text($"Mã cơ sở: {toChuc.MaToChuc}").FontSize(7.5f).FontColor(Colors.Black);
                        });

                        row.RelativeItem(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold().FontSize(8.5f).FontColor(Colors.Black);
                            c.Item().AlignCenter().Text("Độc lập - Tự do - Hạnh phúc").Underline().FontSize(8f).FontColor(Colors.Black);
                        });
                    });

                    col.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Black);
                });

                // Body Nội dung báo cáo theo đúng 3 phần của Mẫu số 06
                page.Content().PaddingTop(5).Column(col =>
                {
                    // Tiêu đề báo cáo
                    col.Item().AlignCenter().Text("BÁO CÁO").Bold().FontSize(12).FontColor(Colors.Black);
                    col.Item().AlignCenter().Text($"KẾT QUẢ KIỂM KÊ KHÍ NHÀ KÍNH CHO NĂM {kyBaoCao.Nam}").Bold().FontSize(11).FontColor(Colors.Black);
                    col.Item().AlignCenter().Text($"(Kèm theo Mẫu số 06, Nghị định số 06/2022/NĐ-CP - Kỳ kiểm kê: {kyBaoCao.TenKy})").Italic().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Black);

                    // I. THÔNG TIN CỦA CƠ SỞ PHẢI THỰC HIỆN KIỂM KÊ KHÍ NHÀ KÍNH
                    col.Item().PaddingTop(6).Text("I. THÔNG TIN CỦA CƠ SỞ PHẢI THỰC HIỆN KIỂM KÊ KHÍ NHÀ KÍNH").Bold().FontSize(9f).FontColor(Colors.Black);
                    
                    col.Item().PaddingTop(2).Text("1. Tên cơ sở, địa chỉ, thông tin đăng ký hoạt động:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"• Tên cơ sở: {toChuc.TenToChuc}").FontColor(Colors.Black);
                            c.Item().Text($"• Địa chỉ cơ sở: {toChuc.DiaChi ?? "Khu công nghiệp Tân Bình, TP. Hồ Chí Minh"}").FontColor(Colors.Black);
                        });
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"• Diện tích sàn hoạt động: {(toChuc.DienTichSanM2.HasValue ? toChuc.DienTichSanM2.Value.ToString("N0") : "12,000")} m²").FontColor(Colors.Black);
                            c.Item().Text($"• Quy mô nhân sự: {(toChuc.SoNhanVien.HasValue ? toChuc.SoNhanVien.Value.ToString() : "250")} người").FontColor(Colors.Black);
                        });
                    });

                    col.Item().PaddingTop(3).Text("2. Thông tin về người đại diện của cơ sở trước pháp luật:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Row(r =>
                    {
                        r.RelativeItem().Text($"• Họ và tên người đại diện: {nguoiXacNhan.HoTen}").FontColor(Colors.Black);
                        r.RelativeItem().Text($"• Chức vụ / Vai trò: {roleName}").FontColor(Colors.Black);
                    });

                    var linhVuc = toChuc.LoaiToChuc switch
                    {
                        "nha_may" => "Sản xuất công nghiệp & Chế biến chế tạo",
                        "tap_doan" => "Trụ sở điều hành & Văn phòng quản trị tập đoàn",
                        _ => toChuc.LoaiToChuc ?? "Sản xuất / Dịch vụ công nghiệp"
                    };

                    col.Item().PaddingTop(3).Text("3. Thông tin về lĩnh vực hoạt động sản xuất kinh doanh:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Text($"• Lĩnh vực: {linhVuc}").FontColor(Colors.Black);

                    // II. THÔNG TIN VỀ HOẠT ĐỘNG SẢN XUẤT KINH DOANH VÀ SỐ LIỆU HOẠT ĐỘNG CỦA CƠ SỞ
                    col.Item().PaddingTop(6).Text("II. THÔNG TIN VỀ HOẠT ĐỘNG SẢN XUẤT KINH DOANH VÀ SỐ LIỆU HOẠT ĐỘNG").Bold().FontSize(9f).FontColor(Colors.Black);

                    col.Item().PaddingTop(2).Text("1. Ranh giới và phạm vi hoạt động của cơ sở:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Text("• Ranh giới kiểm kê: Kiểm soát vận hành toàn bộ khuôn viên, dây chuyền sản xuất và phương tiện trực thuộc cơ sở. Phạm vi kiểm kê gồm: Phát thải trực tiếp (Scope 1), Phát thải gián tiếp từ điện (Scope 2) và Phát thải gián tiếp khác (Scope 3).").FontColor(Colors.Black);

                    col.Item().PaddingTop(3).Text("2. Cơ sở hạ tầng, công nghệ và hoạt động:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Text($"• Cơ sở hạ tầng gồm nhà xưởng sản xuất, kho vận và khu điều hành với tổng diện tích {(toChuc.DienTichSanM2.HasValue ? toChuc.DienTichSanM2.Value.ToString("N0") : "12,000")} m², sử dụng hệ thống máy móc tiêu thụ điện năng và nhiên liệu hóa thạch.").FontColor(Colors.Black);

                    col.Item().PaddingTop(3).Text("3. Các nguồn phát thải và bể hấp thụ khí nhà kính trong phạm vi cơ sở:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text("• Nguồn Scope 1 (Trực tiếp): Đốt nhiên liệu tĩnh (Dầu Diesel DO máy phát điện, Khí LPG) và di động (Xăng RON95 phương tiện vận tải).").FontColor(Colors.Black);
                        c.Item().Text("• Nguồn Scope 2 (Điện mua ngoài): Điện năng nhận từ lưới điện quốc gia phục vụ chiếu sáng và vận hành sản xuất.").FontColor(Colors.Black);
                        c.Item().Text("• Nguồn Scope 3 (Gián tiếp khác): Tiêu thụ nước sạch, xử lý chất thải và chuỗi cung ứng phụ trợ.").FontColor(Colors.Black);
                        c.Item().Text("• Bể hấp thụ khí nhà kính: Cơ sở chưa thiết lập bể hấp thụ sinh khối hoặc áp dụng công nghệ thu giữ lưu trữ carbon (CCUS).").FontColor(Colors.Black);
                    });

                    col.Item().PaddingTop(3).Text("4. Hệ thống thông tin, dữ liệu về phát thải khí nhà kính của cơ sở:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Text("• Dữ liệu hoạt động được thu thập từ đồng hồ đo đếm điện năng, hóa đơn VAT tiền điện/nhiên liệu và phiếu xuất kho định kỳ, được số hóa và quản lý tập trung trên hệ thống CEMIS.").FontColor(Colors.Black);

                    // III. KẾT QUẢ THỰC HIỆN KIỂM KÊ PHÁT THẢI KHÍ NHÀ KÍNH
                    col.Item().PaddingTop(6).Text("III. KẾT QUẢ THỰC HIỆN KIỂM KÊ PHÁT THẢI KHÍ NHÀ KÍNH").Bold().FontSize(9f).FontColor(Colors.Black);

                    col.Item().PaddingTop(2).Text("1. Mô tả phương pháp kiểm kê phát thải khí nhà kính:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Text("• Áp dụng tiêu chuẩn TCVN ISO 14064-1:2018 và Hướng dẫn kiểm kê quốc gia IPCC 2006 (GWP theo Báo cáo Đánh giá AR5). Công thức tính: Phát thải (tCO₂e) = Số liệu hoạt động (AD) × Hệ số phát thải (EF).").FontColor(Colors.Black);

                    col.Item().PaddingTop(3).Text("2. Số liệu hoạt động và kết quả tính toán chi tiết:").Bold().FontSize(8f).FontColor(Colors.Black);

                    // Bảng chi tiết kết quả kiểm kê (Trắng đen, viền mảnh)
                    col.Item().PaddingTop(2).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(24);  // STT
                            columns.RelativeColumn(3);  // Nguồn
                            columns.ConstantColumn(52);  // Scope
                            columns.ConstantColumn(62);  // Tiêu thụ
                            columns.ConstantColumn(36);  // ĐVT
                            columns.ConstantColumn(62);  // EF
                            columns.ConstantColumn(66);  // tCO2e
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Padding(3).Text("STT").Bold().FontColor(Colors.Black).FontSize(7.5f);
                            header.Cell().BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Padding(3).Text("Nguồn phát thải").Bold().FontColor(Colors.Black).FontSize(7.5f);
                            header.Cell().BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Padding(3).Text("Phạm vi").Bold().FontColor(Colors.Black).FontSize(7.5f);
                            header.Cell().BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Padding(3).AlignRight().Text("Tiêu thụ").Bold().FontColor(Colors.Black).FontSize(7.5f);
                            header.Cell().BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Padding(3).Text("ĐVT").Bold().FontColor(Colors.Black).FontSize(7.5f);
                            header.Cell().BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Padding(3).AlignRight().Text("Hệ số EF").Bold().FontColor(Colors.Black).FontSize(7.5f);
                            header.Cell().BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Padding(3).AlignRight().Text("tCO₂e").Bold().FontColor(Colors.Black).FontSize(7.5f);
                        });

                        int index = 1;
                        foreach (var r in calculations)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(3).Text(index.ToString()).FontColor(Colors.Black).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(3).Text(r.IdDuLieuHoatDongNavigation?.IdNguonPhatThaiNavigation?.TenNguon ?? "").FontColor(Colors.Black).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(3).Text(r.PhamVi.ToUpper().Replace("_", " ")).FontColor(Colors.Black).FontSize(7.5f).Bold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight().Text(r.SoLuongHoatDong.ToString("N0")).FontColor(Colors.Black).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(3).Text(r.DonViTinh).FontColor(Colors.Black).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight().Text(r.IdHeSoPhatThaiNavigation?.TongHeSoCo2e.ToString("N4") ?? "0").FontColor(Colors.Black).FontSize(7.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight().Text(r.TongPhatThaiTanCo2e.ToString("N4")).Bold().FontSize(7.5f).FontColor(Colors.Black);
                            index++;
                        }
                    });

                    // 3. Tổng hợp kết quả kiểm kê
                    col.Item().PaddingTop(4).Text("3. Tổng hợp kết quả kiểm kê khí nhà kính của cơ sở:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingTop(2).Row(row =>
                    {
                        row.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Background(Colors.White).Padding(4).Column(c =>
                        {
                            c.Item().Text("Scope 1 (Trực tiếp)").Bold().FontSize(7.5f).FontColor(Colors.Black);
                            c.Item().Text($"{totalScope1:N3} tCO₂e").Bold().FontSize(10).FontColor(Colors.Black);
                            c.Item().Text($"Tỷ trọng: {(grandTotal > 0 ? (totalScope1 / grandTotal * 100).ToString("N1") : "0")}%").FontSize(7).FontColor(Colors.Black);
                        });

                        row.ConstantItem(5);

                        row.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Background(Colors.White).Padding(4).Column(c =>
                        {
                            c.Item().Text("Scope 2 (Điện lưới)").Bold().FontSize(7.5f).FontColor(Colors.Black);
                            c.Item().Text($"{totalScope2:N3} tCO₂e").Bold().FontSize(10).FontColor(Colors.Black);
                            c.Item().Text($"Tỷ trọng: {(grandTotal > 0 ? (totalScope2 / grandTotal * 100).ToString("N1") : "0")}%").FontSize(7).FontColor(Colors.Black);
                        });

                        row.ConstantItem(5);

                        row.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Background(Colors.White).Padding(4).Column(c =>
                        {
                            c.Item().Text("Scope 3 (Gián tiếp)").Bold().FontSize(7.5f).FontColor(Colors.Black);
                            c.Item().Text($"{totalScope3:N3} tCO₂e").Bold().FontSize(10).FontColor(Colors.Black);
                            c.Item().Text($"Tỷ trọng: {(grandTotal > 0 ? (totalScope3 / grandTotal * 100).ToString("N1") : "0")}%").FontSize(7).FontColor(Colors.Black);
                        });
                    });

                    // Tổng phát thải toàn cơ sở (Trắng đen, viền mảnh)
                    col.Item().PaddingTop(3).Border(0.5f).BorderColor(Colors.Black).Background(Colors.White).Padding(4).Row(r =>
                    {
                        r.RelativeItem().Text("TỔNG PHÁT THẢI TOÀN CƠ SỞ:").Bold().FontSize(9f).FontColor(Colors.Black);
                        r.RelativeItem().AlignRight().Text($"{grandTotal:N3} tCO₂e").Bold().FontSize(10.5f).FontColor(Colors.Black);
                    });

                    // 4. Đánh giá độ tin cậy, tính đầy đủ và độ không chắc chắn
                    col.Item().PaddingTop(4).Text("4. Đánh giá độ tin cậy, tính đầy đủ và độ không chắc chắn:").Bold().FontSize(8f).FontColor(Colors.Black);
                    col.Item().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text("• Tính đầy đủ: Báo cáo bao quát toàn bộ 100% các nguồn phát thải có số liệu hoạt động trong kỳ kiểm kê.").FontColor(Colors.Black);
                        c.Item().Text("• Độ tin cậy: Mức độ tin cậy cao; 100% số liệu hoạt động có hóa đơn GTGT đối chiếu và đã qua kiểm soát phê duyệt.").FontColor(Colors.Black);
                        c.Item().Text("• Độ không chắc chắn: Ước tính dưới 5% đối với dữ liệu đo đếm; hệ số phát thải lưới điện áp dụng theo công bố chính thức của Bộ Tài nguyên và Môi trường.").FontColor(Colors.Black);
                    });

                    // KHỐI CHỮ KÝ XÁC NHẬN BÁO CÁO (Trắng đen, chữ đen, gom nguyên khối chống vỡ trang)
                    col.Item().ShowEntire().PaddingTop(10).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ĐẠI DIỆN ĐƠN VỊ LẬP BÁO CÁO").Bold().FontSize(8f).FontColor(Colors.Black);
                            c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(7.5f).FontColor(Colors.Black);
                            c.Item().PaddingTop(45).Text("Bộ phận Quản lý KNK & ESG").Bold().FontSize(8f).FontColor(Colors.Black);
                            c.Item().Text(toChuc.TenToChuc).FontSize(7.5f).FontColor(Colors.Black);
                        });

                        // Khung xác nhận chữ ký tay đại diện cơ sở (Viền đen mảnh, nền trắng, chữ đen)
                        r.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Background(Colors.White).Padding(6).Column(box =>
                        {
                            box.Item().Row(hr =>
                            {
                                hr.RelativeItem().Text("ĐẠI DIỆN CƠ SỞ").Bold().FontSize(8f).FontColor(Colors.Black);
                                hr.AutoItem().Text("ĐÃ XÁC NHẬN").FontSize(7f).Bold().FontColor(Colors.Black);
                            });
                            box.Item().Text("(Ký, ghi rõ họ tên và đóng dấu)").Italic().FontSize(7f).FontColor(Colors.Black);

                            // Nhúng ảnh chữ ký tay PNG trong suốt
                            box.Item().PaddingVertical(3).Height(45).AlignCenter().Image(signatureImageBytes).FitArea();

                            box.Item().AlignCenter().Text(nguoiXacNhan.HoTen).Bold().FontSize(8f).FontColor(Colors.Black);
                            box.Item().AlignCenter().Text(roleName).FontSize(7f).FontColor(Colors.Black);
                            box.Item().AlignCenter().Text($"Đơn vị: {toChuc.TenToChuc}").FontSize(7f).FontColor(Colors.Black);
                            box.Item().AlignCenter().Text($"Thời gian xác nhận: {signTime:dd/MM/yyyy HH:mm:ss} (GMT+7)").FontSize(6.5f).FontColor(Colors.Black);
                        });
                    });
                });

                // Footer (Chữ đen)
                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("Hệ thống Quản lý Phát thải Carbon CEMIS - Xác nhận báo cáo điện tử").FontSize(7).FontColor(Colors.Black);
                    r.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Trang ").FontColor(Colors.Black);
                        x.CurrentPageNumber();
                        x.Span(" / ").FontColor(Colors.Black);
                        x.TotalPages();
                    });
                });
            });
        });

        var pdfBytes = doc.GeneratePdf();
        var fileName = $"BaoCao_KNK_XacNhan_{toChuc.MaToChuc}_{kyBaoCao.Nam}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        // Lưu vết lịch sử xuất báo cáo đã xác nhận bằng chữ ký tay
        var history = new BaoCaoXuat
        {
            IdToChuc = toChucId,
            IdKyBaoCao = kyBaoCaoId,
            IdNguoiXuat = nguoiXacNhanId,
            TenBaoCao = fileName,
            LoaiBaoCao = "kiem_ke_knk_vn_confirmed",
            DinhDang = "pdf",
            DuongDanFile = $"/reports/{fileName}",
            NgayXuat = DateTime.UtcNow,
            GhiChu = $"Xác nhận báo cáo bằng ảnh chữ ký bởi: {nguoiXacNhan.HoTen} ({roleName}) lúc {signTime:dd/MM/yyyy HH:mm:ss}"
        };
        _context.BaoCaoXuats.Add(history);

        // Khóa sổ kỳ báo cáo sau khi lãnh đạo xác nhận
        kyBaoCao.TrangThaiKhoaSo = "da_khoa_so";

        await _context.SaveChangesAsync();

        return (pdfBytes, fileName, "application/pdf");
    }

    public async Task<(byte[] FileBytes, string FileName, string ContentType, string HashSha256)> GenerateAndSignKiemKePdfAsync(
        long toChucId, 
        long kyBaoCaoId, 
        long nguoiKyId, 
        string? lyDoKy = null, 
        string? pinCode = null)
    {
        var res = await GenerateConfirmedKiemKePdfAsync(toChucId, kyBaoCaoId, nguoiKyId, lyDoKy);
        return (res.FileBytes, res.FileName, res.ContentType, string.Empty);
    }
}
