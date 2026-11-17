using CEMIS.DTOs;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Services.Implementations;

public class HeSoPhatThaiService : IHeSoPhatThaiService
{
    private readonly CemisDbContext _context;

    public HeSoPhatThaiService(CemisDbContext context)
    {
        _context = context;
    }

    public async Task<List<DanhMucNguonPhatThaiDto>> GetDanhMucNguonAsync(string? phamVi = null)
    {
        var query = _context.DanhMucNguonPhatThais.AsQueryable();

        if (!string.IsNullOrEmpty(phamVi))
        {
            query = query.Where(x => x.PhamVi == phamVi);
        }

        var list = await query.OrderBy(x => x.PhamVi).ThenBy(x => x.Id).ToListAsync();

        return list.Select(x => new DanhMucNguonPhatThaiDto
        {
            Id = x.Id,
            MaNguon = x.MaNguon,
            TenNguon = x.TenNguon,
            PhamVi = x.PhamVi,
            PhanLoai = x.PhanLoai,
            DonViTinhChuan = x.DonViTinhChuan,
            MoTa = x.MoTa
        }).ToList();
    }

    public async Task<List<HeSoPhatThaiDto>> GetHeSoListAsync(int? nam = null, string? phamVi = null)
    {
        var query = _context.HeSoPhatThais
            .Include(h => h.IdNguonPhatThaiNavigation)
            .Include(h => h.IdGwpNavigation)
            .AsQueryable();

        if (nam.HasValue)
        {
            query = query.Where(h => h.NamApDung == nam.Value);
        }

        if (!string.IsNullOrEmpty(phamVi))
        {
            query = query.Where(h => h.IdNguonPhatThaiNavigation.PhamVi == phamVi);
        }

        var list = await query.OrderByDescending(h => h.NamApDung).ThenBy(h => h.IdNguonPhatThai).ToListAsync();

        return list.Select(h => new HeSoPhatThaiDto
        {
            Id = h.Id,
            IdNguonPhatThai = h.IdNguonPhatThai,
            MaNguon = h.IdNguonPhatThaiNavigation.MaNguon,
            TenNguon = h.IdNguonPhatThaiNavigation.TenNguon,
            PhamVi = h.IdNguonPhatThaiNavigation.PhamVi,
            IdGwp = h.IdGwp,
            PhienBanGwp = h.IdGwpNavigation?.PhienBanIpcc,
            TenHeSo = h.TenHeSo,
            NguonGoc = h.NguonGoc,
            NamApDung = h.NamApDung,
            HeSoCo2 = h.HeSoCo2,
            HeSoCh4 = h.HeSoCh4,
            HeSoN2o = h.HeSoN2o,
            TongHeSoCo2e = h.TongHeSoCo2e,
            DonViHeSo = h.DonViHeSo,
            ApDung = h.ApDung,
            GhiChu = h.GhiChu
        }).ToList();
    }

    public async Task<List<HeSoGwpDto>> GetGwpListAsync()
    {
        var list = await _context.HeSoGwps.OrderBy(g => g.PhienBanIpcc).ToListAsync();
        return list.Select(g => new HeSoGwpDto
        {
            Id = g.Id,
            PhienBanIpcc = g.PhienBanIpcc,
            GwpCo2 = g.GwpCo2,
            GwpCh4 = g.GwpCh4,
            GwpN2o = g.GwpN2o,
            GhiChu = g.GhiChu
        }).ToList();
    }

    public async Task<HeSoPhatThaiDto> CreateHeSoAsync(CreateHeSoPhatThaiDto dto)
    {
        var entity = new HeSoPhatThai
        {
            IdNguonPhatThai = dto.IdNguonPhatThai,
            IdGwp = dto.IdGwp,
            TenHeSo = dto.TenHeSo,
            NguonGoc = dto.NguonGoc,
            NamApDung = dto.NamApDung,
            HeSoCo2 = dto.HeSoCo2 ?? 0,
            HeSoCh4 = dto.HeSoCh4 ?? 0,
            HeSoN2o = dto.HeSoN2o ?? 0,
            TongHeSoCo2e = dto.TongHeSoCo2e,
            DonViHeSo = dto.DonViHeSo,
            ApDung = true,
            GhiChu = dto.GhiChu
        };

        _context.HeSoPhatThais.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.HeSoPhatThais
            .Include(h => h.IdNguonPhatThaiNavigation)
            .Include(h => h.IdGwpNavigation)
            .FirstAsync(h => h.Id == entity.Id);

        return new HeSoPhatThaiDto
        {
            Id = created.Id,
            IdNguonPhatThai = created.IdNguonPhatThai,
            MaNguon = created.IdNguonPhatThaiNavigation.MaNguon,
            TenNguon = created.IdNguonPhatThaiNavigation.TenNguon,
            PhamVi = created.IdNguonPhatThaiNavigation.PhamVi,
            IdGwp = created.IdGwp,
            PhienBanGwp = created.IdGwpNavigation?.PhienBanIpcc,
            TenHeSo = created.TenHeSo,
            NguonGoc = created.NguonGoc,
            NamApDung = created.NamApDung,
            HeSoCo2 = created.HeSoCo2,
            HeSoCh4 = created.HeSoCh4,
            HeSoN2o = created.HeSoN2o,
            TongHeSoCo2e = created.TongHeSoCo2e,
            DonViHeSo = created.DonViHeSo,
            ApDung = created.ApDung,
            GhiChu = created.GhiChu
        };
    }

    public async Task<bool> ToggleApDungAsync(long id, bool apDung)
    {
        var entity = await _context.HeSoPhatThais.FindAsync(id);
        if (entity == null) return false;

        entity.ApDung = apDung;
        await _context.SaveChangesAsync();
        return true;
    }
}
