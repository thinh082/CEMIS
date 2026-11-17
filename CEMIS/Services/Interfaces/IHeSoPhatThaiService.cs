using CEMIS.DTOs;

namespace CEMIS.Services.Interfaces;

public interface IHeSoPhatThaiService
{
    Task<List<DanhMucNguonPhatThaiDto>> GetDanhMucNguonAsync(string? phamVi = null);
    Task<List<HeSoPhatThaiDto>> GetHeSoListAsync(int? nam = null, string? phamVi = null);
    Task<List<HeSoGwpDto>> GetGwpListAsync();
    Task<HeSoPhatThaiDto> CreateHeSoAsync(CreateHeSoPhatThaiDto dto);
    Task<bool> ToggleApDungAsync(long id, bool apDung);
}
