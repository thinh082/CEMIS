using CEMIS.DTOs;

namespace CEMIS.Services.Interfaces;

public interface INguoiDungService
{
    Task<List<NguoiDungDto>> GetListAsync();
    Task<NguoiDungDto> GetByIdAsync(long id);
    Task<NguoiDungDto> CreateAsync(CreateNguoiDungDto dto);
    Task<NguoiDungDto> UpdateAsync(long id, UpdateNguoiDungDto dto);
    Task<bool> ToggleLockAsync(long id);
    Task<List<VaiTroDto>> GetVaiTroListAsync();
    Task<bool> UpdateSignatureAsync(long id, string? duongDanChuKyAnh);
    Task<string> UploadSignatureAsync(long id, Microsoft.AspNetCore.Http.IFormFile file);
}
