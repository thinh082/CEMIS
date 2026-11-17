using CEMIS.DTOs;

namespace CEMIS.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(long? toChucId = null, int? nam = null);
    Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(long? toChucId = null, int nam = 2026);
    Task<List<EmissionBySourceDto>> GetEmissionBySourceAsync(long? toChucId = null, int? nam = null);
    Task<List<FacilityComparisonDto>> GetFacilityComparisonAsync(int nam = 2026);
    Task<List<AlertDto>> GetAlertsAsync(long? toChucId = null, bool? daXuLy = null);
    Task<bool> ResolveAlertAsync(long alertId);
}
