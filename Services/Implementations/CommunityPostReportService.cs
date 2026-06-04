using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.CommunityPostReports;
using Had360back.DTOs.CommunityPosts;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// تنفيذ سيرفس إدارة بلاغات المنشورات المجتمعية.
/// مخصص للأدمن ومدير المحتوى فقط.
/// </summary>
public class CommunityPostReportService : ICommunityPostReportService
{
    private readonly DbContextApp _context;
    private readonly IFileStorageService _fileStorageService;

    public CommunityPostReportService(DbContextApp context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// يُطبّق منطق تطبيع قيم الصفحة.
    /// </summary>
    private static (int pageNumber, int pageSize) NormalizePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;
        return (pageNumber, pageSize);
    }

    /// <summary>
    /// يبني PaginatedResponse من قائمة تقارير البلاغات.
    /// </summary>
    private static PaginatedResponse<CommunityPostReportResponseDto> BuildPaginatedResponse(
        List<CommunityPostReportResponseDto> dtos, int totalEntries, int currentPage, int perPage)
    {
        var totalPages = perPage > 0 ? (int)Math.Ceiling((double)totalEntries / perPage) : 0;
        return new PaginatedResponse<CommunityPostReportResponseDto>
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
        UserName        = user.UserName,
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
    /// يحوّل كائن Report إلى CommunityPostReportResponseDto كامل للأدمن.
    /// </summary>
    private static CommunityPostReportResponseDto MapToReportResponse(Report report) => new()
    {
        ReportId  = report.ReportId,
        Reason    = report.Reason,
        CreatedAt = report.CreatedAt,
        ReportedBy = new ReportedByUserDto
        {
            UserId          = report.User.Id,
            UserName        = report.User.UserName,
            ProfileImageUrl = report.User.ProfileImageUrl
        },
        ReportedPost = new ReportedPostDto
        {
            PostId      = report.Post.PostId,
            ContentText = report.Post.ContentText,
            CreatedAt   = report.Post.CreatedAt,
            User        = MapToUserDto(report.Post.User),
            Media       = report.Post.PostMedia.OrderBy(m => m.MediaId).Select(MapToMediaDto).ToList()
        }
    };

    // ─── Public Methods ──────────────────────────────────────────────────────────

    /// <summary>
    /// يجلب قائمة مقسّمة إلى صفحات من بلاغات المنشورات المجتمعية.
    /// مرتبة من الأحدث إلى الأقدم.
    /// يتضمن كل بلاغ: بيانات البلاغ، المُبلِّغ، والمنشور المُبلَّغ عنه كاملًا.
    /// </summary>
    public async Task<ApiResponse<PaginatedResponse<CommunityPostReportResponseDto>>> GetAllReportsAsync(
        CommunityPostReportQueryDto query)
    {
        var (pageNumber, pageSize) = NormalizePagination(query.PageNumber, query.PageSize);

        var dbQuery = _context.Set<Report>()
            .Include(r => r.User)
            .Include(r => r.Post)
                .ThenInclude(p => p.User)
            .Include(r => r.Post)
                .ThenInclude(p => p.PostMedia)
            .AsQueryable();

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToReportResponse).ToList();

        return ApiResponse<PaginatedResponse<CommunityPostReportResponseDto>>.Ok(
            BuildPaginatedResponse(dtos, totalItems, pageNumber, pageSize));
    }

    /// <summary>
    /// يرفض بلاغًا (يحذفه فقط) دون حذف المنشور.
    /// </summary>
    public async Task<ApiResponse<bool>> DismissReportAsync(int reportId)
    {
        var report = await _context.Set<Report>()
            .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null)
            return ApiResponse<bool>.Fail("Report not found.", null, 404);

        _context.Set<Report>().Remove(report);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Report dismissed successfully.");
    }

    /// <summary>
    /// يحذف المنشور المُبلَّغ عنه مع جميع ميديا وبلاغاته وملفاته الفعلية.
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteReportedPostAsync(int reportId)
    {
        var report = await _context.Set<Report>()
            .Include(r => r.Post)
                .ThenInclude(p => p.PostMedia)
            .Include(r => r.Post)
                .ThenInclude(p => p.Reports)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null)
            return ApiResponse<bool>.Fail("Report not found.", null, 404);

        if (report.Post == null)
            return ApiResponse<bool>.Fail("Related post not found.", null, 404);

        var mediaUrls = report.Post.PostMedia.Select(m => m.MediaUrl).ToList();

        _context.Set<CommunityPost>().Remove(report.Post);
        await _context.SaveChangesAsync();

        foreach (var url in mediaUrls)
            await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Post and all related data deleted successfully.");
    }
}
