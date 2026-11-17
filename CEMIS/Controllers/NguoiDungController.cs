using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEMIS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NguoiDungController : ControllerBase
{
    private readonly INguoiDungService _nguoiDungService;

    public NguoiDungController(INguoiDungService nguoiDungService)
    {
        _nguoiDungService = nguoiDungService;
    }

    [Authorize(Roles = "super_admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NguoiDungDto>>>> GetList()
    {
        var list = await _nguoiDungService.GetListAsync();
        return Ok(ApiResponse<List<NguoiDungDto>>.Ok(list));
    }

    [Authorize(Roles = "super_admin")]
    [HttpGet("vai-tro")]
    public async Task<ActionResult<ApiResponse<List<VaiTroDto>>>> GetVaiTroList()
    {
        var list = await _nguoiDungService.GetVaiTroListAsync();
        return Ok(ApiResponse<List<VaiTroDto>>.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<NguoiDungDto>>> GetById(long id)
    {
        var user = await _nguoiDungService.GetByIdAsync(id);
        return Ok(ApiResponse<NguoiDungDto>.Ok(user));
    }

    [Authorize(Roles = "super_admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<NguoiDungDto>>> Create([FromBody] CreateNguoiDungDto dto)
    {
        var created = await _nguoiDungService.CreateAsync(dto);
        return Ok(ApiResponse<NguoiDungDto>.Ok(created, "Tạo tài khoản người dùng thành công"));
    }

    [Authorize(Roles = "super_admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<NguoiDungDto>>> Update(long id, [FromBody] UpdateNguoiDungDto dto)
    {
        var updated = await _nguoiDungService.UpdateAsync(id, dto);
        return Ok(ApiResponse<NguoiDungDto>.Ok(updated, "Cập nhật tài khoản thành công"));
    }

    [Authorize(Roles = "super_admin")]
    [HttpPatch("{id}/khoa")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleLock(long id)
    {
        var result = await _nguoiDungService.ToggleLockAsync(id);
        if (!result) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy người dùng", 404));
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái tài khoản thành công"));
    }

    [HttpPatch("{id}/chu-ky-anh")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSignature(long id, [FromBody] UpdateSignatureDto dto)
    {
        var result = await _nguoiDungService.UpdateSignatureAsync(id, dto.DuongDanChuKyAnh);
        if (!result) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy người dùng", 404));
        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật đường dẫn ảnh chữ ký thành công"));
    }

    [HttpPost("{id}/upload-chu-ky")]
    public async Task<ActionResult<ApiResponse<string>>> UploadSignature(long id, IFormFile file)
    {
        var path = await _nguoiDungService.UploadSignatureAsync(id, file);
        return Ok(ApiResponse<string>.Ok(path, "Tải lên ảnh chữ ký thành công"));
    }
}
