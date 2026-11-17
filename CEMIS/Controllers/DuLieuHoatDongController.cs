using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEMIS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DuLieuHoatDongController : ControllerBase
{
    private readonly IDuLieuHoatDongService _duLieuService;
    private readonly ICurrentUserAccess _currentUserAccess;

    public DuLieuHoatDongController(
        IDuLieuHoatDongService duLieuService,
        ICurrentUserAccess currentUserAccess)
    {
        _duLieuService = duLieuService;
        _currentUserAccess = currentUserAccess;
    }

    #region Kỳ báo cáo
    [HttpGet("ky-bao-cao")]
    public async Task<ActionResult<ApiResponse<List<KyBaoCaoDto>>>> GetKyBaoCaoList([FromQuery] long? toChucId, [FromQuery] int? nam)
    {
        var list = await _duLieuService.GetKyBaoCaoListAsync(toChucId, nam);
        return Ok(ApiResponse<List<KyBaoCaoDto>>.Ok(list));
    }

    [Authorize(Roles = "super_admin,quan_ly_co_so")]
    [HttpPost("ky-bao-cao")]
    public async Task<ActionResult<ApiResponse<KyBaoCaoDto>>> CreateKyBaoCao([FromBody] CreateKyBaoCaoDto dto)
    {
        var created = await _duLieuService.CreateKyBaoCaoAsync(dto);
        return Ok(ApiResponse<KyBaoCaoDto>.Ok(created, "Tạo kỳ báo cáo thành công"));
    }

    [Authorize(Roles = "super_admin,quan_ly_co_so")]
    [HttpPatch("ky-bao-cao/{id}/khoa-so")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleKhoaSo(long id, [FromQuery] string trangThai)
    {
        var result = await _duLieuService.ToggleKhoaSoAsync(id, trangThai);
        if (!result) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy kỳ báo cáo", 404));
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái sổ thành công"));
    }
    #endregion

    #region Dữ liệu hoạt động (Activity Data)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DuLieuHoatDongDto>>>> GetDuLieuList(
        [FromQuery] long? toChucId,
        [FromQuery] long? kyBaoCaoId,
        [FromQuery] string? phamVi,
        [FromQuery] string? trangThaiDuyet)
    {
        var list = await _duLieuService.GetDuLieuListAsync(toChucId, kyBaoCaoId, phamVi, trangThaiDuyet);
        return Ok(ApiResponse<List<DuLieuHoatDongDto>>.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DuLieuHoatDongDto>>> GetById(long id)
    {
        var item = await _duLieuService.GetDuLieuByIdAsync(id);
        return Ok(ApiResponse<DuLieuHoatDongDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DuLieuHoatDongDto>>> Create([FromBody] CreateDuLieuHoatDongDto dto)
    {
        var userId = await _currentUserAccess.GetUserIdAsync();
        var created = await _duLieuService.CreateDuLieuAsync(dto, userId);
        return Ok(ApiResponse<DuLieuHoatDongDto>.Ok(created, "Kê khai dữ liệu thành công"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DuLieuHoatDongDto>>> Update(long id, [FromBody] UpdateDuLieuHoatDongDto dto)
    {
        var updated = await _duLieuService.UpdateDuLieuAsync(id, dto);
        return Ok(ApiResponse<DuLieuHoatDongDto>.Ok(updated, "Cập nhật dữ liệu thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _duLieuService.DeleteDuLieuAsync(id);
        if (!result) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy dữ liệu", 404));
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công"));
    }

    [Authorize(Roles = "super_admin,quan_ly_co_so")]
    [HttpPost("{id}/phe-duyet")]
    public async Task<ActionResult<ApiResponse<bool>>> PheDuyet(long id, [FromBody] PheDuyetRequestDto dto)
    {
        var userId = await _currentUserAccess.GetUserIdAsync();
        var result = await _duLieuService.PheDuyetDuLieuAsync(id, dto, userId);
        return Ok(ApiResponse<bool>.Ok(result, "Xử lý phê duyệt thành công"));
    }

    [HttpPost("{id}/upload-minh-chung")]
    public async Task<ActionResult<ApiResponse<FileMinhChungDto>>> UploadMinhChung(long id, IFormFile file, [FromForm] string? ghiChu)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<FileMinhChungDto>.Fail("Vui lòng chọn tệp tin hóa đơn/minh chứng hợp lệ."));

        var result = await _duLieuService.UploadMinhChungAsync(id, file, ghiChu);
        return Ok(ApiResponse<FileMinhChungDto>.Ok(result, "Tải lên minh chứng thành công"));
    }

    [HttpPost("batch-import")]
    public async Task<ActionResult<ApiResponse<int>>> BatchImport(
        [FromForm] long toChucId,
        [FromForm] long kyBaoCaoId,
        IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
            return BadRequest(ApiResponse<int>.Fail("Vui lòng chọn file Excel mẫu hợp lệ."));

        var userId = await _currentUserAccess.GetUserIdAsync();
        var count = await _duLieuService.BatchImportExcelAsync(toChucId, kyBaoCaoId, userId, excelFile);
        return Ok(ApiResponse<int>.Ok(count, $"Đã nhập thành công {count} dòng dữ liệu"));
    }

    [AllowAnonymous]
    [HttpGet("download-template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var (fileBytes, fileName, contentType) = await _duLieuService.GenerateImportTemplateAsync();
        return File(fileBytes, contentType, fileName);
    }

    [HttpGet("file/{fileId}")]
    public async Task<IActionResult> GetFileMinhChung(long fileId)
    {
        var (fileBytes, fileName, contentType) = await _duLieuService.GetFileMinhChungAsync(fileId);
        return File(fileBytes, contentType, fileName);
    }
    #endregion
}
