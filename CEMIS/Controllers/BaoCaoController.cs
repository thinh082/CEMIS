using System.Threading.Tasks;
using System.Collections.Generic;
using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEMIS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BaoCaoController : ControllerBase
{
    private readonly IBaoCaoService _baoCaoService;
    private readonly IPdfReportService _pdfReportService;
    private readonly ICurrentUserAccess _currentUserAccess;

    public BaoCaoController(
        IBaoCaoService baoCaoService,
        IPdfReportService pdfReportService,
        ICurrentUserAccess currentUserAccess)
    {
        _baoCaoService = baoCaoService;
        _pdfReportService = pdfReportService;
        _currentUserAccess = currentUserAccess;
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequestDto request)
    {
        await _currentUserAccess.AssertCanAccessOrganizationAsync(request.IdToChuc);

        var userId = await _currentUserAccess.GetUserIdAsync();
        var (fileBytes, fileName, contentType) = await _baoCaoService.ExportReportAsync(request, userId);
        return File(fileBytes, contentType, fileName);
    }

    [HttpPost("export-pdf-signed")]
    [HttpPost("export-pdf-confirmed")]
    public async Task<IActionResult> ExportConfirmedPdf([FromBody] ExportReportRequestDto request)
    {
        await _currentUserAccess.AssertCanApproveAsync(request.IdToChuc);

        var userId = await _currentUserAccess.GetUserIdAsync();

        // Kiểm tra loại báo cáo hỗ trợ định dạng PDF
        var reportType = request.LoaiBaoCao?.ToLower().Trim() ?? "kiem_ke_knk_vn";
        if (reportType != "kiem_ke_knk_vn" && 
            reportType != "kiem_ke_knk_vn_signed" && 
            reportType != "kiem_ke_knk_vn_confirmed")
        {
            return BadRequest(ApiResponse<string>.Fail(
                "Định dạng PDF có xác nhận hiện chỉ hỗ trợ Báo cáo kiểm kê KNK cấp cơ sở (Nghị định 06/2022/NĐ-CP). Các báo cáo khác (GRI 302, GRI 305, Đối chiếu kiểm toán) vui lòng xuất định dạng Excel."
            ));
        }

        var (fileBytes, fileName, contentType) = await _pdfReportService.GenerateConfirmedKiemKePdfAsync(
            request.IdToChuc, 
            request.IdKyBaoCao, 
            userId, 
            "Xác nhận Báo cáo Kiểm kê KNK (ISO 14064-1 & GHG Protocol)");

        return File(fileBytes, contentType, fileName);
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<BaoCaoLichSuDto>>>> GetHistory([FromQuery] long? toChucId)
    {
        var effectiveToChucId = await _currentUserAccess.GetEffectiveOrganizationIdAsync(toChucId);
        var list = await _baoCaoService.GetReportHistoryAsync(effectiveToChucId);
        return Ok(ApiResponse<List<BaoCaoLichSuDto>>.Ok(list));
    }
}
