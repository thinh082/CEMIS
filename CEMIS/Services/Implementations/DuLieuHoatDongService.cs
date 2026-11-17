using ClosedXML.Excel;
using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Services.Implementations;

public class DuLieuHoatDongService : IDuLieuHoatDongService
{
    private readonly CemisDbContext _context;
    private readonly ICalculationEngineService _calculationService;
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentUserAccess _currentUserAccess;

    public DuLieuHoatDongService(
        CemisDbContext context,
        ICalculationEngineService calculationService,
        IWebHostEnvironment environment,
        ICurrentUserAccess currentUserAccess)
    {
        _context = context;
        _calculationService = calculationService;
        _environment = environment;
        _currentUserAccess = currentUserAccess;
    }

    public async Task<List<KyBaoCaoDto>> GetKyBaoCaoListAsync(long? toChucId = null, int? nam = null)
    {
        var effectiveToChucId = await _currentUserAccess.GetEffectiveOrganizationIdAsync(toChucId);

        var query = _context.KyBaoCaos
            .Include(k => k.IdToChucNavigation)
            .AsQueryable();

        if (effectiveToChucId.HasValue)
            query = query.Where(k => k.IdToChuc == effectiveToChucId.Value);

        if (nam.HasValue)
            query = query.Where(k => k.Nam == nam.Value);

        var list = await query.OrderByDescending(k => k.Nam).ThenByDescending(k => k.Thang).ToListAsync();

        var dtos = new List<KyBaoCaoDto>();
        foreach (var k in list)
        {
            var tongPhatThai = await _context.KetQuaTinhToans
                .Where(kq => kq.IdKyBaoCao == k.Id)
                .SumAsync(kq => (decimal?)kq.TongPhatThaiTanCo2e) ?? 0;

            dtos.Add(new KyBaoCaoDto
            {
                Id = k.Id,
                IdToChuc = k.IdToChuc,
                TenToChuc = k.IdToChucNavigation.TenToChuc,
                TenKy = k.TenKy,
                Thang = k.Thang,
                Quy = k.Quy,
                Nam = k.Nam,
                NgayBatDau = k.NgayBatDau,
                NgayKetThuc = k.NgayKetThuc,
                TrangThaiKhoaSo = k.TrangThaiKhoaSo,
                TongPhatThaiTanCo2e = tongPhatThai
            });
        }

        return dtos;
    }

    public async Task<KyBaoCaoDto> CreateKyBaoCaoAsync(CreateKyBaoCaoDto dto)
    {
        await _currentUserAccess.AssertCanApproveAsync(dto.IdToChuc);

        var entity = new KyBaoCao
        {
            IdToChuc = dto.IdToChuc,
            TenKy = dto.TenKy,
            Thang = dto.Thang,
            Quy = dto.Quy,
            Nam = dto.Nam,
            NgayBatDau = dto.NgayBatDau,
            NgayKetThuc = dto.NgayKetThuc,
            TrangThaiKhoaSo = "mo_so"
        };

        _context.KyBaoCaos.Add(entity);
        await _context.SaveChangesAsync();

        var toChuc = await _context.ToChucs.FindAsync(dto.IdToChuc);

        return new KyBaoCaoDto
        {
            Id = entity.Id,
            IdToChuc = entity.IdToChuc,
            TenToChuc = toChuc?.TenToChuc ?? "",
            TenKy = entity.TenKy,
            Thang = entity.Thang,
            Quy = entity.Quy,
            Nam = entity.Nam,
            NgayBatDau = entity.NgayBatDau,
            NgayKetThuc = entity.NgayKetThuc,
            TrangThaiKhoaSo = entity.TrangThaiKhoaSo,
            TongPhatThaiTanCo2e = 0
        };
    }

