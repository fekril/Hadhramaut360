using Had360back.Common;
using Had360back.Dtos.AdminUsers;
using System.Threading.Tasks;

namespace Had360back.Services.Interfaces;

/// <summary>
/// Service interface for admin user management operations.
/// All methods are restricted to Admin role at the policy level.
/// </summary>
public interface IAdminUserService
{
    /// <summary>
    /// Returns a paginated, filterable list of all users.
    /// Supports search by UserName, Email, PhoneNumber and filter by Role.
    /// </summary>
    Task<ApiResponse<PaginatedResponse<AdminUserResponseDto>>> GetUsersAsync(AdminUserQueryDto query);

    /// <summary>
    /// Creates a new ContentManager account.
    /// Returns the created user as AdminUserResponseDto (no JWT token).
    /// </summary>
    Task<ApiResponse<AdminUserResponseDto>> CreateContentManagerAsync(CreateContentManagerDto dto);

    /// <summary>
    /// Permanently deletes a user and all their related data.
    /// Enforces safety rules: cannot delete self, cannot delete last Admin.
    /// Uses an EF Core transaction; physical files deleted only after commit.
    /// </summary>
    /// <param name="targetUserId">ID of the user to delete.</param>
    /// <param name="requestingAdminId">ID of the admin performing the deletion.</param>
    Task<ApiResponse<bool>> DeleteUserAsync(int targetUserId, int requestingAdminId);
}
