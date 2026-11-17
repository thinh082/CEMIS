using System.Threading.Tasks;
using CEMIS.Models;

namespace CEMIS.Services.Interfaces;

public interface ICurrentUserAccess
{
    /// <summary>
    /// Lấy thông tin người dùng hiện tại từ Database dựa trên JWT Claims.
    /// Tự động kiểm tra trạng thái hoạt động (bị khóa sẽ ném ngoại lệ 403).
    /// Có cache trong phạm vi request (Scoped).
    /// </summary>
    Task<NguoiDung> GetCurrentUserAsync();

    Task<long> GetUserIdAsync();
    Task<string> GetRoleCodeAsync();
    Task<long?> GetUserOrganizationIdAsync();

    Task<bool> IsSuperAdminAsync();
    Task<bool> IsFacilityManagerAsync();
    Task<bool> IsDataEntryAsync();
    Task<bool> IsAuditorAsync();

    /// <summary>
    /// Xác định ID tổ chức có hiệu lực cho truy vấn:
    /// - Nếu là Super Admin: trả về requestedOrgId (nếu null là toàn tập đoàn).
    /// - Nếu là Non-Admin:
    ///   + Chưa gán cơ sở: ném ngoại lệ 403 (yêu cầu liên hệ admin).
    ///   + requestedOrgId khác cơ sở được gán: ném ngoại lệ 403.
    ///   + Trả về ID cơ sở được gán.
    /// </summary>
    Task<long?> GetEffectiveOrganizationIdAsync(long? requestedOrgId);

    /// <summary>
    /// Kiểm tra người dùng có quyền xem/truy cập cơ sở targetOrgId không.
    /// </summary>
    Task AssertCanAccessOrganizationAsync(long targetOrgId);

    /// <summary>
    /// Kiểm tra người dùng có quyền thêm/sửa/xóa dữ liệu tại targetOrgId không:
    /// Kiểm toán viên bị từ chối; non-admin khác cơ sở bị từ chối.
    /// </summary>
    Task AssertCanModifyDataAsync(long targetOrgId);

    /// <summary>
    /// Kiểm tra người dùng có quyền phê duyệt hoặc khóa sổ kỳ báo cáo tại targetOrgId không:
    /// Chỉ Super Admin hoặc Quản lý cơ sở của chính cơ sở đó mới được duyệt.
    /// Nhân viên nhập liệu và Kiểm toán viên bị từ chối.
    /// </summary>
    Task AssertCanApproveAsync(long targetOrgId);

    /// <summary>
    /// Kiểm tra quyền quản trị người dùng (chỉ Super Admin).
    /// </summary>
    Task AssertCanManageUsersAsync();
}
