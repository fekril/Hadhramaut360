using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Common.Messages;
using Had360back.Data;
using Had360back.Dtos.AdminUsers;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// Implements admin-only user management: list, create ContentManager, delete user.
/// Deletion uses an EF Core transaction; physical files are deleted only after a
/// successful commit to avoid leaving the database in an inconsistent state.
/// </summary>
public class AdminUserService : IAdminUserService
{
    private readonly DbContextApp         _context;
    private readonly UserManager<User>    _userManager;
    private readonly IFileStorageService  _fileStorageService;

    public AdminUserService(
        DbContextApp        context,
        UserManager<User>   userManager,
        IFileStorageService fileStorageService)
    {
        _context            = context;
        _userManager        = userManager;
        _fileStorageService = fileStorageService;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Maps a User entity to AdminUserResponseDto and resolves the user's roles.
    /// </summary>
    private async Task<AdminUserResponseDto> MapToResponseDtoAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AdminUserResponseDto
        {
            UserId          = user.Id,
            UserName        = user.UserName,
            Email           = user.Email,
            PhoneNumber     = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            Roles           = roles.ToList()
        };
    }

    /// <summary>
    /// Normalizes pagination parameters to prevent invalid values.
    /// </summary>
    private static (int pageNumber, int pageSize) NormalizePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize   < 1) pageSize   = 10;
        if (pageSize   > 50) pageSize  = 50;
        return (pageNumber, pageSize);
    }

    /// <summary>
    /// Builds a PaginatedResponse from a list of DTOs and pagination metadata.
    /// </summary>
    private static PaginatedResponse<AdminUserResponseDto> BuildPaginatedResponse(
        List<AdminUserResponseDto> dtos, int totalEntries, int currentPage, int perPage)
    {
        var totalPages = perPage > 0 ? (int)Math.Ceiling((double)totalEntries / perPage) : 0;
        return new PaginatedResponse<AdminUserResponseDto>
        {
            Items = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage  = currentPage,
                NextPage     = currentPage < totalPages ? (int?)(currentPage + 1) : null,
                PerPage      = perPage,
                PreviousPage = currentPage > 1 ? (int?)(currentPage - 1) : null,
                TotalEntries = totalEntries,
                TotalPages   = totalPages
            }
        };
    }

    /// <summary>
    /// Checks whether any other Admin account exists besides the given user.
    /// Used to prevent deleting the last Admin.
    /// </summary>
    private async Task<bool> IsLastAdminAsync(int userId)
    {
        var admins = await _userManager.GetUsersInRoleAsync(IdentityRoles.Admin);
        return admins.Count == 1 && admins[0].Id == userId;
    }

    /// <summary>
    /// Returns the role name that is valid for filtering, or null if the value
    /// provided does not match any known role.
    /// </summary>
    private static bool IsValidRoleFilter(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return true; // no filter = valid
        return role == IdentityRoles.Admin
            || role == IdentityRoles.ContentManager
            || role == IdentityRoles.Organization
            || role == IdentityRoles.User;
    }

    // ─── Public Methods ──────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ApiResponse<PaginatedResponse<AdminUserResponseDto>>> GetUsersAsync(AdminUserQueryDto query)
    {
        if (!IsValidRoleFilter(query.Role))
            return ApiResponse<PaginatedResponse<AdminUserResponseDto>>
                .Fail(ErrorMessages.InvalidRole, null, 400);

        var (pageNumber, pageSize) = NormalizePagination(query.PageNumber, query.PageSize);

        // Build base query
        IQueryable<User> dbQuery = _context.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var lowerSearch = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(u =>
                (u.UserName    != null && u.UserName.ToLower().Contains(lowerSearch)) ||
                (u.Email       != null && u.Email.ToLower().Contains(lowerSearch))    ||
                (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(lowerSearch)));
        }

        // Apply role filter — requires joining AspNetUserRoles
        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            // We resolve user IDs in that role via UserManager to stay consistent
            // with Identity's role store, then filter the EF query by those IDs.
            var usersInRole = await _userManager.GetUsersInRoleAsync(query.Role);
            var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
            dbQuery = dbQuery.Where(u => userIdsInRole.Contains(u.Id));
        }

        var totalEntries = await dbQuery.CountAsync();

        var users = await dbQuery
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map with roles (runs N small queries, acceptable for admin page sizes)
        var dtos = new List<AdminUserResponseDto>(users.Count);
        foreach (var user in users)
            dtos.Add(await MapToResponseDtoAsync(user));

        var paginatedResponse = BuildPaginatedResponse(dtos, totalEntries, pageNumber, pageSize);
        return ApiResponse<PaginatedResponse<AdminUserResponseDto>>
            .Ok(paginatedResponse, SuccessMessages.RetrievedSuccessfully);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<AdminUserResponseDto>> CreateContentManagerAsync(CreateContentManagerDto dto)
    {
        // Check email uniqueness
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return ApiResponse<AdminUserResponseDto>.Fail(ErrorMessages.EmailAlreadyExists, null, 409);

        var user = new User
        {
            UserName    = dto.UserName,
            Email       = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt   = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        string? uploadedImageUrl = null;

        try
        {
            // 1. Create the user with Identity
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<AdminUserResponseDto>.Fail(
                    string.Join(", ", errors), errors, 400);
            }

            // 2. Assign ContentManager role
            var roleResult = await _userManager.AddToRoleAsync(user, IdentityRoles.ContentManager);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                await transaction.RollbackAsync();
                return ApiResponse<AdminUserResponseDto>.Fail(
                    ErrorMessages.UnexpectedError, null, 500);
            }

            // 3. Save profile image if provided
            if (dto.ProfileImageFile != null)
            {
                try
                {
                    uploadedImageUrl = await _fileStorageService.SaveUserProfileImageAsync(
                        user.Id, dto.ProfileImageFile);
                    user.ProfileImageUrl = uploadedImageUrl;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        // Cleanup: delete uploaded file, remove user, rollback
                        await _fileStorageService.DeleteFileAsync(uploadedImageUrl);
                        await _userManager.DeleteAsync(user);
                        await transaction.RollbackAsync();
                        return ApiResponse<AdminUserResponseDto>.Fail(
                            ErrorMessages.UnexpectedError, null, 500);
                    }
                }
                catch (Exception)
                {
                    if (uploadedImageUrl != null)
                        await _fileStorageService.DeleteFileAsync(uploadedImageUrl);
                    await _userManager.DeleteAsync(user);
                    await transaction.RollbackAsync();
                    return ApiResponse<AdminUserResponseDto>.Fail(
                        "Failed to upload profile image. User was not created.", null, 500);
                }
            }

            await transaction.CommitAsync();

            var responseDto = await MapToResponseDtoAsync(user);
            return ApiResponse<AdminUserResponseDto>.Ok(
                responseDto, SuccessMessages.ContentManagerCreatedSuccessfully, 201);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            if (uploadedImageUrl != null)
                await _fileStorageService.DeleteFileAsync(uploadedImageUrl);
            return ApiResponse<AdminUserResponseDto>.Fail(ErrorMessages.UnexpectedError, null, 500);
        }
    }

    /// <inheritdoc />
    public async Task<ApiResponse<bool>> DeleteUserAsync(int targetUserId, int requestingAdminId)
    {
        // ── Safety: cannot delete yourself ──────────────────────────────────────
        if (targetUserId == requestingAdminId)
            return ApiResponse<bool>.Fail(ErrorMessages.CannotDeleteYourself, null, 400);

        // ── Find user ───────────────────────────────────────────────────────────
        var user = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (user == null)
            return ApiResponse<bool>.Fail(ErrorMessages.UserNotFound, null, 404);

        // ── Safety: cannot delete last Admin ────────────────────────────────────
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Contains(IdentityRoles.Admin) && await IsLastAdminAsync(targetUserId))
            return ApiResponse<bool>.Fail(ErrorMessages.CannotDeleteLastAdmin, null, 400);

        // ── Collect all physical file URLs before touching the DB ────────────────
        var filesToDelete = new List<string?>();

        // 1. User profile image
        filesToDelete.Add(user.ProfileImageUrl);

        // 2. Organization logo + event media (if user is an Organization)
        var organization = await _context.Organizations
            .Include(o => o.Events)
                .ThenInclude(e => e.EventMedia)
            .FirstOrDefaultAsync(o => o.UserId == targetUserId);

        if (organization != null)
        {
            filesToDelete.Add(organization.LogoUrl);

            foreach (var ev in organization.Events)
                foreach (var em in ev.EventMedia)
                    filesToDelete.Add(em.MediaUrl);
        }

        // 3. Community post media
        var communityPosts = await _context.CommunityPosts
            .Include(p => p.PostMedia)
            .Include(p => p.Reports)
            .Where(p => p.UserId == targetUserId)
            .ToListAsync();

        foreach (var post in communityPosts)
            foreach (var pm in post.PostMedia)
                filesToDelete.Add(pm.MediaUrl);

        // ── Begin transaction ────────────────────────────────────────────────────
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ── 3a. Delete Reports made BY this user ─────────────────────────────
            // (reports where Report.UserId == targetUserId — these are safe to remove
            //  regardless of whether the posts still exist)
            var reportsByUser = await _context.Reports
                .Where(r => r.UserId == targetUserId)
                .ToListAsync();
            _context.Reports.RemoveRange(reportsByUser);
            await _context.SaveChangesAsync();

            // ── 3b. Delete Community Posts + their PostMedia + Reports on those posts
            foreach (var post in communityPosts)
            {
                // Delete reports ON this post (from other users)
                _context.Reports.RemoveRange(post.Reports);
                // Delete post media records
                _context.PostMedia.RemoveRange(post.PostMedia);
            }
            _context.CommunityPosts.RemoveRange(communityPosts);
            await _context.SaveChangesAsync();

            // ── 4. Delete Favorites created by this user ─────────────────────────
            var favorites = await _context.Favorites
                .Where(f => f.UserId == targetUserId)
                .ToListAsync();
            _context.Favorites.RemoveRange(favorites);
            await _context.SaveChangesAsync();

            // ── 5. Organization + Events + EventMedia ────────────────────────────
            if (organization != null)
            {
                foreach (var ev in organization.Events)
                {
                    _context.EventMedia.RemoveRange(ev.EventMedia);
                }
                _context.Events.RemoveRange(organization.Events);
                _context.Organizations.Remove(organization);
                await _context.SaveChangesAsync();
            }

            // ── 6. Delete Identity user (removes AspNetUsers + AspNetUserRoles) ──
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = deleteResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<bool>.Fail(ErrorMessages.DeleteUserFailed, errors, 500);
            }

            await transaction.CommitAsync();

            // ── 7. Delete physical files AFTER successful commit ─────────────────
            foreach (var fileUrl in filesToDelete)
                await _fileStorageService.DeleteFileAsync(fileUrl);

            return ApiResponse<bool>.Ok(true, SuccessMessages.UserDeletedSuccessfully);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            // Do NOT delete physical files — DB was rolled back, data is intact
            return ApiResponse<bool>.Fail(ErrorMessages.UnexpectedError, null, 500);
        }
    }
}
