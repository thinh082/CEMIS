using CEMIS.DTOs;
using Microsoft.AspNetCore.Http;

namespace CEMIS.Services.Interfaces;

public interface IDuLieuHoatDongService
{
    Task<List<KyBaoCaoDto>> GetKyBaoCaoListAsync(long? toChucId = null, int? nam = null);
    Task<KyBaoCaoDto> CreateKyBaoCaoAsync(CreateKyBaoCaoDto dto);
    Task<bool> ToggleKhoaSoAsync(long kyBaoCaoId, string trangThai);

    Task<List<DuLieuHoatDongDto>> GetDuLieuListAsync(long? toChucId = null, long? kyBaoCaoId = null, string? phamVi = null, string? trangThaiDuyet = null);
    Task<DuLieuHoatDongDto> GetDuLieuByIdAsync(long id);
    Task<DuLieuHoatDongDto> CreateDuLieuAsync(CreateDuLieuHoatDongDto dto, long nguoiNhapId);
    Task<DuLieuHoatDongDto> UpdateDuLieuAsync(long id, UpdateDuLieuHoatDongDto dto);
    Task<bool> DeleteDuLieuAsync(long id);

    Task<bool> PheDuyetDuLieuAsync(long id, PheDuyetRequestDto dto, long nguoiThucHienId);
    Task<FileMinhChungDto> UploadMinhChungAsync(long duLieuId, IFormFile file, string? ghiChu);
    Task<int> BatchImportExcelAsync(long toChucId, long kyBaoCaoId, long nguoiNhapId, IFormFile excelFile);
    Task<(byte[] FileBytes, string FileName, string ContentType)> GenerateImportTemplateAsync();
    Task<(byte[] FileBytes, string FileName, string ContentType)> GetFileMinhChungAsync(long fileId);
}
