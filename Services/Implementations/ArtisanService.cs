using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.Artisans;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// تنفيذ سيرفس إدارة الحرفيين وميديا الحرفيين.
/// يوفر منطق الأعمال لجميع عمليات الحرفيين وميديا الحرفيين.
/// </summary>
public class ArtisanService : IArtisanService
{
    private readonly DbContextApp _context;
    private readonly IFileStorageService _fileStorageService;

    public ArtisanService(DbContextApp context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

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
    /// يبني استعلام IQueryable مُضمَّن بالبحث على حقول النص للحرفيين.
    /// يستخدم Contains مباشرةً لضمان الترجمة إلى SQL بدون تقييم من جانب العميل.
    /// </summary>
    private IQueryable<Artisan> BuildFilteredQuery(string? search)
    {
        var dbQuery = _context.Set<Artisan>()
            .Include(a => a.ArtisanMedia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            dbQuery = dbQuery.Where(a =>
                a.NameAr.ToLower().Contains(lowerSearch) ||
                a.NameEn.ToLower().Contains(lowerSearch) ||
                a.DescriptionAr.ToLower().Contains(lowerSearch) ||
                a.DescriptionEn.ToLower().Contains(lowerSearch) ||
                (a.LocationTextAr != null && a.LocationTextAr.ToLower().Contains(lowerSearch)) ||
                (a.LocationTextEn != null && a.LocationTextEn.ToLower().Contains(lowerSearch)));
        }

        return dbQuery;
    }

    /// <summary>
    /// يبني PaginatedResponse من قائمة الـ DTOs وبيانات التقسيم.
    /// </summary>
    private static PaginatedResponse<ArtisanResponseDto> BuildPaginatedResponse(
        List<ArtisanResponseDto> dtos, int totalEntries, int currentPage, int perPage)
    {
        var totalPages = perPage > 0 ? (int)Math.Ceiling((double)totalEntries / perPage) : 0;
        return new PaginatedResponse<ArtisanResponseDto>
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
    /// يحوّل كائن Artisan إلى ArtisanResponseDto مختصر للقوائم.
    /// صورة الغلاف تُؤخذ من ArtisanMedia حيث IsCover = true.
    /// </summary>
    private static ArtisanResponseDto MapToResponse(Artisan artisan) => new()
    {
        ArtisanId     = artisan.ArtisanId,
        NameAr        = artisan.NameAr,
        NameEn        = artisan.NameEn,
        CoverImageUrl = artisan.ArtisanMedia.FirstOrDefault(m => m.IsCover)?.MediaUrl
    };

    /// <summary>
    /// يحوّل كائن Artisan إلى ArtisanDetailsDto كامل.
    /// </summary>
    private static ArtisanDetailsDto MapToDetails(Artisan artisan) => new()
    {
        ArtisanId     = artisan.ArtisanId,
        NameAr        = artisan.NameAr,
        NameEn        = artisan.NameEn,
        Phone         = artisan.Phone,
        DescriptionAr = artisan.DescriptionAr,
        DescriptionEn = artisan.DescriptionEn,
        LocationTextAr = artisan.LocationTextAr,
        LocationTextEn = artisan.LocationTextEn,
        MapUrl        = artisan.MapUrl,
        CoverImageUrl = artisan.ArtisanMedia.FirstOrDefault(m => m.IsCover)?.MediaUrl,
        Media         = artisan.ArtisanMedia.Select(MapToMediaResponse).ToList()
    };

    /// <summary>
    /// يحوّل كائن ArtisanMedia إلى ArtisanMediaResponseDto.
    /// </summary>
    private static ArtisanMediaResponseDto MapToMediaResponse(ArtisanMedia media) => new()
    {
        MediaId   = media.MediaId,
        ArtisanId = media.ArtisanId,
        MediaUrl  = media.MediaUrl,
        MediaType = media.MediaType,
        IsCover   = media.IsCover
    };

    // ─── Public Methods ──────────────────────────────────────────────────────────

    /// <summary>
    /// يجلب قائمة الحرفيين مع دعم البحث والتقسيم إلى صفحات.
    /// يرتب النتائج من الأحدث إلى الأقدم.
    /// </summary>
    public async Task<ApiResponse<PaginatedResponse<ArtisanResponseDto>>> GetAllAsync(ArtisanQueryDto query)
    {
        var (pageNumber, pageSize) = NormalizePagination(query.PageNumber, query.PageSize);

        var dbQuery = BuildFilteredQuery(query.Search);

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToResponse).ToList();

        return ApiResponse<PaginatedResponse<ArtisanResponseDto>>.Ok(
            BuildPaginatedResponse(dtos, totalItems, pageNumber, pageSize));
    }

    /// <summary>
    /// يجلب التفاصيل الكاملة لحرفي واحد حسب رقمه.
    /// </summary>
    public async Task<ApiResponse<ArtisanDetailsDto>> GetByIdAsync(int id)
    {
        var artisan = await _context.Set<Artisan>()
            .Include(a => a.ArtisanMedia)
            .FirstOrDefaultAsync(a => a.ArtisanId == id);

        if (artisan == null)
            return ApiResponse<ArtisanDetailsDto>.Fail("Artisan not found.", null, 404);

        return ApiResponse<ArtisanDetailsDto>.Ok(MapToDetails(artisan));
    }

    /// <summary>
    /// ينشئ حرفيًا جديدًا مع رفع ملفات الميديا في نفس الطلب.
    /// أول ملف يُعيَّن صورة غلاف (IsCover = true).
    /// يستخدم Transaction لضمان اتساق البيانات.
    /// في حالة الفشل يتم التراجع عن العملية وحذف الملفات المرفوعة.
    /// </summary>
    public async Task<ApiResponse<ArtisanDetailsDto>> CreateWithMediaAsync(CreateArtisanWithMediaDto dto)
    {
        if (dto.Files != null && dto.Files.Count > 10)
            return ApiResponse<ArtisanDetailsDto>.Fail("Maximum 10 files allowed.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        var savedFileUrls = new List<string>();

        try
        {
            var now = DateTime.UtcNow;
            var artisan = new Artisan
            {
                NameAr        = dto.NameAr,
                NameEn        = dto.NameEn,
                Phone         = dto.Phone,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                LocationTextAr = dto.LocationTextAr,
                LocationTextEn = dto.LocationTextEn,
                MapUrl        = dto.MapUrl,
                CreatedAt     = now,
                UpdatedAt     = now
            };

            _context.Set<Artisan>().Add(artisan);
            await _context.SaveChangesAsync(); // Get ArtisanId before file upload

            if (dto.Files != null && dto.Files.Count > 0)
            {
                bool isFirstFile = true;
                foreach (var file in dto.Files)
                {
                    var fileUrl = await _fileStorageService.SaveArtisanMediaAsync(artisan.ArtisanId, file);
                    savedFileUrls.Add(fileUrl);

                    _context.Set<ArtisanMedia>().Add(new ArtisanMedia
                    {
                        ArtisanId = artisan.ArtisanId,
                        MediaUrl  = fileUrl,
                        MediaType = MediaType.Image,
                        IsCover   = isFirstFile, // First file becomes cover
                        CreatedAt = DateTime.UtcNow
                    });

                    isFirstFile = false;
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return await GetByIdAsync(artisan.ArtisanId);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            foreach (var url in savedFileUrls)
                await _fileStorageService.DeleteFileAsync(url);

            return ApiResponse<ArtisanDetailsDto>.Fail(
                "An error occurred while creating the artisan. The operation has been rolled back.", null, 500);
        }
    }

    /// <summary>
    /// يحدّث البيانات النصية لحرفي موجود (بدون ميديا).
    /// </summary>
    public async Task<ApiResponse<ArtisanDetailsDto>> UpdateAsync(int id, UpdateArtisanDto dto)
    {
        var artisan = await _context.Set<Artisan>()
            .FirstOrDefaultAsync(a => a.ArtisanId == id);

        if (artisan == null)
            return ApiResponse<ArtisanDetailsDto>.Fail("Artisan not found.", null, 404);

        artisan.NameAr        = dto.NameAr;
        artisan.NameEn        = dto.NameEn;
        artisan.Phone         = dto.Phone;
        artisan.DescriptionAr = dto.DescriptionAr;
        artisan.DescriptionEn = dto.DescriptionEn;
        artisan.LocationTextAr = dto.LocationTextAr;
        artisan.LocationTextEn = dto.LocationTextEn;
        artisan.MapUrl        = dto.MapUrl;
        artisan.UpdatedAt     = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(artisan.ArtisanId);
    }

    /// <summary>
    /// يحذف حرفيًا موجودًا وجميع ملفات الميديا المرتبطة به.
    /// يحذف الملفات الفعلية من التخزين بعد نجاح حذف قاعدة البيانات.
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var artisan = await _context.Set<Artisan>()
            .Include(a => a.ArtisanMedia)
            .FirstOrDefaultAsync(a => a.ArtisanId == id);

        if (artisan == null)
            return ApiResponse<bool>.Fail("Artisan not found.", null, 404);

        var mediaUrls = artisan.ArtisanMedia.Select(m => m.MediaUrl).ToList();

        _context.Set<Artisan>().Remove(artisan);
        await _context.SaveChangesAsync();

        foreach (var url in mediaUrls)
            await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Artisan deleted successfully.");
    }

    // ─── Media Management ────────────────────────────────────────────────────────

    /// <summary>
    /// يضيف ملف ميديا واحدًا لحرفي موجود.
    /// إذا لم يكن للحرفي صورة غلاف، يُعيَّن الملف الجديد صورة غلاف.
    /// في حالة فشل حفظ قاعدة البيانات يتم حذف الملف المرفوع.
    /// </summary>
    public async Task<ApiResponse<ArtisanMediaResponseDto>> AddMediaAsync(int artisanId, AddArtisanMediaDto dto)
    {
        var artisan = await _context.Set<Artisan>()
            .Include(a => a.ArtisanMedia)
            .FirstOrDefaultAsync(a => a.ArtisanId == artisanId);

        if (artisan == null)
            return ApiResponse<ArtisanMediaResponseDto>.Fail("Artisan not found.", null, 404);

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SaveArtisanMediaAsync(artisanId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<ArtisanMediaResponseDto>.Fail(ex.Message);
        }

        // New media becomes cover only if artisan has no existing cover
        var hasCover = artisan.ArtisanMedia.Any(m => m.IsCover);

        var media = new ArtisanMedia
        {
            ArtisanId = artisanId,
            MediaUrl  = savedFileUrl,
            MediaType = MediaType.Image,
            IsCover   = !hasCover,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Set<ArtisanMedia>().Add(media);
            await _context.SaveChangesAsync();

            return ApiResponse<ArtisanMediaResponseDto>.Ok(MapToMediaResponse(media), "Media uploaded successfully.", 201);
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<ArtisanMediaResponseDto>.Fail("Failed to save media to database.", null, 500);
        }
    }

    /// <summary>
    /// يستبدل ملف ميديا موجود بملف جديد مع الحفاظ على قيمة IsCover.
    /// يحذف الملف القديم فقط بعد نجاح تحديث قاعدة البيانات.
    /// في حالة فشل الحفظ يحذف الملف الجديد.
    /// </summary>
    public async Task<ApiResponse<ArtisanMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateArtisanMediaDto dto)
    {
        var media = await _context.Set<ArtisanMedia>()
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media == null)
            return ApiResponse<ArtisanMediaResponseDto>.Fail("Media not found.", null, 404);

        var oldUrl = media.MediaUrl;

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SaveArtisanMediaAsync(media.ArtisanId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<ArtisanMediaResponseDto>.Fail(ex.Message);
        }

        media.MediaUrl = savedFileUrl;
        // IsCover is intentionally NOT changed here — preserved from existing record

        try
        {
            await _context.SaveChangesAsync();
            await _fileStorageService.DeleteFileAsync(oldUrl);

            return ApiResponse<ArtisanMediaResponseDto>.Ok(MapToMediaResponse(media), "Media updated successfully.");
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<ArtisanMediaResponseDto>.Fail("Failed to update media in database.", null, 500);
        }
    }

    /// <summary>
    /// يحذف ملف ميديا من قاعدة البيانات ومن التخزين.
    /// إذا كانت الميديا المحذوفة هي صورة الغلاف، يُرقَّى ملف ميديا آخر للحرفي ليصبح الغلاف.
    /// إذا لم يتبقَّ ميديا أخرى، لا يبقى غلاف.
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId)
    {
        var media = await _context.Set<ArtisanMedia>()
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media == null)
            return ApiResponse<bool>.Fail("Media not found.", null, 404);

