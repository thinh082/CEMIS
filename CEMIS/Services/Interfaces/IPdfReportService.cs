namespace CEMIS.Services.Interfaces;

public interface IPdfReportService
{
    Task<(byte[] FileBytes, string FileName, string ContentType)> GenerateConfirmedKiemKePdfAsync(
        long toChucId, 
        long kyBaoCaoId, 
        long nguoiXacNhanId, 
        string? ghiChuXacNhan = null);

    Task<(byte[] FileBytes, string FileName, string ContentType, string HashSha256)> GenerateAndSignKiemKePdfAsync(
        long toChucId, 
        long kyBaoCaoId, 
        long nguoiKyId, 
        string? lyDoKy = null, 
        string? pinCode = null);
}
