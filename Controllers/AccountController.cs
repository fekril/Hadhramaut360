using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Dtos.Account;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Authenticated user self-management endpoints.
/// All endpoints require a valid JWT — no role restriction beyond being authenticated.
/// The user can only manage their own account; no cross-user access is possible here.
/// </summary>
[ApiController]
[Route(ApiRoutes.Account.Base)]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService     _accountService;
    private readonly ICurrentUserService _currentUserService;

    public AccountController(IAccountService accountService, ICurrentUserService currentUserService)
    {
        _accountService     = accountService;
        _currentUserService = currentUserService;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts userId from JWT via ICurrentUserService.
    /// Returns null and sets Unauthorized if the claim is missing.
    /// </summary>
    private int? GetCurrentUserId() => _currentUserService.GetUserId();

    // ─── Endpoints ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current user's profile.
    /// </summary>
    /// <returns>AccountProfileDto with UserId, FullName, Email, PhoneNumber, ProfileImageUrl, Roles.</returns>
    /// <remarks>
    /// Method: GET
    /// Access: Any authenticated user
    /// </remarks>
    [HttpGet(ApiRoutes.Account.Me)]
    public async Task<ActionResult<ApiResponse<AccountProfileDto>>> GetMe()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _accountService.GetMeAsync(userId.Value);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Updates the current user's FullName, PhoneNumber, and optionally their profile image.
    /// </summary>
    /// <param name="dto">FullName (required), PhoneNumber (optional), ProfileImageFile (optional).</param>
    /// <returns>Updated AccountProfileDto.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Any authenticated user
    /// Content-Type: multipart/form-data
    /// </remarks>
    [HttpPut(ApiRoutes.Account.Me)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<AccountProfileDto>>> UpdateProfile(
        [FromForm] UpdateAccountProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _accountService.UpdateProfileAsync(userId.Value, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Changes the current user's email address after verifying their current password.
    /// </summary>
    /// <param name="dto">NewEmail (required), CurrentPassword (required).</param>
    /// <returns>Updated AccountProfileDto with new email.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Any authenticated user
    /// </remarks>
    [HttpPut(ApiRoutes.Account.Email)]
    public async Task<ActionResult<ApiResponse<AccountProfileDto>>> ChangeEmail([FromBody] ChangeEmailDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _accountService.ChangeEmailAsync(userId.Value, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="dto">CurrentPassword (required), NewPassword (required, min 6 chars).</param>
    /// <returns>ApiResponse&lt;bool&gt; true on success.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Any authenticated user
    /// </remarks>
    [HttpPut(ApiRoutes.Account.Password)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _accountService.ChangePasswordAsync(userId.Value, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Deletes the current user's profile image.
    /// If no image exists the call is silently successful (idempotent).
    /// Physical file is deleted only after the DB update succeeds.
    /// </summary>
    /// <returns>Updated AccountProfileDto (ProfileImageUrl will be null).</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Any authenticated user
    /// </remarks>
    [HttpDelete(ApiRoutes.Account.ProfileImage)]
    public async Task<ActionResult<ApiResponse<AccountProfileDto>>> DeleteProfileImage()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _accountService.DeleteProfileImageAsync(userId.Value);
        return StatusCode(result.StatusCode, result);
    }
}
