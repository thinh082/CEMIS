using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEMIS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary([FromQuery] long? toChucId, [FromQuery] int? nam)
    {
        var result = await _dashboardService.GetSummaryAsync(toChucId, nam);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(result));
    }

    [HttpGet("monthly-trend")]
    public async Task<ActionResult<ApiResponse<List<MonthlyTrendDto>>>> GetMonthlyTrend([FromQuery] long? toChucId, [FromQuery] int? nam)
    {
        var result = await _dashboardService.GetMonthlyTrendAsync(toChucId, nam ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<List<MonthlyTrendDto>>.Ok(result));
    }

    [HttpGet("emission-by-source")]
    public async Task<ActionResult<ApiResponse<List<EmissionBySourceDto>>>> GetEmissionBySource([FromQuery] long? toChucId, [FromQuery] int? nam)
    {
        var result = await _dashboardService.GetEmissionBySourceAsync(toChucId, nam);
        return Ok(ApiResponse<List<EmissionBySourceDto>>.Ok(result));
    }

    [HttpGet("facility-comparison")]
    public async Task<ActionResult<ApiResponse<List<FacilityComparisonDto>>>> GetFacilityComparison([FromQuery] int? nam)
    {
        var result = await _dashboardService.GetFacilityComparisonAsync(nam ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<List<FacilityComparisonDto>>.Ok(result));
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<ApiResponse<List<AlertDto>>>> GetAlerts([FromQuery] long? toChucId, [FromQuery] bool? daXuLy)
    {
        var result = await _dashboardService.GetAlertsAsync(toChucId, daXuLy);
        return Ok(ApiResponse<List<AlertDto>>.Ok(result));
    }

    [HttpPatch("alerts/{id}/resolve")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveAlert(long id)
    {
        var result = await _dashboardService.ResolveAlertAsync(id);
        if (!result) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy cảnh báo", 404));
        return Ok(ApiResponse<bool>.Ok(true, "Đã xử lý cảnh báo thành công"));
    }
}
