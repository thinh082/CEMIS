using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Services.Implementations;

public class NguoiDungService : INguoiDungService
{
    private readonly CemisDbContext _context;
    private readonly ICurrentUserAccess _currentUserAccess;

    public NguoiDungService(CemisDbContext context, ICurrentUserAccess currentUserAccess)
    {
        _context = context;
        _currentUserAccess = currentUserAccess;
    }

    public async Task<List<NguoiDungDto>> GetListAsync()
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var users = await _context.NguoiDungs
            .Include(u => u.IdVaiTroNavigation)
            .Include(u => u.IdToChucNavigation)
            .OrderBy(u => u.Id)
            .ToListAsync();

        return users.Select(u => new NguoiDungDto
        {
            Id = u.Id,
            TenDangNhap = u.TenDangNhap,
            HoTen = u.HoTen,
            Email = u.Email,
            SoDienThoai = u.SoDienThoai,
            IdVaiTro = u.IdVaiTro,
            MaVaiTro = u.IdVaiTroNavigation?.MaVaiTro ?? "",
            TenVaiTro = u.IdVaiTroNavigation?.TenVaiTro ?? "",
            IdToChuc = u.IdToChuc,
            TenToChuc = u.IdToChucNavigation?.TenToChuc,
            TrangThai = u.TrangThai ?? "hoat_dong",
            DuongDanChuKyAnh = u.DuongDanChuKyAnh
        }).ToList();
    }

    public async Task<NguoiDungDto> GetByIdAsync(long id)
    {
        var isSuperAdmin = await _currentUserAccess.IsSuperAdminAsync();
        var currentUserId = await _currentUserAccess.GetUserIdAsync();
        if (!isSuperAdmin && currentUserId != id)
        {
            throw new AppForbiddenException("Bạn không có quyền xem thông tin tài khoản người dùng khác.");
        }

        var u = await _context.NguoiDungs
            .Include(u => u.IdVaiTroNavigation)
            .Include(u => u.IdToChucNavigation)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (u == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng.");

        return new NguoiDungDto
        {
            Id = u.Id,
            TenDangNhap = u.TenDangNhap,
            HoTen = u.HoTen,
            Email = u.Email,
            SoDienThoai = u.SoDienThoai,
            IdVaiTro = u.IdVaiTro,
            MaVaiTro = u.IdVaiTroNavigation?.MaVaiTro ?? "",
            TenVaiTro = u.IdVaiTroNavigation?.TenVaiTro ?? "",
            IdToChuc = u.IdToChuc,
            TenToChuc = u.IdToChucNavigation?.TenToChuc,
            TrangThai = u.TrangThai ?? "hoat_dong",
            DuongDanChuKyAnh = u.DuongDanChuKyAnh
        };
    }

    public async Task<NguoiDungDto> CreateAsync(CreateNguoiDungDto dto)
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var username = dto.TenDangNhap.Trim().ToLower();
        var exists = await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == username || u.Email == dto.Email.Trim());
        if (exists)
            throw new InvalidOperationException("Tên đăng nhập hoặc Email đã tồn tại trong hệ thống.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau);

        var entity = new NguoiDung
        {
            TenDangNhap = username,
            MatKhauHash = passwordHash,
            HoTen = dto.HoTen.Trim(),
            Email = dto.Email.Trim().ToLower(),
            SoDienThoai = dto.SoDienThoai?.Trim(),
            IdVaiTro = dto.IdVaiTro,
            IdToChuc = dto.IdToChuc,
            TrangThai = "hoat_dong",
            DuongDanChuKyAnh = dto.DuongDanChuKyAnh?.Trim()
        };

        _context.NguoiDungs.Add(entity);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task<NguoiDungDto> UpdateAsync(long id, UpdateNguoiDungDto dto)
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var entity = await _context.NguoiDungs.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (dto.TenDangNhap != null)
        {
            var username = dto.TenDangNhap.Trim().ToLowerInvariant();
            if (username.Length == 0 || username.Length > 100)
                throw new ArgumentException("Tên đăng nhập phải có từ 1 đến 100 ký tự.");

            var duplicate = await _context.NguoiDungs
                .AnyAsync(u => u.Id != id && u.TenDangNhap.ToLower() == username);
            if (duplicate)
                throw new InvalidOperationException("Tên đăng nhập đã được sử dụng bởi tài khoản khác.");

            entity.TenDangNhap = username;
        }

        entity.HoTen = dto.HoTen.Trim();
        entity.Email = dto.Email.Trim().ToLower();
        entity.SoDienThoai = dto.SoDienThoai?.Trim();
        entity.IdVaiTro = dto.IdVaiTro;
        entity.IdToChuc = dto.IdToChuc;
        entity.TrangThai = dto.TrangThai;
        entity.DuongDanChuKyAnh = dto.DuongDanChuKyAnh?.Trim();

        if (!string.IsNullOrWhiteSpace(dto.MatKhauMoi))
        {
            entity.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhauMoi);
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task<bool> ToggleLockAsync(long id)
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var entity = await _context.NguoiDungs.FindAsync(id);
        if (entity == null) return false;

        entity.TrangThai = entity.TrangThai == "khoa" ? "hoat_dong" : "khoa";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<VaiTroDto>> GetVaiTroListAsync()
    {
        await _currentUserAccess.AssertCanManageUsersAsync();

        var list = await _context.VaiTros.ToListAsync();
        return list.Select(v => new VaiTroDto
        {
            Id = v.Id,
            MaVaiTro = v.MaVaiTro,
            TenVaiTro = v.TenVaiTro,
            MoTa = v.MoTa
        }).ToList();
    }

    public async Task<bool> UpdateSignatureAsync(long id, string? duongDanChuKyAnh)
    {
        var isSuperAdmin = await _currentUserAccess.IsSuperAdminAsync();
        var currentUserId = await _currentUserAccess.GetUserIdAsync();
        if (!isSuperAdmin && currentUserId != id)
        {
            throw new AppForbiddenException("Bạn chỉ có quyền cập nhật ảnh chữ ký của chính mình.");
        }

        var user = await _context.NguoiDungs.FindAsync(id);
        if (user == null) return false;

        user.DuongDanChuKyAnh = string.IsNullOrWhiteSpace(duongDanChuKyAnh) ? null : duongDanChuKyAnh.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> UploadSignatureAsync(long id, Microsoft.AspNetCore.Http.IFormFile file)
    {
        var isSuperAdmin = await _currentUserAccess.IsSuperAdminAsync();
        var currentUserId = await _currentUserAccess.GetUserIdAsync();
        if (!isSuperAdmin && currentUserId != id)
        {
            throw new AppForbiddenException("Bạn chỉ có quyền tải lên ảnh chữ ký của chính mình.");
        }

        var user = await _context.NguoiDungs.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (file == null || file.Length == 0)
            throw new ArgumentException("Tệp tải lên không hợp lệ.");

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Dung lượng ảnh chữ ký tối đa là 5MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".webp")
            throw new InvalidOperationException("Chỉ chấp nhận tệp ảnh định dạng .png, .jpg hoặc .webp.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "signatures");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var safeFileName = $"signature_user_{id}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsFolder, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/signatures/{safeFileName}";
        user.DuongDanChuKyAnh = relativePath;
        await _context.SaveChangesAsync();

        return relativePath;
    }
}
