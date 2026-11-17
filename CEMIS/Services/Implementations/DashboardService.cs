using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly CemisDbContext _context;
    private readonly ICurrentUserAccess _currentUserAccess;

    public DashboardService(CemisDbContext context, ICurrentUserAccess currentUserAccess)
    {
        _context = context;
        _currentUserAccess = currentUserAccess;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(long? toChucId = null, int? nam = null)
    {
        var year = nam ?? DateTime.UtcNow.Year;
        var effectiveToChucId = await _currentUserAccess.GetEffectiveOrganizationIdAsync(toChucId);

        var query = _context.KetQuaTinhToans
            .Include(k => k.IdKyBaoCaoNavigation)
            .Include(k => k.IdDuLieuHoatDongNavigation)
                .ThenInclude(d => d.IdNguonPhatThaiNavigation)
            .Where(k => k.IdKyBaoCaoNavigation.Nam == year);

        if (effectiveToChucId.HasValue)
        {
            query = query.Where(k => k.IdToChuc == effectiveToChucId.Value);
        }

        var results = await query.ToListAsync();

        var scope1 = results.Where(r => r.PhamVi == "scope_1").Sum(r => r.TongPhatThaiTanCo2e);
        var scope2 = results.Where(r => r.PhamVi == "scope_2").Sum(r => r.TongPhatThaiTanCo2e);
        var scope3 = results.Where(r => r.PhamVi == "scope_3").Sum(r => r.TongPhatThaiTanCo2e);
        var total = scope1 + scope2 + scope3;

        // Tổng điện kWh
        var totalElectricityKWh = results
            .Where(r => r.IdDuLieuHoatDongNavigation.IdNguonPhatThaiNavigation.MaNguon == "DIEN_LUOI")
            .Sum(r => r.SoLuongHoatDong);

        // Tổng nhiên liệu Lít
        var totalFuelLit = results
            .Where(r => r.IdDuLieuHoatDongNavigation.IdNguonPhatThaiNavigation.DonViTinhChuan == "lit")
            .Sum(r => r.SoLuongHoatDong);

        var totalFacilities = effectiveToChucId.HasValue ? 1 : await _context.ToChucs.CountAsync(t => t.TrangThai == "hoat_dong");

        var alertQuery = _context.ThongBaoCanhBaos.Where(a => a.DaXuLy == false || a.DaXuLy == null);
        if (effectiveToChucId.HasValue)
            alertQuery = alertQuery.Where(a => a.IdToChuc == effectiveToChucId.Value);
        var activeAlerts = await alertQuery.CountAsync();

        // Tính toán cường độ phát thải (Emission Intensity)
        var businessDataQuery = _context.DuLieuSanXuatKinhDoanhs
            .Include(b => b.IdKyBaoCaoNavigation)
            .Where(b => b.IdKyBaoCaoNavigation.Nam == year);

        if (effectiveToChucId.HasValue)
            businessDataQuery = businessDataQuery.Where(b => b.IdToChuc == effectiveToChucId.Value);

        var businessData = await businessDataQuery.ToListAsync();
        var totalProductOutput = businessData.Sum(b => b.SanLuongSanPham ?? 0);
        var totalRevenueMillion = businessData.Sum(b => b.DoanhThuTrieuDong ?? 0);

        var toChucQuery = _context.ToChucs.AsQueryable();
        if (effectiveToChucId.HasValue)
            toChucQuery = toChucQuery.Where(t => t.Id == effectiveToChucId.Value);
        var totalFloorArea = await toChucQuery.SumAsync(t => t.DienTichSanM2 ?? 0);

        decimal intensityProduct = totalProductOutput > 0 ? Math.Round(total / totalProductOutput, 4) : 0;
        decimal intensityRevenue = totalRevenueMillion > 0 ? Math.Round(total / (totalRevenueMillion / 1000m), 4) : 0; // tCO2e / tỷ đồng
        decimal intensityFloor = totalFloorArea > 0 ? Math.Round(total / totalFloorArea, 4) : 0; // tCO2e / m2 sàn

        return new DashboardSummaryDto
        {
            TongPhatThaiTanCo2e = total,
            Scope1TanCo2e = scope1,
            Scope2TanCo2e = scope2,
            Scope3TanCo2e = scope3,
            TongDienTieuThuKWh = totalElectricityKWh,
            TongNhienLieuLit = totalFuelLit,
            TongSoCoSo = totalFacilities,
            SoLuongCanhBaoChuaXuLy = activeAlerts,
            CuongDoPhatThaiTheoSanPham = intensityProduct,
            CuongDoPhatThaiTheoDoanhThu = intensityRevenue,
            CuongDoPhatThaiTheoDienTich = intensityFloor
        };
    }

    public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(long? toChucId = null, int nam = 2026)
    {
        var effectiveToChucId = await _currentUserAccess.GetEffectiveOrganizationIdAsync(toChucId);

        var query = _context.KetQuaTinhToans
            .Include(k => k.IdKyBaoCaoNavigation)
            .Where(k => k.IdKyBaoCaoNavigation.Nam == nam);

        if (effectiveToChucId.HasValue)
            query = query.Where(k => k.IdToChuc == effectiveToChucId.Value);

        var list = await query.ToListAsync();

        var trends = new List<MonthlyTrendDto>();
        for (int m = 1; m <= 12; m++)
        {
            var monthData = list.Where(k => k.IdKyBaoCaoNavigation.Thang == m).ToList();
            trends.Add(new MonthlyTrendDto
            {
                Thang = $"T{m}",
                Scope1 = monthData.Where(x => x.PhamVi == "scope_1").Sum(x => x.TongPhatThaiTanCo2e),
                Scope2 = monthData.Where(x => x.PhamVi == "scope_2").Sum(x => x.TongPhatThaiTanCo2e),
                Scope3 = monthData.Where(x => x.PhamVi == "scope_3").Sum(x => x.TongPhatThaiTanCo2e)
            });
        }

        return trends;
    }

    public async Task<List<EmissionBySourceDto>> GetEmissionBySourceAsync(long? toChucId = null, int? nam = null)
    {
        var year = nam ?? DateTime.UtcNow.Year;
        var effectiveToChucId = await _currentUserAccess.GetEffectiveOrganizationIdAsync(toChucId);

        var query = _context.KetQuaTinhToans
            .Include(k => k.IdKyBaoCaoNavigation)
            .Include(k => k.IdDuLieuHoatDongNavigation)
                .ThenInclude(d => d.IdNguonPhatThaiNavigation)
            .Where(k => k.IdKyBaoCaoNavigation.Nam == year);

        if (effectiveToChucId.HasValue)
            query = query.Where(k => k.IdToChuc == effectiveToChucId.Value);

        var list = await query.ToListAsync();
        var total = list.Sum(k => k.TongPhatThaiTanCo2e);

        var grouped = list
            .GroupBy(k => new
            {
                k.IdDuLieuHoatDongNavigation.IdNguonPhatThaiNavigation.TenNguon,
                k.PhamVi
            })
            .Select(g =>
            {
                var sum = g.Sum(x => x.TongPhatThaiTanCo2e);
                return new EmissionBySourceDto
                {
                    TenNguon = g.Key.TenNguon,
                    PhamVi = g.Key.PhamVi,
                    TongTanCo2e = sum,
                    PhanTram = total > 0 ? Math.Round(sum / total * 100, 2) : 0
                };
            })
            .OrderByDescending(x => x.TongTanCo2e)
            .ToList();

        return grouped;
    }

    public async Task<List<FacilityComparisonDto>> GetFacilityComparisonAsync(int nam = 2026)
    {
        var isSuperAdmin = await _currentUserAccess.IsSuperAdminAsync();
        var facilitiesQuery = _context.ToChucs
            .Where(t => t.LoaiToChuc == "nha_may" || t.LoaiToChuc == "chi_nhanh");

        if (!isSuperAdmin)
        {
            var userOrgId = await _currentUserAccess.GetUserOrganizationIdAsync();
            if (!userOrgId.HasValue)
            {
                throw new AppForbiddenException("Tài khoản chưa được phân công cơ sở hoạt động.");
            }
            facilitiesQuery = facilitiesQuery.Where(t => t.Id == userOrgId.Value);
        }

        var facilities = await facilitiesQuery.ToListAsync();

        var calculations = await _context.KetQuaTinhToans
            .Include(k => k.IdKyBaoCaoNavigation)
            .Where(k => k.IdKyBaoCaoNavigation.Nam == nam)
            .ToListAsync();

        var result = new List<FacilityComparisonDto>();
        foreach (var fac in facilities)
        {
            var facCalcs = calculations.Where(c => c.IdToChuc == fac.Id).ToList();
            var s1 = facCalcs.Where(x => x.PhamVi == "scope_1").Sum(x => x.TongPhatThaiTanCo2e);
            var s2 = facCalcs.Where(x => x.PhamVi == "scope_2").Sum(x => x.TongPhatThaiTanCo2e);
            var s3 = facCalcs.Where(x => x.PhamVi == "scope_3").Sum(x => x.TongPhatThaiTanCo2e);
            var total = s1 + s2 + s3;
            var area = fac.DienTichSanM2 ?? 1;

            result.Add(new FacilityComparisonDto
            {
                IdToChuc = fac.Id,
                TenToChuc = fac.TenToChuc,
                Scope1 = s1,
                Scope2 = s2,
                Scope3 = s3,
                CuongDoM2 = area > 0 ? Math.Round(total / area, 4) : 0
            });
        }

        return result;
    }

    public async Task<List<AlertDto>> GetAlertsAsync(long? toChucId = null, bool? daXuLy = null)
    {
        var effectiveToChucId = await _currentUserAccess.GetEffectiveOrganizationIdAsync(toChucId);

        var query = _context.ThongBaoCanhBaos
            .Include(a => a.IdToChucNavigation)
            .AsQueryable();

        if (effectiveToChucId.HasValue)
            query = query.Where(a => a.IdToChuc == effectiveToChucId.Value);

        if (daXuLy.HasValue)
            query = query.Where(a => a.DaXuLy == daXuLy.Value);

        var list = await query.OrderByDescending(a => a.NgayPhatHien).ToListAsync();

        return list.Select(a => new AlertDto
        {
            Id = a.Id,
            TenToChuc = a.IdToChucNavigation?.TenToChuc ?? "",
            TieuDe = a.TieuDe,
            NoiDung = a.NoiDung,
            GiaTriThucTe = a.GiaTriThucTe,
            GiaTriDinhMuc = a.GiaTriDinhMuc,
            PhanTramVuot = a.PhanTramVuot,
            MucDo = a.MucDo,
            NgayPhatHien = a.NgayPhatHien,
            DaXuLy = a.DaXuLy
        }).ToList();
    }

    public async Task<bool> ResolveAlertAsync(long alertId)
    {
        var alert = await _context.ThongBaoCanhBaos.FindAsync(alertId);
        if (alert == null) return false;

        await _currentUserAccess.AssertCanModifyDataAsync(alert.IdToChuc);

        alert.DaXuLy = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
