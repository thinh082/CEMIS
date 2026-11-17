using CEMIS.Models;

namespace CEMIS.Services.Interfaces;

public interface ICalculationEngineService
{
    Task<KetQuaTinhToan> CalculateEmissionAsync(long duLieuHoatDongId);
    Task RecalculatePeriodAsync(long kyBaoCaoId);
}