    public async Task<bool> ToggleKhoaSoAsync(long kyBaoCaoId, string trangThai)
    {
        var ky = await _context.KyBaoCaos.FindAsync(kyBaoCaoId);
        if (ky == null) return false;

        await _currentUserAccess.AssertCanApproveAsync(ky.IdToChuc);

        ky.TrangThaiKhoaSo = trangThai; // 'mo_so', 'da_khoa_so'
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DuLieuHoatDongDto>> GetDuLieuListAsync(
        long? toChucId = null,
        long? kyBaoCaoId = null,
        string? phamVi = null,
        string? trangThaiDuyet = null)
    {
        var effectiveToChucId = await _currentUserAccess.GetEffectiveOrganizationIdAsync(toChucId);

        var query = _context.DuLieuHoatDongs
            .Include(d => d.IdToChucNavigation)
            .Include(d => d.IdKyBaoCaoNavigation)
            .Include(d => d.IdNguonPhatThaiNavigation)
            .Include(d => d.IdNguoiNhapNavigation)
            .Include(d => d.KetQuaTinhToan)
                .ThenInclude(k => k.IdHeSoPhatThaiNavigation)
            .Include(d => d.FileMinhChungs)
            .AsQueryable();

        if (effectiveToChucId.HasValue)
            query = query.Where(d => d.IdToChuc == effectiveToChucId.Value);

        if (kyBaoCaoId.HasValue)
            query = query.Where(d => d.IdKyBaoCao == kyBaoCaoId.Value);

        if (!string.IsNullOrEmpty(phamVi))
            query = query.Where(d => d.IdNguonPhatThaiNavigation.PhamVi == phamVi);

        if (!string.IsNullOrEmpty(trangThaiDuyet))
            query = query.Where(d => d.TrangThaiDuyet == trangThaiDuyet);

        var list = await query.OrderByDescending(d => d.Id).ToListAsync();

        return list.Select(MapToDto).ToList();
    }

    public async Task<DuLieuHoatDongDto> GetDuLieuByIdAsync(long id)
    {
        var entity = await _context.DuLieuHoatDongs
            .Include(d => d.IdToChucNavigation)
            .Include(d => d.IdKyBaoCaoNavigation)
            .Include(d => d.IdNguonPhatThaiNavigation)
            .Include(d => d.IdNguoiNhapNavigation)
            .Include(d => d.KetQuaTinhToan)
                .ThenInclude(k => k.IdHeSoPhatThaiNavigation)
            .Include(d => d.FileMinhChungs)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy bản ghi dữ liệu hoạt động.");

        await _currentUserAccess.AssertCanAccessOrganizationAsync(entity.IdToChuc);

        return MapToDto(entity);
    }

    public async Task<DuLieuHoatDongDto> CreateDuLieuAsync(CreateDuLieuHoatDongDto dto, long nguoiNhapId)
    {
        await _currentUserAccess.AssertCanModifyDataAsync(dto.IdToChuc);

        var ky = await _context.KyBaoCaos.FindAsync(dto.IdKyBaoCao);
        if (ky == null)
            throw new KeyNotFoundException("Không tìm thấy kỳ báo cáo.");

        if (ky.IdToChuc != dto.IdToChuc)
            throw new InvalidOperationException("Kỳ báo cáo không thuộc cơ sở đang kê khai.");

        if (ky.TrangThaiKhoaSo == "da_khoa_so")
        {
            throw new InvalidOperationException("Kỳ báo cáo này đã khóa sổ, không thể thêm mới dữ liệu.");
        }

        var currentUserId = await _currentUserAccess.GetUserIdAsync();

        var nguon = await _context.DanhMucNguonPhatThais.FindAsync(dto.IdNguonPhatThai);
        if (nguon == null)
            throw new KeyNotFoundException("Không tìm thấy nguồn phát thải đã chọn.");

        var (heSoQuyDoi, soLuongQuyChuan) = UnitConversionHelper.Convert(dto.SoLuongNhap, dto.DonViNhap, nguon.DonViTinhChuan);

        var entity = new DuLieuHoatDong
        {
            IdToChuc = dto.IdToChuc,
            IdKyBaoCao = dto.IdKyBaoCao,
            IdNguonPhatThai = dto.IdNguonPhatThai,
            IdNguoiNhap = currentUserId,
            SoLuongNhap = dto.SoLuongNhap,
            DonViNhap = string.IsNullOrWhiteSpace(dto.DonViNhap) ? nguon.DonViTinhChuan : dto.DonViNhap.Trim(),
            HeSoQuyDoi = heSoQuyDoi,
            SoLuongQuyChuan = soLuongQuyChuan,
            NgaySuDung = dto.NgaySuDung,
            SoHoaDon = dto.SoHoaDon,
            NgayHoaDon = dto.NgayHoaDon,
            NhaCungCap = dto.NhaCungCap,
            TrangThaiDuyet = "cho_duyet",
            GhiChu = dto.GhiChu
        };

        _context.DuLieuHoatDongs.Add(entity);
        await _context.SaveChangesAsync();

        return await GetDuLieuByIdAsync(entity.Id);
    }

    public async Task<DuLieuHoatDongDto> UpdateDuLieuAsync(long id, UpdateDuLieuHoatDongDto dto)
    {
        var entity = await _context.DuLieuHoatDongs
            .Include(d => d.IdKyBaoCaoNavigation)
            .Include(d => d.IdNguonPhatThaiNavigation)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy dữ liệu hoạt động.");

        await _currentUserAccess.AssertCanModifyDataAsync(entity.IdToChuc);

        if (entity.TrangThaiDuyet == "da_duyet")
        {
            throw new InvalidOperationException("Dữ liệu đã được phê duyệt, không thể chỉnh sửa.");
        }

        if (entity.IdKyBaoCaoNavigation?.TrangThaiKhoaSo == "da_khoa_so")
        {
            throw new InvalidOperationException("Kỳ báo cáo đã khóa sổ, không thể chỉnh sửa.");
        }

        var donViChuan = entity.IdNguonPhatThaiNavigation?.DonViTinhChuan ?? entity.DonViNhap;
        var (heSoQuyDoi, soLuongQuyChuan) = UnitConversionHelper.Convert(dto.SoLuongNhap, dto.DonViNhap, donViChuan);

        entity.SoLuongNhap = dto.SoLuongNhap;
        entity.DonViNhap = string.IsNullOrWhiteSpace(dto.DonViNhap) ? donViChuan : dto.DonViNhap.Trim();
        entity.HeSoQuyDoi = heSoQuyDoi;
        entity.SoLuongQuyChuan = soLuongQuyChuan;
        entity.NgaySuDung = dto.NgaySuDung;
        entity.SoHoaDon = dto.SoHoaDon;
        entity.NgayHoaDon = dto.NgayHoaDon;
        entity.NhaCungCap = dto.NhaCungCap;
        entity.GhiChu = dto.GhiChu;

        // Nếu đã được duyệt trước đó, tính toán lại
        if (entity.TrangThaiDuyet == "da_duyet")
        {
            await _calculationService.CalculateEmissionAsync(entity.Id);
        }

        await _context.SaveChangesAsync();
        return await GetDuLieuByIdAsync(id);
    }

    public async Task<bool> DeleteDuLieuAsync(long id)
    {
        var entity = await _context.DuLieuHoatDongs
            .Include(d => d.IdKyBaoCaoNavigation)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (entity == null) return false;

        await _currentUserAccess.AssertCanModifyDataAsync(entity.IdToChuc);

        if (entity.TrangThaiDuyet == "da_duyet")
        {
            throw new InvalidOperationException("Dữ liệu đã được phê duyệt, không thể xóa.");
        }

        if (entity.IdKyBaoCaoNavigation?.TrangThaiKhoaSo == "da_khoa_so")
        {
            throw new InvalidOperationException("Kỳ báo cáo đã khóa sổ, không thể xóa dữ liệu.");
        }

        _context.DuLieuHoatDongs.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PheDuyetDuLieuAsync(long id, PheDuyetRequestDto dto, long nguoiThucHienId)
    {
        var entity = await _context.DuLieuHoatDongs
            .Include(d => d.IdKyBaoCaoNavigation)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy dữ liệu hoạt động.");

        await _currentUserAccess.AssertCanApproveAsync(entity.IdToChuc);

        if (entity.IdKyBaoCaoNavigation?.TrangThaiKhoaSo == "da_khoa_so")
        {
            throw new InvalidOperationException("Kỳ báo cáo này đã khóa sổ, không thể phê duyệt hoặc từ chối dữ liệu.");
        }

        string action = dto.HanhDong?.Trim().ToLower() ?? "";
        string targetStatus;
        if (action == "phe_duyet" || action == "da_duyet")
        {
            targetStatus = "da_duyet";
        }
        else if (action == "tu_choi")
        {
            if (string.IsNullOrWhiteSpace(dto.LyDoTuChoi))
                throw new ArgumentException("Vui lòng cung cấp lý do từ chối bản ghi.");
            targetStatus = "tu_choi";
        }
        else
        {
            targetStatus = "cho_duyet";
        }

        entity.TrangThaiDuyet = targetStatus;

        var currentUserId = await _currentUserAccess.GetUserIdAsync();

        // Ghi log lịch sử phê duyệt
        var log = new LichSuPheDuyet
        {
            IdDuLieuHoatDong = entity.Id,
            IdNguoiThucHien = currentUserId,
            HanhDong = targetStatus,
            NgayThucHien = DateTime.UtcNow,
            LyDoTuChoi = dto.LyDoTuChoi,
            GhiChu = dto.GhiChu
        };
        _context.LichSuPheDuyets.Add(log);

        await _context.SaveChangesAsync();

        // Nếu được duyệt, tự động kích hoạt Động cơ tính toán phát thải
        if (targetStatus == "da_duyet")
        {
            await _calculationService.CalculateEmissionAsync(entity.Id);
        }

        return true;
    }

    public async Task<FileMinhChungDto> UploadMinhChungAsync(long duLieuId, IFormFile file, string? ghiChu)
    {
        var duLieu = await _context.DuLieuHoatDongs
            .Include(d => d.IdKyBaoCaoNavigation)
            .FirstOrDefaultAsync(d => d.Id == duLieuId);
        if (duLieu == null)
            throw new KeyNotFoundException("Không tìm thấy dữ liệu hoạt động.");

        await _currentUserAccess.AssertCanModifyDataAsync(duLieu.IdToChuc);

        if (duLieu.IdKyBaoCaoNavigation?.TrangThaiKhoaSo == "da_khoa_so")
        {
            throw new InvalidOperationException("Kỳ báo cáo này đã khóa sổ, không thể tải thêm minh chứng.");
        }

        if (file == null || file.Length == 0)
            throw new ArgumentException("Tệp tin không được rỗng.");

        if (file.Length > 10 * 1024 * 1024)
            throw new InvalidOperationException("Dung lượng file vượt quá giới hạn cho phép (tối đa 10MB).");

        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
        var allowedExts = new[] { "pdf", "jpg", "jpeg", "png", "xlsx", "xls", "doc", "docx" };
        if (!allowedExts.Contains(ext))
            throw new InvalidOperationException($"Định dạng file '.{ext}' không được hỗ trợ. Vui lòng chọn tệp PDF, Ảnh (JPG, PNG), Excel hoặc Word.");

        // Lưu trữ an toàn ngoài wwwroot để ngăn ngừa truy cập tĩnh không xác thực
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "proofs");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/proofs/{fileName}";

        var minhChung = new FileMinhChung
        {
            IdDuLieuHoatDong = duLieuId,
            TenFile = file.FileName,
            DuongDanFile = relativePath,
            LoaiFile = ext,
            DungLuongKb = file.Length / 1024,
            GhiChu = ghiChu
        };

        _context.FileMinhChungs.Add(minhChung);
        await _context.SaveChangesAsync();

        return new FileMinhChungDto
        {
            Id = minhChung.Id,
            IdDuLieuHoatDong = minhChung.IdDuLieuHoatDong,
            TenFile = minhChung.TenFile,
            DuongDanFile = minhChung.DuongDanFile,
            LoaiFile = minhChung.LoaiFile,
            DungLuongKb = minhChung.DungLuongKb,
            GhiChu = minhChung.GhiChu
        };
    }

    public async Task<int> BatchImportExcelAsync(long toChucId, long kyBaoCaoId, long nguoiNhapId, IFormFile excelFile)
    {
        await _currentUserAccess.AssertCanModifyDataAsync(toChucId);

        var ky = await _context.KyBaoCaos.FindAsync(kyBaoCaoId);
        if (ky == null)
            throw new KeyNotFoundException("Không tìm thấy kỳ báo cáo.");

        if (ky.IdToChuc != toChucId)
            throw new InvalidOperationException("Kỳ báo cáo không thuộc cơ sở đã chọn.");

        if (ky.TrangThaiKhoaSo == "da_khoa_so")
        {
            throw new InvalidOperationException("Kỳ báo cáo này đã khóa sổ, không thể import dữ liệu.");
        }

        var currentUserId = await _currentUserAccess.GetUserIdAsync();

        int count = 0;
        using var stream = new MemoryStream();
        await excelFile.CopyToAsync(stream);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return 0;

        var range = worksheet.RangeUsed();
        if (range == null) return 0;
        var rows = range.RowsUsed().Skip(1); // Bỏ qua Header row

        var danhMucNguon = await _context.DanhMucNguonPhatThais.ToListAsync();

        foreach (var row in rows)
        {
            var maNguon = row.Cell(1).GetString().Trim();
            var soLuong = row.Cell(2).GetValue<decimal>();
            var donVi = row.Cell(3).GetString().Trim();
            var soHoaDon = row.Cell(4).GetString().Trim();
            var nhaCungCap = row.Cell(5).GetString().Trim();

            var nguon = danhMucNguon.FirstOrDefault(n => n.MaNguon.Equals(maNguon, StringComparison.OrdinalIgnoreCase));
            if (nguon != null && soLuong > 0)
            {
                var donViNhap = string.IsNullOrEmpty(donVi) ? nguon.DonViTinhChuan : donVi;
                var (heSoQuyDoi, soLuongQuyChuan) = UnitConversionHelper.Convert(soLuong, donViNhap, nguon.DonViTinhChuan);

                var entity = new DuLieuHoatDong
                {
                    IdToChuc = toChucId,
                    IdKyBaoCao = kyBaoCaoId,
                    IdNguonPhatThai = nguon.Id,
                    IdNguoiNhap = currentUserId,
                    SoLuongNhap = soLuong,
                    DonViNhap = donViNhap,
                    HeSoQuyDoi = heSoQuyDoi,
                    SoLuongQuyChuan = soLuongQuyChuan,
                    NgaySuDung = DateOnly.FromDateTime(DateTime.UtcNow),
                    SoHoaDon = soHoaDon,
                    NhaCungCap = nhaCungCap,
                    TrangThaiDuyet = "cho_duyet"
                };
                _context.DuLieuHoatDongs.Add(entity);
                count++;
            }
        }

        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<(byte[] FileBytes, string FileName, string ContentType)> GetFileMinhChungAsync(long fileId)
    {
        var file = await _context.FileMinhChungs
            .Include(f => f.IdDuLieuHoatDongNavigation)
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (file == null)
            throw new KeyNotFoundException("Không tìm thấy file minh chứng.");

        await _currentUserAccess.AssertCanAccessOrganizationAsync(file.IdDuLieuHoatDongNavigation.IdToChuc);

        var fileName = Path.GetFileName(file.DuongDanFile);
        var baseDir = Directory.GetCurrentDirectory();

        string fullPath = Path.Combine(baseDir, "uploads", "proofs", fileName);
        if (!File.Exists(fullPath))
        {
            fullPath = Path.Combine(baseDir, "uploads", fileName);
            if (!File.Exists(fullPath))
            {
                var wwwroot = Path.Combine(baseDir, "wwwroot", "uploads", fileName);
                if (File.Exists(wwwroot)) fullPath = wwwroot;
                else throw new KeyNotFoundException("Tệp tin vật lý không tồn tại trên hệ thống máy chủ.");
            }
        }

        var bytes = await File.ReadAllBytesAsync(fullPath);
        var ext = Path.GetExtension(file.TenFile).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        return (bytes, file.TenFile, contentType);
    }

    public async Task<(byte[] FileBytes, string FileName, string ContentType)> GenerateImportTemplateAsync()
    {
        using var workbook = new XLWorkbook();

        // 1. Sheet Nhập liệu chính
        var wsData = workbook.Worksheets.Add("Nhap_Lieu_Hoat_Dong");

        wsData.Cell(1, 1).Value = "MÃ NGUỒN (*)";
        wsData.Cell(1, 2).Value = "SỐ LƯỢNG TIÊU THỤ (*)";
        wsData.Cell(1, 3).Value = "ĐƠN VỊ TÍNH (*)";
        wsData.Cell(1, 4).Value = "SỐ HÓA ĐƠN VAT / CHỨNG TỪ";
        wsData.Cell(1, 5).Value = "NHÀ CUNG CẤP / ĐƠN VỊ";
        wsData.Cell(1, 6).Value = "GHI CHÚ";

        var headerRange = wsData.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Dòng mẫu hướng dẫn
        var sampleRows = new[]
        {
            new { Ma = "DIEN_LUOI", Sl = 50000m, DonVi = "kWh", HD = "HD-EVN-00129", NCC = "Công ty Điện lực Tân Bình", Note = "Điện sản xuất xưởng A" },
            new { Ma = "DAU_DO", Sl = 1200m, DonVi = "lit", HD = "HD-PETRO-991", NCC = "Petrolimex Sài Gòn", Note = "Dầu chạy máy phát điện dự phòng" },
            new { Ma = "XANG_RON95", Sl = 350m, DonVi = "lit", HD = "HD-XANG-882", NCC = "Petrolimex", Note = "Xăng xe đội xe công tác" },
            new { Ma = "GAS_LPG", Sl = 200m, DonVi = "kg", HD = "HD-GAS-112", NCC = "Saigon Petro Gas", Note = "Khí gas bếp ăn ca nhân viên" },
            new { Ma = "NUOC_SACH", Sl = 450m, DonVi = "m3", HD = "HD-SAWACO-771", NCC = "Tổng Công ty Cấp nước Sài Gòn", Note = "Nước sạch sinh hoạt & văn phòng" }
        };

        int r = 2;
        foreach (var sample in sampleRows)
        {
            wsData.Cell(r, 1).Value = sample.Ma;
            wsData.Cell(r, 2).Value = sample.Sl;
            wsData.Cell(r, 3).Value = sample.DonVi;
            wsData.Cell(r, 4).Value = sample.HD;
            wsData.Cell(r, 5).Value = sample.NCC;
            wsData.Cell(r, 6).Value = sample.Note;
            r++;
        }

        wsData.Columns().AdjustToContents();

        // 2. Sheet Danh mục mã nguồn tra cứu
        var wsDanhMuc = workbook.Worksheets.Add("Danh_Muc_Ma_Nguon");
        wsDanhMuc.Cell(1, 1).Value = "MÃ NGUỒN TRA CỨU";
        wsDanhMuc.Cell(1, 2).Value = "TÊN NGUỒN PHÁT THẢI";
        wsDanhMuc.Cell(1, 3).Value = "PHẠM VI (SCOPE)";
        wsDanhMuc.Cell(1, 4).Value = "ĐƠN VỊ CHUẨN";
        wsDanhMuc.Cell(1, 5).Value = "PHÂN LOẠI";

        var dmHeaderRange = wsDanhMuc.Range(1, 1, 1, 5);
        dmHeaderRange.Style.Font.Bold = true;
        dmHeaderRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#059669");
        dmHeaderRange.Style.Font.FontColor = XLColor.White;

        var danhMuc = await _context.DanhMucNguonPhatThais.OrderBy(d => d.PhamVi).ThenBy(d => d.Id).ToListAsync();
        int dmRow = 2;
        foreach (var dm in danhMuc)
        {
            wsDanhMuc.Cell(dmRow, 1).Value = dm.MaNguon;
            wsDanhMuc.Cell(dmRow, 2).Value = dm.TenNguon;
            wsDanhMuc.Cell(dmRow, 3).Value = dm.PhamVi.ToUpper().Replace("_", " ");
            wsDanhMuc.Cell(dmRow, 4).Value = dm.DonViTinhChuan;
            wsDanhMuc.Cell(dmRow, 5).Value = dm.PhanLoai;
            dmRow++;
        }

        wsDanhMuc.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        var bytes = ms.ToArray();

        var fileName = "Mau_Nhap_Lieu_Phat_Thai_CEMIS.xlsx";
        return (bytes, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static DuLieuHoatDongDto MapToDto(DuLieuHoatDong d)
    {
        return new DuLieuHoatDongDto
        {
            Id = d.Id,
            IdToChuc = d.IdToChuc,
            TenToChuc = d.IdToChucNavigation?.TenToChuc ?? "",
            IdKyBaoCao = d.IdKyBaoCao,
            TenKyBaoCao = d.IdKyBaoCaoNavigation?.TenKy ?? "",
            IdNguonPhatThai = d.IdNguonPhatThai,
            MaNguon = d.IdNguonPhatThaiNavigation?.MaNguon ?? "",
            TenNguon = d.IdNguonPhatThaiNavigation?.TenNguon ?? "",
            PhamVi = d.IdNguonPhatThaiNavigation?.PhamVi ?? "",
            IdNguoiNhap = d.IdNguoiNhap,
            TenNguoiNhap = d.IdNguoiNhapNavigation?.HoTen ?? "",
            SoLuongNhap = d.SoLuongNhap,
            DonViNhap = d.DonViNhap,
            HeSoQuyDoi = d.HeSoQuyDoi,
            SoLuongQuyChuan = d.SoLuongQuyChuan,
            NgaySuDung = d.NgaySuDung,
            SoHoaDon = d.SoHoaDon,
            NgayHoaDon = d.NgayHoaDon,
            NhaCungCap = d.NhaCungCap,
            TrangThaiDuyet = d.TrangThaiDuyet,
            GhiChu = d.GhiChu,
            KetQuaPhatThaiTanCo2e = d.KetQuaTinhToan?.TongPhatThaiTanCo2e,
            CongThucApDung = d.KetQuaTinhToan?.CongThucApDung,
            HeSoPhatThai = d.KetQuaTinhToan?.IdHeSoPhatThaiNavigation?.TongHeSoCo2e,
            DonViHeSo = d.KetQuaTinhToan?.IdHeSoPhatThaiNavigation?.DonViHeSo,
            MinhChungs = d.FileMinhChungs?.Select(f => new FileMinhChungDto
            {
                Id = f.Id,
                IdDuLieuHoatDong = f.IdDuLieuHoatDong,
                TenFile = f.TenFile,
                DuongDanFile = f.DuongDanFile,
                LoaiFile = f.LoaiFile,
                DungLuongKb = f.DungLuongKb,
                GhiChu = f.GhiChu
            }).ToList() ?? new List<FileMinhChungDto>()
        };
    }
}
