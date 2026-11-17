using CEMIS.DTOs;

namespace CEMIS.Services.Interfaces;

public interface IToChucService
{
    Task<List<ToChucDto>> GetAllFlatAsync();
    Task<List<ToChucDto>> GetTreeAsync();
    Task<ToChucDto> GetByIdAsync(long id);
    Task<ToChucDto> CreateAsync(CreateToChucDto dto);
    Task<ToChucDto> UpdateAsync(long id, UpdateToChucDto dto);
    Task<bool> DeleteAsync(long id);
}
