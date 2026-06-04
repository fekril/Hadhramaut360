using System;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Common.Messages;
using Had360back.Dtos.Account;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Had360back.Services.Implementations;

/// <summary>
/// Implements self-management operations for the authenticated user:
/// view profile, update profile, change email, change password, delete profile image.
/// All methods receive userId extracted from JWT by the controller via ICurrentUserService.
/// </summary>
public class AccountService : IAccountService
{
    private readonly UserManager<User>   _userManager;
    private readonly IFileStorageService _fileStorageService;

    public AccountService(UserManager<User> userManager, IFileStorageService fileStorageService)
    {
        _userManager        = userManager;
        _fileStorageService = fileStorageService;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Maps a User entity to AccountProfileDto.
    /// Never returns null for FullName — uses fallback chain if data is old.
    /// </summary>
    private async Task<AccountProfileDto> MapToProfileDtoAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AccountProfileDto
        {
            UserId          = user.Id,
            FullName        = !string.IsNullOrWhiteSpace(user.FullName)
                                ? user.FullName
                                : user.Email ?? user.UserName ?? "User",
            Email           = user.Email,
            PhoneNumber     = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            Roles           = roles.ToList()
        };
    }

    // ─── Public Methods ──────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ApiResponse<AccountProfileDto>> GetMeAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<AccountProfileDto>.Fail(ErrorMessages.UserNotFound, null, 404);

        var dto = await MapToProfileDtoAsync(user);
        return ApiResponse<AccountProfileDto>.Ok(dto, SuccessMessages.RetrievedSuccessfully);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<AccountProfileDto>> UpdateProfileAsync(int userId, UpdateAccountProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<AccountProfileDto>.Fail(ErrorMessages.UserNotFound, null, 404);

        // ── Update text fields ────────────────────────────────────────────────
        user.FullName    = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        // ── Handle profile image upload ───────────────────────────────────────
        string? oldImageUrl     = user.ProfileImageUrl;
        string? uploadedImageUrl = null;

        if (dto.ProfileImageFile != null)
        {
            try
            {
                uploadedImageUrl = await _fileStorageService.SaveUserProfileImageAsync(
                    user.Id, dto.ProfileImageFile);
                user.ProfileImageUrl = uploadedImageUrl;
            }
            catch (Exception)
            {
                if (uploadedImageUrl != null)
                    await _fileStorageService.DeleteFileAsync(uploadedImageUrl);
                return ApiResponse<AccountProfileDto>.Fail(
                    "Failed to upload profile image.", null, 500);
            }
        }

        // ── Persist changes ───────────────────────────────────────────────────
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            // DB save failed — clean up newly uploaded file
            if (uploadedImageUrl != null)
                await _fileStorageService.DeleteFileAsync(uploadedImageUrl);

            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AccountProfileDto>.Fail(
                string.Join(", ", errors), errors, 400);
        }

        // ── Delete old image ONLY after successful DB save ────────────────────
        if (uploadedImageUrl != null && oldImageUrl != null)
            await _fileStorageService.DeleteFileAsync(oldImageUrl);

        var profileDto = await MapToProfileDtoAsync(user);
        return ApiResponse<AccountProfileDto>.Ok(profileDto, SuccessMessages.ProfileUpdatedSuccessfully);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<AccountProfileDto>> ChangeEmailAsync(int userId, ChangeEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<AccountProfileDto>.Fail(ErrorMessages.UserNotFound, null, 404);

        // Verify current password before allowing email change
        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
        if (!passwordValid)
            return ApiResponse<AccountProfileDto>.Fail(ErrorMessages.IncorrectPassword, null, 400);

        // Check new email not already taken
        var existing = await _userManager.FindByEmailAsync(dto.NewEmail);
        if (existing != null && existing.Id != userId)
            return ApiResponse<AccountProfileDto>.Fail(ErrorMessages.EmailAlreadyExists, null, 409);

        // Generate email change token and apply — updates NormalizedEmail correctly
        var token  = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);
        var result = await _userManager.ChangeEmailAsync(user, dto.NewEmail, token);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AccountProfileDto>.Fail(
                ErrorMessages.ChangeEmailFailed, errors, 400);
        }

        // NOTE: UserName is NOT updated — it remains "user{id}" (internal, not shown to user)

        var profileDto = await MapToProfileDtoAsync(user);
        return ApiResponse<AccountProfileDto>.Ok(profileDto, SuccessMessages.EmailChangedSuccessfully);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<bool>.Fail(ErrorMessages.UserNotFound, null, 404);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<bool>.Fail(ErrorMessages.ChangePasswordFailed, errors, 400);
        }

        return ApiResponse<bool>.Ok(true, SuccessMessages.PasswordChangedSuccessfully);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<AccountProfileDto>> DeleteProfileImageAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResponse<AccountProfileDto>.Fail(ErrorMessages.UserNotFound, null, 404);

        if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            // No image — return current profile unchanged (idempotent)
            var emptyDto = await MapToProfileDtoAsync(user);
            return ApiResponse<AccountProfileDto>.Ok(emptyDto, SuccessMessages.ProfileImageDeletedSuccessfully);
        }

        var imageUrlToDelete = user.ProfileImageUrl;
        user.ProfileImageUrl = null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AccountProfileDto>.Fail(
                ErrorMessages.UnexpectedError, errors, 500);
        }

        // Delete physical file ONLY after successful DB save
        await _fileStorageService.DeleteFileAsync(imageUrlToDelete);

        var profileDto = await MapToProfileDtoAsync(user);
        return ApiResponse<AccountProfileDto>.Ok(profileDto, SuccessMessages.ProfileImageDeletedSuccessfully);
    }
}
