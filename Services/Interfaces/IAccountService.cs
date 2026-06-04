using Had360back.Common;
using Had360back.Dtos.Account;
using System.Threading.Tasks;

namespace Had360back.Services.Interfaces;

/// <summary>
/// Service interface for authenticated user self-management.
/// All methods operate on the currently authenticated user identified by userId from JWT.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    Task<ApiResponse<AccountProfileDto>> GetMeAsync(int userId);

    /// <summary>
    /// Updates FullName, PhoneNumber, and optionally the profile image.
    /// If a new image is provided, the old image file is deleted after DB save succeeds.
    /// If DB save fails after image upload, the new image file is deleted.
    /// </summary>
    Task<ApiResponse<AccountProfileDto>> UpdateProfileAsync(int userId, UpdateAccountProfileDto dto);

    /// <summary>
    /// Changes the user's email address after verifying the current password.
    /// Uses UserManager methods so NormalizedEmail is updated correctly.
    /// UserName is NOT changed — it remains the internal "user{id}" value.
    /// </summary>
    Task<ApiResponse<AccountProfileDto>> ChangeEmailAsync(int userId, ChangeEmailDto dto);

    /// <summary>
    /// Changes the user's password using UserManager.ChangePasswordAsync.
    /// Current password is validated by Identity before the change is applied.
    /// </summary>
    Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);

    /// <summary>
    /// Deletes the user's profile image file and clears ProfileImageUrl.
    /// Physical file is deleted only after the DB save succeeds.
    /// Returns success if user has no image (idempotent).
    /// </summary>
    Task<ApiResponse<AccountProfileDto>> DeleteProfileImageAsync(int userId);
}
