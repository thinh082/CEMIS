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

public class ToChucService : IToChucService
{
    private readonly CemisDbContext _context;
    private readonly ICurrentUserAccess _currentUserAccess;

    public ToChucService(CemisDbContext context, ICurrentUserAccess currentUserAccess)
    {
        _context = context;
        _currentUserAccess = currentUserAccess;
    }

    public async Task<List<ToChucDto>> GetAllFlatAsync()
    {
        var isSuperAdmin = await _currentUserAccess.IsSuperAdminAsync();
        var query = _context.ToChucs
            .Include(t => t.IdToChucChaNavigation)
            .OrderBy(t => t.Id)
            .AsQueryable();

        if (!isSuperAdmin)
        {
            var userOrgId = await _currentUserAccess.GetUserOrganizationIdAsync();
            if (!userOrgId.HasValue)
            {
                throw new AppForbiddenException("Tài khoản chưa được phân công cơ sở hoạt động. Vui lòng liên hệ quản trị viên.");
            }
            query = query.Where(t => t.Id == userOrgId.Value);
        }

        var list = await query.ToListAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<ToChucDto>> GetTreeAsync()
    {
        var isSuperAdmin = await _currentUserAccess.IsSuperAdminAsync();
        if (!isSuperAdmin)
        {
            var flat = await GetAllFlatAsync();
            foreach (var item in flat)
            {
                item.ChiNhanhCon = new List<ToChucDto>();
            }
            return flat;
        }

        var all = await _context.ToChucs
            .Include(t => t.IdToChucChaNavigation)
            .OrderBy(t => t.Id)
            .ToListAsync();
        var dtos = all.Select(MapToDto).ToList();

        var rootNodes = dtos.Where(t => t.IdToChucCha == null).ToList();
        foreach (var root in rootNodes)
        {
            BuildChildren(root, dtos);
        }

        return rootNodes;
    }

    private void BuildChildren(ToChucDto parent, List<ToChucDto> all)
    {
        parent.ChiNhanhCon = all.Where(t => t.IdToChucCha == parent.Id).ToList();
        foreach (var child in parent.ChiNhanhCon)
        {
            BuildChildren(child, all);
        }
    }

    public async Task<ToChucDto> GetByIdAsync(long id)
    {
        await _currentUserAccess.AssertCanAccessOrganizationAsync(id);

        var toChuc = await _context.ToChucs
            .Include(t => t.IdToChucChaNavigation)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (toChuc == null)
            throw new KeyNotFoundException("Không tìm thấy cơ sở/tổ chức.");

        return MapToDto(toChuc);
    }

    public async Task<ToChucDto> CreateAsync(CreateToChucDto dto)
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var exists = await _context.ToChucs.AnyAsync(t => t.MaToChuc == dto.MaToChuc);
        if (exists)
            throw new InvalidOperationException($"Mã tổ chức '{dto.MaToChuc}' đã tồn tại.");

        var entity = new ToChuc
        {
            MaToChuc = dto.MaToChuc,
            TenToChuc = dto.TenToChuc,
            LoaiToChuc = dto.LoaiToChuc,
            IdToChucCha = dto.IdToChucCha,
            DiaChi = dto.DiaChi,
            DienTichSanM2 = dto.DienTichSanM2,
            SoNhanVien = dto.SoNhanVien,
            TrangThai = "hoat_dong"
        };

        _context.ToChucs.Add(entity);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task<ToChucDto> UpdateAsync(long id, UpdateToChucDto dto)
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var entity = await _context.ToChucs.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy cơ sở/tổ chức.");

        entity.TenToChuc = dto.TenToChuc;
        entity.LoaiToChuc = dto.LoaiToChuc;
        entity.IdToChucCha = dto.IdToChucCha;
        entity.DiaChi = dto.DiaChi;
        entity.DienTichSanM2 = dto.DienTichSanM2;
        entity.SoNhanVien = dto.SoNhanVien;
        if (!string.IsNullOrEmpty(dto.TrangThai))
            entity.TrangThai = dto.TrangThai;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var entity = await _context.ToChucs.FindAsync(id);
        if (entity == null) return false;

        if (await _context.ToChucs.AnyAsync(t => t.IdToChucCha == id))
            throw new InvalidOperationException("Không thể xóa cơ sở đang có chi nhánh trực thuộc.");
        if (await _context.NguoiDungs.AnyAsync(u => u.IdToChuc == id) ||
            await _context.KyBaoCaos.AnyAsync(k => k.IdToChuc == id) ||
            await _context.DuLieuHoatDongs.AnyAsync(d => d.IdToChuc == id) ||
            await _context.KetQuaTinhToans.AnyAsync(k => k.IdToChuc == id) ||
            await _context.BaoCaoXuats.AnyAsync(b => b.IdToChuc == id) ||
            await _context.DuLieuSanXuatKinhDoanhs.AnyAsync(d => d.IdToChuc == id) ||
            await _context.DinhMucCanhBaos.AnyAsync(d => d.IdToChuc == id) ||
            await _context.ThongBaoCanhBaos.AnyAsync(a => a.IdToChuc == id))
            throw new InvalidOperationException("Không thể xóa đơn vị đang có tài khoản hoặc dữ liệu liên quan. Bạn có thể sửa trạng thái thành ngừng hoạt động.");

        _context.ToChucs.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ToChucDto MapToDto(ToChuc t)
    {
        return new ToChucDto
        {
            Id = t.Id,
            MaToChuc = t.MaToChuc,
            TenToChuc = t.TenToChuc,
            LoaiToChuc = t.LoaiToChuc,
            IdToChucCha = t.IdToChucCha,
            TenToChucCha = t.IdToChucChaNavigation?.TenToChuc,
            DiaChi = t.DiaChi,
            DienTichSanM2 = t.DienTichSanM2,
            SoNhanVien = t.SoNhanVien,
            TrangThai = t.TrangThai
        };
    }
}
