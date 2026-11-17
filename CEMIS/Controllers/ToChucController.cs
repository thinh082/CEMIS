using CEMIS.Common;
using CEMIS.DTOs;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEMIS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ToChucController : ControllerBase
{
    private readonly IToChucService _toChucService;

    public ToChucController(IToChucService toChucService)
    {
        _toChucService = toChucService;
    }

    [HttpGet("flat")]
    public async Task<ActionResult<ApiResponse<List<ToChucDto>>>> GetAllFlat()
    {
        var list = await _toChucService.GetAllFlatAsync();
        return Ok(ApiResponse<List<ToChucDto>>.Ok(list));
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<List<ToChucDto>>>> GetTree()
    {
        var tree = await _toChucService.GetTreeAsync();
        return Ok(ApiResponse<List<ToChucDto>>.Ok(tree));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ToChucDto>>> GetById(long id)
    {
        var item = await _toChucService.GetByIdAsync(id);
        return Ok(ApiResponse<ToChucDto>.Ok(item));
    }

    [Authorize(Roles = "super_admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ToChucDto>>> Create([FromBody] CreateToChucDto dto)
    {
        var created = await _toChucService.CreateAsync(dto);
        return Ok(ApiResponse<ToChucDto>.Ok(created, "Thêm cơ sở/tổ chức thành công"));
    }

    [Authorize(Roles = "super_admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ToChucDto>>> Update(long id, [FromBody] UpdateToChucDto dto)
    {
        var updated = await _toChucService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ToChucDto>.Ok(updated, "Cập nhật thành công"));
    }

    [Authorize(Roles = "super_admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _toChucService.DeleteAsync(id);
        if (!result) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy tổ chức", 404));
        return Ok(ApiResponse<bool>.Ok(true, "Xóa thành công"));
    }
}
