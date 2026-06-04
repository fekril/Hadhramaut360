using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Dtos.AdminUsers;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Admin-only controller for user management.
/// Provides user listing, ContentManager creation, and permanent user deletion.
/// ContentManager role does NOT have access to this controller.
/// </summary>
[ApiController]
[Route(ApiRoutes.AdminUsers.Base)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService   _adminUserService;
    private readonly ICurrentUserService _currentUserService;

    public AdminUsersController(
        IAdminUserService   adminUserService,
        ICurrentUserService currentUserService)
    {
        _adminUserService   = adminUserService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns a paginated list of all users with optional search and role filtering.
    /// </summary>
    /// <param name="query">Search, Role, PageNumber, PageSize.</param>
    /// <returns>Paginated list of AdminUserResponseDto.</returns>
    /// <remarks>
    /// Method: GET
    /// Access: Admin only
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<AdminUserResponseDto>>>> GetUsers(
        [FromQuery] AdminUserQueryDto query)
    {
        var result = await _adminUserService.GetUsersAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Creates a new ContentManager account.
    /// Does NOT return a JWT token — the account is created for management purposes only.
    /// </summary>
    /// <param name="dto">UserName, Email, Password (required); PhoneNumber, ProfileImage (optional).</param>
    /// <returns>The created ContentManager as AdminUserResponseDto with status 201.</returns>
    /// <remarks>
    /// Method: POST
    /// Access: Admin only
    /// Content-Type: multipart/form-data
    /// </remarks>
    [HttpPost(ApiRoutes.AdminUsers.ContentManagers)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<AdminUserResponseDto>>> CreateContentManager(
        [FromForm] CreateContentManagerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _adminUserService.CreateContentManagerAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Permanently deletes a user and all related data.
    /// Safety rules: Admin cannot delete themselves or the last Admin account.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>true on success, or an error response with details.</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Admin only
    ///
    /// Data deleted: profile image, favorites, community posts + media + reports,
    /// reports created by the user, and if Organisation: org record + logo + events + event media.
    /// Physical files are deleted only after the database transaction commits successfully.
    /// </remarks>
    [HttpDelete(ApiRoutes.AdminUsers.ById)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
    {
        var requestingAdminId = _currentUserService.GetUserId();
        if (!requestingAdminId.HasValue)
            return Unauthorized();

        var result = await _adminUserService.DeleteUserAsync(id, requestingAdminId.Value);
        return StatusCode(result.StatusCode, result);
    }
}
