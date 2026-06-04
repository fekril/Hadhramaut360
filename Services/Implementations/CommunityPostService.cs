using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.CommunityPosts;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// تنفيذ سيرفس إدارة المنشورات المجتمعية وميديا المنشورات.
/// يوفر منطق الأعمال لجميع عمليات الفيد المجتمعي وإدارة المحتوى والبلاغات.
/// </summary>
public class CommunityPostService : ICommunityPostService
{
    private readonly DbContextApp _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly UserManager<User> _userManager;

    private const int MaxImagesPerPost = 5;

    public CommunityPostService(
        DbContextApp context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        UserManager<User> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// يُرجع معرّف المستخدم الحالي أو -1 إذا كان غير مُسجّل دخوله.
    /// </summary>
    private int GetCurrentUserId() => _currentUserService.GetUserId() ?? -1;

    /// <summary>
    /// يتحقق إذا كان المستخدم الحالي في أحد الأدوار المعطاة.
    /// </summary>
    private async Task<bool> CurrentUserIsInRoleAsync(params string[] roles)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue) return false;

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null) return false;

        foreach (var role in roles)
        {
            if (await _userManager.IsInRoleAsync(user, role))
                return true;
        }
        return false;
    }

    /// <summary>
    /// يتحقق إذا كان المستخدم الحالي يملك صلاحية إدارة المنشور.
    /// الأدمن ومدير المحتوى يملكان الصلاحية دائمًا.
    /// صاحب المنشور يملك الصلاحية فقط على منشوره.
    /// </summary>
    private async Task<bool> CanManagePostAsync(CommunityPost post)
    {
        if (await CurrentUserIsInRoleAsync(IdentityRoles.Admin, IdentityRoles.ContentManager))
            return true;

        return post.UserId == GetCurrentUserId();
    }

    /// <summary>
    /// يُطبّق منطق تطبيع قيم الصفحة لمنع القيم غير الصالحة.
    /// </summary>
    private static (int pageNumber, int pageSize) NormalizePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;
        return (pageNumber, pageSize);
    }

    /// <summary>
    /// يبني PaginatedResponse من قائمة الـ DTOs وبيانات التقسيم.
    /// </summary>
    private static PaginatedResponse<CommunityPostResponseDto> BuildPaginatedResponse(
        List<CommunityPostResponseDto> dtos, int totalEntries, int currentPage, int perPage)
    {
        var totalPages = perPage > 0 ? (int)Math.Ceiling((double)totalEntries / perPage) : 0;
        return new PaginatedResponse<CommunityPostResponseDto>
        {
            Items = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage = currentPage,
                NextPage = currentPage < totalPages ? (int?)(currentPage + 1) : null,
                PerPage = perPage,
                PreviousPage = currentPage > 1 ? (int?)(currentPage - 1) : null,
                TotalEntries = totalEntries,
                TotalPages = totalPages
            }
        };
    }

    // ─── Mapping ────────────────────────────────────────────────────────────────

    /// <summary>
    /// يحوّل كائن User إلى CommunityPostUserDto.
    /// </summary>
    private static CommunityPostUserDto MapToUserDto(User user) => new()
    {
        UserId          = user.Id,
        FullName        = !string.IsNullOrWhiteSpace(user.FullName)
                            ? user.FullName
                            : user.Email ?? user.UserName ?? "User",
        ProfileImageUrl = user.ProfileImageUrl
    };

    /// <summary>
    /// يحوّل كائن PostMedia إلى CommunityPostMediaResponseDto.
    /// </summary>
    private static CommunityPostMediaResponseDto MapToMediaDto(PostMedia media) => new()
    {
        MediaId   = media.MediaId,
        PostId    = media.PostId,
        MediaUrl  = media.MediaUrl,
        MediaType = media.MediaType
    };

    /// <summary>
    /// يحوّل كائن CommunityPost إلى CommunityPostResponseDto.
    /// </summary>
    private static CommunityPostResponseDto MapToResponse(CommunityPost post) => new()
    {
        PostId      = post.PostId,
        ContentText = post.ContentText,
        CreatedAt   = post.CreatedAt,
        User        = MapToUserDto(post.User),
        Media       = post.PostMedia.OrderBy(m => m.MediaId).Select(MapToMediaDto).ToList()
    };

    // ─── Delete Post Files Helper ────────────────────────────────────────────────

    /// <summary>
    /// يحذف جميع ملفات الميديا الفعلية المرتبطة بمنشور.
    /// </summary>
    private async Task DeletePostFilesAsync(CommunityPost post)
    {
        foreach (var media in post.PostMedia)
            await _fileStorageService.DeleteFileAsync(media.MediaUrl);
    }

    // ─── Public Methods ──────────────────────────────────────────────────────────

    /// <summary>
    /// يجلب الفيد العام للمنشورات المجتمعية مع دعم البحث والتقسيم إلى صفحات.
    /// مرتب من الأحدث إلى الأقدم.
    /// </summary>
    public async Task<ApiResponse<PaginatedResponse<CommunityPostResponseDto>>> GetAllAsync(CommunityPostQueryDto query)
    {
        var (pageNumber, pageSize) = NormalizePagination(query.PageNumber, query.PageSize);

        var dbQuery = _context.Set<CommunityPost>()
            .Include(p => p.User)
            .Include(p => p.PostMedia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var lowerSearch = query.Search.ToLower();
            dbQuery = dbQuery.Where(p => p.ContentText.ToLower().Contains(lowerSearch));
        }

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToResponse).ToList();

        return ApiResponse<PaginatedResponse<CommunityPostResponseDto>>.Ok(
            BuildPaginatedResponse(dtos, totalItems, pageNumber, pageSize));
    }

    /// <summary>
    /// ينشئ منشورًا مجتمعيًا جديدًا مع إمكانية رفع صور.
    /// رقم المستخدم يُؤخذ من JWT. الحد الأقصى 5 صور.
    /// يستخدم Transaction لضمان اتساق البيانات.
    /// </summary>
    public async Task<ApiResponse<CommunityPostResponseDto>> CreateWithMediaAsync(CreateCommunityPostWithMediaDto dto)
    {
        if (dto.Files != null && dto.Files.Count > MaxImagesPerPost)
            return ApiResponse<CommunityPostResponseDto>.Fail($"Maximum {MaxImagesPerPost} images allowed per post.");

        var currentUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        using var transaction = await _context.Database.BeginTransactionAsync();
        var savedFileUrls = new List<string>();

        try
        {
            var post = new CommunityPost
            {
                UserId      = currentUserId,
                ContentText = dto.ContentText,
                CreatedAt   = now,
                UpdatedAt   = now
            };

            _context.Set<CommunityPost>().Add(post);
            await _context.SaveChangesAsync(); // Get PostId before file upload

            if (dto.Files != null && dto.Files.Count > 0)
            {
                foreach (var file in dto.Files)
                {
                    var fileUrl = await _fileStorageService.SavePostMediaAsync(post.PostId, file);
                    savedFileUrls.Add(fileUrl);

                    _context.Set<PostMedia>().Add(new PostMedia
                    {
                        PostId    = post.PostId,
                        MediaUrl  = fileUrl,
                        MediaType = MediaType.Image,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Reload with User and PostMedia for response
            var created = await _context.Set<CommunityPost>()
                .Include(p => p.User)
                .Include(p => p.PostMedia)
                .FirstAsync(p => p.PostId == post.PostId);

            return ApiResponse<CommunityPostResponseDto>.Ok(MapToResponse(created), "Post created successfully.", 201);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            foreach (var url in savedFileUrls)
                await _fileStorageService.DeleteFileAsync(url);

            return ApiResponse<CommunityPostResponseDto>.Fail(
                "An error occurred while creating the post. The operation has been rolled back.", null, 500);
        }
    }

    /// <summary>
    /// يحدّث نص منشور موجود.
    /// يُسمح فقط لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    public async Task<ApiResponse<CommunityPostResponseDto>> UpdateAsync(int id, UpdateCommunityPostDto dto)
    {
        var post = await _context.Set<CommunityPost>()
            .Include(p => p.User)
            .Include(p => p.PostMedia)
            .FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null)
            return ApiResponse<CommunityPostResponseDto>.Fail("Post not found.", null, 404);

        if (!await CanManagePostAsync(post))
            return ApiResponse<CommunityPostResponseDto>.Fail("You are not authorized to update this post.", null, 403);

        post.ContentText = dto.ContentText;
        post.UpdatedAt   = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<CommunityPostResponseDto>.Ok(MapToResponse(post));
    }

    /// <summary>
    /// يحذف منشورًا مجتمعيًا وجميع ملفات ميديا والبلاغات المرتبطة به.
    /// يُسمح فقط لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var post = await _context.Set<CommunityPost>()
            .Include(p => p.PostMedia)
            .Include(p => p.Reports)
            .FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null)
            return ApiResponse<bool>.Fail("Post not found.", null, 404);

        if (!await CanManagePostAsync(post))
            return ApiResponse<bool>.Fail("You are not authorized to delete this post.", null, 403);

        var mediaUrls = post.PostMedia.Select(m => m.MediaUrl).ToList();

        _context.Set<CommunityPost>().Remove(post);
        await _context.SaveChangesAsync();

        foreach (var url in mediaUrls)
            await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Post deleted successfully.");
    }

    // ─── Media Management ────────────────────────────────────────────────────────

    /// <summary>
    /// يضيف ملف ميديا واحدًا لمنشور موجود.
    /// يُسمح فقط لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// لا يُسمح إذا كان المنشور يحتوي بالفعل على الحد الأقصى من الصور.
    /// </summary>
    public async Task<ApiResponse<CommunityPostMediaResponseDto>> AddMediaAsync(int postId, AddCommunityPostMediaDto dto)
    {
        var post = await _context.Set<CommunityPost>()
            .Include(p => p.PostMedia)
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null)
            return ApiResponse<CommunityPostMediaResponseDto>.Fail("Post not found.", null, 404);

        if (!await CanManagePostAsync(post))
            return ApiResponse<CommunityPostMediaResponseDto>.Fail("You are not authorized to manage media for this post.", null, 403);

        if (post.PostMedia.Count >= MaxImagesPerPost)
            return ApiResponse<CommunityPostMediaResponseDto>.Fail($"Post already has the maximum of {MaxImagesPerPost} images.");

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SavePostMediaAsync(postId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<CommunityPostMediaResponseDto>.Fail(ex.Message);
        }

        var media = new PostMedia
        {
            PostId    = postId,
            MediaUrl  = savedFileUrl,
            MediaType = MediaType.Image,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Set<PostMedia>().Add(media);
            await _context.SaveChangesAsync();

            return ApiResponse<CommunityPostMediaResponseDto>.Ok(MapToMediaDto(media), "Media uploaded successfully.", 201);
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<CommunityPostMediaResponseDto>.Fail("Failed to save media to database.", null, 500);
        }
    }

    /// <summary>
    /// يستبدل ملف ميديا موجود بملف جديد.
    /// يُسمح فقط لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// يحذف الملف القديم فقط بعد نجاح تحديث قاعدة البيانات.
    /// </summary>
    public async Task<ApiResponse<CommunityPostMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateCommunityPostMediaDto dto)
    {
        var media = await _context.Set<PostMedia>()
            .Include(m => m.Post)
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media == null)
            return ApiResponse<CommunityPostMediaResponseDto>.Fail("Media not found.", null, 404);

        if (!await CanManagePostAsync(media.Post))
            return ApiResponse<CommunityPostMediaResponseDto>.Fail("You are not authorized to manage media for this post.", null, 403);

        var oldUrl = media.MediaUrl;

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SavePostMediaAsync(media.PostId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<CommunityPostMediaResponseDto>.Fail(ex.Message);
        }

        media.MediaUrl = savedFileUrl;

        try
        {
            await _context.SaveChangesAsync();
            await _fileStorageService.DeleteFileAsync(oldUrl);

            return ApiResponse<CommunityPostMediaResponseDto>.Ok(MapToMediaDto(media), "Media updated successfully.");
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<CommunityPostMediaResponseDto>.Fail("Failed to update media in database.", null, 500);
        }
    }

    /// <summary>
    /// يحذف ملف ميديا من المنشور من قاعدة البيانات ومن التخزين.
    /// يُسمح فقط لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId)
    {
        var media = await _context.Set<PostMedia>()
            .Include(m => m.Post)
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media == null)
            return ApiResponse<bool>.Fail("Media not found.", null, 404);

        if (!await CanManagePostAsync(media.Post))
            return ApiResponse<bool>.Fail("You are not authorized to manage media for this post.", null, 403);

        var url = media.MediaUrl;
        _context.Set<PostMedia>().Remove(media);
        await _context.SaveChangesAsync();

        await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Media deleted successfully.");
    }

    // ─── Reporting ────────────────────────────────────────────────────────────────

    /// <summary>
    /// يُرسل بلاغًا على منشور مجتمعي.
    /// رقم المستخدم يُؤخذ من JWT. لا يمكن الإبلاغ على نفس المنشور مرتين.
    /// لا يمكن المستخدم الإبلاغ عن منشوره الخاص.
    /// </summary>
    public async Task<ApiResponse<ReportResponseDto>> ReportAsync(int postId, CreateReportDto dto)
    {
        var currentUserId = GetCurrentUserId();

        var post = await _context.Set<CommunityPost>()
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null)
            return ApiResponse<ReportResponseDto>.Fail("Post not found.", null, 404);

        // Prevent reporting own post
        if (post.UserId == currentUserId)
            return ApiResponse<ReportResponseDto>.Fail("You cannot report your own post.");

        // Check for duplicate report
        var alreadyReported = await _context.Set<Report>()
            .AnyAsync(r => r.UserId == currentUserId && r.PostId == postId);

        if (alreadyReported)
            return ApiResponse<ReportResponseDto>.Fail("You have already reported this post.");

        var report = new Report
        {
            UserId    = currentUserId,
            PostId    = postId,
            Reason    = dto.Reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Report>().Add(report);
        await _context.SaveChangesAsync();

        return ApiResponse<ReportResponseDto>.Ok(new ReportResponseDto
        {
            ReportId  = report.ReportId,
            PostId    = report.PostId,
            Reason    = report.Reason,
            CreatedAt = report.CreatedAt
        }, "Report submitted successfully.", 201);
    }
}