        var wascover = media.IsCover;
        var artisanId = media.ArtisanId;
        var url = media.MediaUrl;

        _context.Set<ArtisanMedia>().Remove(media);
        await _context.SaveChangesAsync();

        // Promote another media to cover if the deleted one was the cover
        if (wascover)
        {
            var nextMedia = await _context.Set<ArtisanMedia>()
                .Where(m => m.ArtisanId == artisanId)
                .OrderBy(m => m.MediaId)
                .FirstOrDefaultAsync();

            if (nextMedia != null)
            {
                nextMedia.IsCover = true;
                await _context.SaveChangesAsync();
            }
        }

        await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Media deleted successfully.");
    }

    /// <summary>
    /// يُعيِّن ملف ميديا محدد كصورة غلاف للحرفي.
    /// يُعيد تعيين IsCover = false لجميع ميديا الحرفي أولًا، ثم يُعيِّن الميديا المختارة كغلاف.
    /// </summary>
    public async Task<ApiResponse<ArtisanMediaResponseDto>> SetCoverAsync(int mediaId)
    {
        var media = await _context.Set<ArtisanMedia>()
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media == null)
            return ApiResponse<ArtisanMediaResponseDto>.Fail("Media not found.", null, 404);

        // Reset all media for the same artisan to IsCover = false
        var allMedia = await _context.Set<ArtisanMedia>()
            .Where(m => m.ArtisanId == media.ArtisanId)
            .ToListAsync();

        foreach (var m in allMedia)
            m.IsCover = false;

        // Set selected media as cover
        media.IsCover = true;

        await _context.SaveChangesAsync();

        return ApiResponse<ArtisanMediaResponseDto>.Ok(MapToMediaResponse(media), "Cover image set successfully.");
    }
}
