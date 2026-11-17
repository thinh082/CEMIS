using System.Security.Claims;
using System.Threading.Tasks;
using CEMIS.Common;
using CEMIS.Models;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Services.Implementations;

public class CurrentUserAccess : ICurrentUserAccess
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CemisDbContext _context;
    private NguoiDung? _cachedUser;

    public CurrentUserAccess(IHttpContextAccessor httpContextAccessor, CemisDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<NguoiDung> GetCurrentUserAsync()
    {
        if (_cachedUser != null)
        {
            return _cachedUser;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
        {
            throw new AppUnauthorizedException("Yêu cầu đăng nhập để truy cập tài nguyên này.");
        }

        var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
        {
            throw new AppUnauthorizedException("Không xác định được danh tính người dùng trong phiên làm việc.");
        }

        var user = await _context.NguoiDungs
            .Include(u => u.IdVaiTroNavigation)
            .Include(u => u.IdToChucNavigation)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new AppUnauthorizedException("Tài khoản người dùng không tồn tại trong hệ thống.");
        }

        if (user.TrangThai == "khoa")
        {
            throw new AppForbiddenException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        _cachedUser = user;
        return _cachedUser;
    }

    public async Task<long> GetUserIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user.Id;
    }

    public async Task<string> GetRoleCodeAsync()
    {
        var user = await GetCurrentUserAsync();
        return user.IdVaiTroNavigation?.MaVaiTro ?? string.Empty;
    }

    public async Task<long?> GetUserOrganizationIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user.IdToChuc;
    }

    public async Task<bool> IsSuperAdminAsync()
    {
        var role = await GetRoleCodeAsync();
        return role == "super_admin";
    }

    public async Task<bool> IsFacilityManagerAsync()
    {
        var role = await GetRoleCodeAsync();
        return role == "quan_ly_co_so";
    }

    public async Task<bool> IsDataEntryAsync()
    {
        var role = await GetRoleCodeAsync();
        return role == "nhan_vien_nhap_lieu";
    }

    public async Task<bool> IsAuditorAsync()
    {
        var role = await GetRoleCodeAsync();
        return role == "kiem_toan_vien";
    }

    public async Task<long?> GetEffectiveOrganizationIdAsync(long? requestedOrgId)
    {
        var user = await GetCurrentUserAsync();
        var role = user.IdVaiTroNavigation?.MaVaiTro ?? string.Empty;

        // Super Admin có quyền xem tất cả (khi requestedOrgId = null) hoặc lọc theo cơ sở cụ thể
        if (role == "super_admin")
        {
            return requestedOrgId;
        }

        // Người dùng không phải admin bắt buộc phải được gán cơ sở
        if (!user.IdToChuc.HasValue)
        {
            throw new AppForbiddenException("Tài khoản chưa được phân công cơ sở hoạt động. Vui lòng liên hệ quản trị viên.");
        }

        // Nếu non-admin yêu cầu xem cơ sở khác với cơ sở được gán -> Chặn truy cập 403
        if (requestedOrgId.HasValue && requestedOrgId.Value != user.IdToChuc.Value)
        {
            throw new AppForbiddenException("Bạn không có quyền truy cập dữ liệu của cơ sở khác.");
        }

        return user.IdToChuc.Value;
    }

    public async Task AssertCanAccessOrganizationAsync(long targetOrgId)
    {
        var user = await GetCurrentUserAsync();
        var role = user.IdVaiTroNavigation?.MaVaiTro ?? string.Empty;

        if (role == "super_admin")
        {
            return;
        }

        if (!user.IdToChuc.HasValue)
        {
            throw new AppForbiddenException("Tài khoản chưa được phân công cơ sở hoạt động. Vui lòng liên hệ quản trị viên.");
        }

        if (user.IdToChuc.Value != targetOrgId)
        {
            throw new AppForbiddenException("Bạn không có quyền truy cập dữ liệu của cơ sở này.");
        }
    }

    public async Task AssertCanModifyDataAsync(long targetOrgId)
    {
        await AssertCanAccessOrganizationAsync(targetOrgId);

        var role = await GetRoleCodeAsync();
        if (role == "kiem_toan_vien")
        {
            throw new AppForbiddenException("Kiểm toán viên chỉ có quyền xem dữ liệu, không được tạo, chỉnh sửa hoặc xóa dữ liệu.");
        }
    }

    public async Task AssertCanApproveAsync(long targetOrgId)
    {
        await AssertCanAccessOrganizationAsync(targetOrgId);

        var role = await GetRoleCodeAsync();
        if (role == "kiem_toan_vien")
        {
            throw new AppForbiddenException("Kiểm toán viên không có quyền phê duyệt.");
        }

        if (role == "nhan_vien_nhap_lieu")
        {
            throw new AppForbiddenException("Nhân viên nhập liệu không có quyền phê duyệt hoặc khóa sổ kỳ báo cáo.");
        }

        if (role != "super_admin" && role != "quan_ly_co_so")
        {
            throw new AppForbiddenException("Chỉ Quản lý cơ sở hoặc Quản trị hệ thống mới có quyền phê duyệt hoặc khóa sổ.");
        }
    }

    public async Task AssertCanManageUsersAsync()
    {
        var role = await GetRoleCodeAsync();
        if (role != "super_admin")
        {
            throw new AppForbiddenException("Chỉ Quản trị hệ thống (Super Admin) mới có quyền quản trị người dùng và phân quyền.");
        }
    }
}
