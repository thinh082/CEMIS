using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Services.Implementations;

public class CalculationEngineService : ICalculationEngineService
{
    private readonly CemisDbContext _context;

    public CalculationEngineService(CemisDbContext context)
    {
        _context = context;
    }

    public async Task<KetQuaTinhToan> CalculateEmissionAsync(long duLieuHoatDongId)
    {
        var duLieu = await _context.DuLieuHoatDongs
            .Include(d => d.IdNguonPhatThaiNavigation)
            .Include(d => d.IdKyBaoCaoNavigation)
            .FirstOrDefaultAsync(d => d.Id == duLieuHoatDongId);

        if (duLieu == null)
            throw new KeyNotFoundException("Không tìm thấy bản ghi dữ liệu hoạt động.");

        var nam = duLieu.IdKyBaoCaoNavigation.Nam;

        // Tìm hệ số phát thải phù hợp (Ưu tiên năm báo cáo, nếu không có lấy năm gần nhất đang áp dụng)
        var heSo = await _context.HeSoPhatThais
            .Include(h => h.IdGwpNavigation)
            .Where(h => h.IdNguonPhatThai == duLieu.IdNguonPhatThai && (h.ApDung == true || h.ApDung == null))
            .OrderByDescending(h => h.NamApDung == nam)
            .ThenByDescending(h => h.NamApDung)
            .FirstOrDefaultAsync();

        if (heSo == null)
            throw new InvalidOperationException($"Chưa cấu hình Hệ số phát thải cho nguồn '{duLieu.IdNguonPhatThaiNavigation.TenNguon}'.");

        var gwpCo2 = heSo.IdGwpNavigation?.GwpCo2 ?? 1.0m;
        var gwpCh4 = heSo.IdGwpNavigation?.GwpCh4 ?? 28.0m;
        var gwpN2o = heSo.IdGwpNavigation?.GwpN2o ?? 265.0m;

        decimal phatThaiCo2Kg = 0;
        decimal phatThaiCh4Kg = 0;
        decimal phatThaiN2oKg = 0;
        decimal tongPhatThaiKgCo2e = 0;

        if (heSo.HeSoCo2 > 0 || heSo.HeSoCh4 > 0 || heSo.HeSoN2o > 0)
        {
            phatThaiCo2Kg = duLieu.SoLuongQuyChuan * (heSo.HeSoCo2 ?? 0);
            phatThaiCh4Kg = duLieu.SoLuongQuyChuan * (heSo.HeSoCh4 ?? 0);
            phatThaiN2oKg = duLieu.SoLuongQuyChuan * (heSo.HeSoN2o ?? 0);

            tongPhatThaiKgCo2e = (phatThaiCo2Kg * gwpCo2) + (phatThaiCh4Kg * gwpCh4) + (phatThaiN2oKg * gwpN2o);
        }
        else
        {
            tongPhatThaiKgCo2e = duLieu.SoLuongQuyChuan * heSo.TongHeSoCo2e;
            phatThaiCo2Kg = tongPhatThaiKgCo2e;
        }

        // Quy đổi ra Tấn CO2e (tCO2e)
        decimal tongTanCo2e = tongPhatThaiKgCo2e / 1000m;
        string congThuc = (duLieu.DonViNhap != null && !duLieu.DonViNhap.Equals(duLieu.IdNguonPhatThaiNavigation.DonViTinhChuan, StringComparison.OrdinalIgnoreCase) && duLieu.HeSoQuyDoi != 1.0m)
            ? $"{duLieu.SoLuongNhap} {duLieu.DonViNhap} (quy đổi {duLieu.SoLuongQuyChuan} {duLieu.IdNguonPhatThaiNavigation.DonViTinhChuan}) x {heSo.TongHeSoCo2e} {heSo.DonViHeSo} / 1000"
            : $"{duLieu.SoLuongQuyChuan} {duLieu.IdNguonPhatThaiNavigation.DonViTinhChuan} x {heSo.TongHeSoCo2e} {heSo.DonViHeSo} / 1000";

        var existingKetQua = await _context.KetQuaTinhToans
            .FirstOrDefaultAsync(k => k.IdDuLieuHoatDong == duLieu.Id);

        if (existingKetQua != null)
        {
            existingKetQua.IdHeSoPhatThai = heSo.Id;
            existingKetQua.PhamVi = duLieu.IdNguonPhatThaiNavigation.PhamVi;
            existingKetQua.SoLuongHoatDong = duLieu.SoLuongQuyChuan;
            existingKetQua.DonViTinh = duLieu.IdNguonPhatThaiNavigation.DonViTinhChuan;
            existingKetQua.PhatThaiCo2Kg = phatThaiCo2Kg;
            existingKetQua.PhatThaiCh4Kg = phatThaiCh4Kg;
            existingKetQua.PhatThaiN2oKg = phatThaiN2oKg;
            existingKetQua.TongPhatThaiKgCo2e = tongPhatThaiKgCo2e;
            existingKetQua.TongPhatThaiTanCo2e = tongTanCo2e;
            existingKetQua.CongThucApDung = congThuc;
            existingKetQua.NgayTinhToan = DateTime.UtcNow;
        }
        else
        {
            existingKetQua = new KetQuaTinhToan
            {
                IdDuLieuHoatDong = duLieu.Id,
                IdToChuc = duLieu.IdToChuc,
                IdKyBaoCao = duLieu.IdKyBaoCao,
                IdHeSoPhatThai = heSo.Id,
                PhamVi = duLieu.IdNguonPhatThaiNavigation.PhamVi,
                SoLuongHoatDong = duLieu.SoLuongQuyChuan,
                DonViTinh = duLieu.IdNguonPhatThaiNavigation.DonViTinhChuan,
                PhatThaiCo2Kg = phatThaiCo2Kg,
                PhatThaiCh4Kg = phatThaiCh4Kg,
                PhatThaiN2oKg = phatThaiN2oKg,
                TongPhatThaiKgCo2e = tongPhatThaiKgCo2e,
                TongPhatThaiTanCo2e = tongTanCo2e,
                CongThucApDung = congThuc,
                NgayTinhToan = DateTime.UtcNow
            };
            _context.KetQuaTinhToans.Add(existingKetQua);
        }

        await _context.SaveChangesAsync();

        // Kiểm tra cảnh báo vượt định mức baseline
        await CheckAndTriggerAlertsAsync(duLieu, tongTanCo2e);

        return existingKetQua;
    }

