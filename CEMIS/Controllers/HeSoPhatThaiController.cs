using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEMIS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HeSoPhatThaiController : ControllerBase
{
    private readonly IHeSoPhatThaiService _heSoService;

    public HeSoPhatThaiController(IHeSoPhatThaiService heSoService)
    {
        _heSoService = heSoService;
    }

    [HttpGet("danh-muc-nguon")]
    public async Task<ActionResult<ApiResponse<List<DanhMucNguonPhatThaiDto>>>> GetDanhMucNguon([FromQuery] string? phamVi)
    {
        var list = await _heSoService.GetDanhMucNguonAsync(phamVi);
        return Ok(ApiResponse<List<DanhMucNguonPhatThaiDto>>.Ok(list));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<HeSoPhatThaiDto>>>> GetHeSoList([FromQuery] int? nam, [FromQuery] string? phamVi)
    {
        var list = await _heSoService.GetHeSoListAsync(nam, phamVi);
        return Ok(ApiResponse<List<HeSoPhatThaiDto>>.Ok(list));
    }

    [HttpGet("gwp")]
    public async Task<ActionResult<ApiResponse<List<HeSoGwpDto>>>> GetGwpList()
    {
        var list = await _heSoService.GetGwpListAsync();
        return Ok(ApiResponse<List<HeSoGwpDto>>.Ok(list));
    }

    [Authorize(Roles = "super_admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<HeSoPhatThaiDto>>> CreateHeSo([FromBody] CreateHeSoPhatThaiDto dto)
    {
        var created = await _heSoService.CreateHeSoAsync(dto);
        return Ok(ApiResponse<HeSoPhatThaiDto>.Ok(created, "Thêm hệ số phát thải thành công"));
    }

    [Authorize(Roles = "super_admin")]
    [HttpPatch("{id}/ap-dung")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleApDung(long id, [FromQuery] bool apDung)
    {
        var success = await _heSoService.ToggleApDungAsync(id, apDung);
        if (!success) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy hệ số", 404));
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công"));
    }
}
