using CEMIS.DTOs;

namespace CEMIS.Services.Interfaces;

public interface IBaoCaoService
{
    Task<(byte[] FileBytes, string FileName, string ContentType)> ExportReportAsync(ExportReportRequestDto request, long nguoiXuatId);
    Task<List<BaoCaoLichSuDto>> GetReportHistoryAsync(long? toChucId = null);
}