    private async Task CheckAndTriggerAlertsAsync(DuLieuHoatDong duLieu, decimal tongTanCo2e)
    {
        var nam = duLieu.IdKyBaoCaoNavigation.Nam;
        var dinhMuc = await _context.DinhMucCanhBaos
            .FirstOrDefaultAsync(d => d.IdToChuc == duLieu.IdToChuc && d.IdNguonPhatThai == duLieu.IdNguonPhatThai && d.NamApDung == nam);

        if (dinhMuc != null)
        {
            // Kiểm tra theo phát thải tháng (tCO2e)
            if (dinhMuc.NguongPhatThaiThangTco2e.HasValue && tongTanCo2e > dinhMuc.NguongPhatThaiThangTco2e.Value)
            {
                decimal phanTramVuot = Math.Round(((tongTanCo2e - dinhMuc.NguongPhatThaiThangTco2e.Value) / dinhMuc.NguongPhatThaiThangTco2e.Value) * 100, 2);
                var alert = new ThongBaoCanhBao
                {
                    IdToChuc = duLieu.IdToChuc,
                    IdKyBaoCao = duLieu.IdKyBaoCao,
                    IdDinhMuc = dinhMuc.Id,
                    TieuDe = $"Cảnh báo phát thải vượt định mức: {duLieu.IdNguonPhatThaiNavigation.TenNguon}",
                    NoiDung = $"Phát thải thực tế đạt {tongTanCo2e:F3} tCO2e, vượt {phanTramVuot}% so với ngưỡng định mức ({dinhMuc.NguongPhatThaiThangTco2e.Value} tCO2e).",
                    GiaTriThucTe = tongTanCo2e,
                    GiaTriDinhMuc = dinhMuc.NguongPhatThaiThangTco2e.Value,
                    PhanTramVuot = phanTramVuot,
                    MucDo = phanTramVuot > 30 ? "nghiem_trong" : "canh_bao",
                    NgayPhatHien = DateTime.UtcNow,
                    DaXuLy = false
                };
                _context.ThongBaoCanhBaos.Add(alert);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task RecalculatePeriodAsync(long kyBaoCaoId)
    {
        var list = await _context.DuLieuHoatDongs
            .Where(d => d.IdKyBaoCao == kyBaoCaoId && d.TrangThaiDuyet == "da_duyet")
            .Select(d => d.Id)
            .ToListAsync();

        foreach (var id in list)
        {
            await CalculateEmissionAsync(id);
        }
    }
}
