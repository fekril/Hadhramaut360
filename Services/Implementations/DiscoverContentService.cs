using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.DiscoverContent;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

public class DiscoverContentService : IDiscoverContentService
{
    private readonly DbContextApp _context;
    private readonly IFileStorageService _fileStorageService;

    public DiscoverContentService(DbContextApp context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف مع دعم البحث والتقسيم إلى صفحات.
    /// </summary>
    /// <param name="query">
    /// يحتوي على كلمة البحث الاختيارية، رقم الصفحة، وحجم الصفحة.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على قائمة DiscoverContentResponseDto مقسّمة إلى صفحات.
    /// </returns>
    /// <remarks>
    /// يبحث في title_ar وtitle_en وbody_ar وbody_en.
    /// يرتب النتائج من الأحدث إلى الأقدم حسب CreatedAt.
    /// </remarks>
    public async Task<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>> GetAllAsync(
        DiscoverContentQueryDto query)
    {
        var dbQuery = _context.Set<DiscoverContent>().AsQueryable();
        return await BuildPagedResponseAsync(dbQuery, query);
    }

    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف المرتبطة بقسم معين بناءً على قائمة الكلمات المفتاحية.
    /// </summary>
    /// <param name="query">
    /// يحتوي على كلمة البحث الاختيارية، رقم الصفحة، وحجم الصفحة.
    /// </param>
    /// <param name="keywords">
    /// قائمة الكلمات المفتاحية الخاصة بالقسم (ثقافة، أكل، ألعاب...).
    /// يتم البحث عن أي كلمة مفتاحية في title_ar أو title_en أو body_ar أو body_en.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على قائمة DiscoverContentResponseDto المرتبطة بالقسم مع التقسيم إلى صفحات.
    /// </returns>
    /// <remarks>
    /// لإضافة قسم جديد يكفي تمرير قائمة كلمات مفتاحية مختلفة دون أي تغيير في المنطق الداخلي.
    /// </remarks>
    public async Task<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>> GetBySectionAsync(
        DiscoverContentQueryDto query,
        IReadOnlyList<string> keywords)
    {
        var dbQuery = _context.Set<DiscoverContent>().AsQueryable();

        // Pre-filter using an EF Core-translatable expression tree.
        // keywords.Any(...) cannot be translated to SQL, so we build the OR predicate
        // manually using Expression trees — one Contains check per keyword per field.
        var keywordPredicate = BuildKeywordPredicate(keywords);
        if (keywordPredicate != null)
            dbQuery = dbQuery.Where(keywordPredicate);

        return await BuildPagedResponseAsync(dbQuery, query);
    }

    /// <summary>
    /// يجلب تفاصيل عنصر محتوى اكتشاف واحد حسب رقمه.
    /// </summary>
    /// <param name="id">رقم عنصر المحتوى المطلوب.</param>
    /// <returns>
    /// ApiResponse يحتوي على DiscoverContentDetailsDto عند العثور على العنصر.
    /// يرجع خطأ 404 إذا لم يتم العثور على العنصر.
    /// </returns>
    public async Task<ApiResponse<DiscoverContentDetailsDto>> GetByIdAsync(int id)
    {
        var content = await _context.Set<DiscoverContent>()
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (content == null)
            return ApiResponse<DiscoverContentDetailsDto>.Fail("Discover content not found.", null, 404);

        return ApiResponse<DiscoverContentDetailsDto>.Ok(MapToDetails(content));
    }

    /// <summary>
    /// ينشئ عنصر محتوى اكتشاف جديدًا.
    /// إذا أُرسل ملف صورة غلاف، يتم رفعه وربطه بالعنصر بعد الحصول على رقم العنصر من قاعدة البيانات.
    /// </summary>
    /// <param name="dto">بيانات العنصر الجديد بصيغة multipart/form-data.</param>
    /// <returns>
    /// ApiResponse يحتوي على تفاصيل العنصر بعد إنشائه برمز 201.
    /// يرجع خطأ 400 إذا فشل التحقق من صحة ملف الصورة.
    /// </returns>
    /// <remarks>
    /// تسلسل العملية:
    /// 1. إنشاء عنصر المحتوى وحفظه للحصول على ContentId.
    /// 2. رفع صورة الغلاف إذا وُجدت وحفظ رابطها النسبي.
    /// 3. SaveChanges الثانية لحفظ رابط الصورة.
    /// في حالة فشل رفع الصورة، يتم حذف عنصر المحتوى والتراجع عن العملية.
    /// </remarks>
    public async Task<ApiResponse<DiscoverContentDetailsDto>> CreateAsync(CreateDiscoverContentDto dto)
    {
        var content = new DiscoverContent
        {
            TitleAr   = dto.TitleAr,
            TitleEn   = dto.TitleEn,
            BodyAr    = dto.BodyAr,
            BodyEn    = dto.BodyEn,
           
        };

        _context.Set<DiscoverContent>().Add(content);
        await _context.SaveChangesAsync(); // Persist to obtain ContentId

        if (dto.CoverImage != null)
        {
            string savedUrl;
            try
            {
                savedUrl = await _fileStorageService.SaveDiscoverContentCoverAsync(content.ContentId, dto.CoverImage);
            }
            catch (Exception ex)
            {
                // Rollback: remove the already-saved entity
                _context.Set<DiscoverContent>().Remove(content);
                await _context.SaveChangesAsync();
                return ApiResponse<DiscoverContentDetailsDto>.Fail($"Failed to upload cover image: {ex.Message}", null, 400);
            }

            content.CoverImageUrl = savedUrl;
            await _context.SaveChangesAsync(); // Persist image URL
        }

        return ApiResponse<DiscoverContentDetailsDto>.Ok(MapToDetails(content), "Discover content created successfully.", 201);
    }

    /// <summary>
    /// يحدث عنصر محتوى اكتشاف موجودًا.
    /// إذا أُرسل ملف صورة غلاف جديد، يستبدل القديم ويحذف الملف القديم من التخزين.
    /// إذا لم يُرسل ملف، تُحافظ العملية على صورة الغلاف الحالية.
    /// </summary>
    /// <param name="id">رقم العنصر المراد تعديله.</param>
    /// <param name="dto">البيانات الجديدة للعنصر بصيغة multipart/form-data.</param>
    /// <returns>
    /// ApiResponse يحتوي على تفاصيل العنصر بعد التحديث.
    /// يرجع خطأ 404 إذا لم يتم العثور على العنصر.
    /// يرجع خطأ 400 إذا فشل التحقق من صحة ملف الصورة الجديد.
    /// </returns>
    /// <remarks>
    /// عند رفع صورة جديدة:
    /// 1. رفع الملف الجديد.
    /// 2. تحديث رابط الصورة في قاعدة البيانات.
    /// 3. SaveChanges.
    /// 4. حذف الملف القديم من التخزين بعد نجاح الحفظ.
    /// عند فشل SaveChanges يتم حذف الملف الجديد لتجنب ملفات يتيمة.
    /// </remarks>
    public async Task<ApiResponse<DiscoverContentDetailsDto>> UpdateAsync(int id, UpdateDiscoverContentDto dto)
    {
        var content = await _context.Set<DiscoverContent>()
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (content == null)
            return ApiResponse<DiscoverContentDetailsDto>.Fail("Discover content not found.", null, 404);

        content.TitleAr   = dto.TitleAr;
        content.TitleEn   = dto.TitleEn;
        content.BodyAr    = dto.BodyAr;
        content.BodyEn    = dto.BodyEn;
        content.UpdatedAt = DateTime.UtcNow;

        var oldCoverUrl  = content.CoverImageUrl;
        string? newCoverUrl = null;

        if (dto.CoverImage != null)
        {
            try
            {
                newCoverUrl = await _fileStorageService.SaveDiscoverContentCoverAsync(content.ContentId, dto.CoverImage);
            }
            catch (Exception ex)
            {
                return ApiResponse<DiscoverContentDetailsDto>.Fail($"Failed to upload cover image: {ex.Message}", null, 400);
            }

            content.CoverImageUrl = newCoverUrl;
        }

        try
        {
            await _context.SaveChangesAsync();

            // Delete old physical file only after DB is successfully updated
            if (dto.CoverImage != null && !string.IsNullOrEmpty(oldCoverUrl))
            {
                await _fileStorageService.DeleteFileAsync(oldCoverUrl);
            }
        }
        catch (Exception)
        {
            // If DB save fails, delete the newly uploaded file to avoid orphaned files
            if (!string.IsNullOrEmpty(newCoverUrl))
            {
                await _fileStorageService.DeleteFileAsync(newCoverUrl);
            }
            return ApiResponse<DiscoverContentDetailsDto>.Fail("An error occurred while saving the updates.", null, 500);
        }

        return ApiResponse<DiscoverContentDetailsDto>.Ok(MapToDetails(content), "Discover content updated successfully.");
    }

    /// <summary>
    /// يحذف عنصر محتوى اكتشاف موجودًا وملف صورة الغلاف المرتبط به من التخزين.
    /// </summary>
    /// <param name="id">رقم العنصر المراد حذفه.</param>
    /// <returns>
    /// ApiResponse يحتوي على true عند نجاح الحذف.
    /// يرجع خطأ 404 إذا لم يتم العثور على العنصر.
    /// </returns>
    /// <remarks>
    /// يتم حفظ رابط الصورة أولًا، ثم حذف السجل من قاعدة البيانات،
    /// ثم حذف الملف الفعلي من التخزين — بنفس نمط LandmarkService.DeleteAsync.
    /// </remarks>
    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var content = await _context.Set<DiscoverContent>()
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (content == null)
            return ApiResponse<bool>.Fail("Discover content not found.", null, 404);

        var coverUrl = content.CoverImageUrl;

        _context.Set<DiscoverContent>().Remove(content);
        await _context.SaveChangesAsync();

        // Delete physical file after successful DB removal
        await _fileStorageService.DeleteFileAsync(coverUrl);

        return ApiResponse<bool>.Ok(true, "Discover content deleted successfully.");
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// منطق البحث والتقسيم إلى صفحات المشترك بين GetAllAsync وGetBySectionAsync.
    /// يطبّق فلتر البحث على title_ar وtitle_en وbody_ar وbody_en ثم يُقسّم النتائج إلى صفحات.
    /// </summary>
    /// <param name="dbQuery">الاستعلام الأساسي (قد يحتوي بالفعل على فلتر الكلمات المفتاحية للقسم).</param>
    /// <param name="query">بيانات البحث والتقسيم إلى صفحات.</param>
    /// <returns>ApiResponse يحتوي على القائمة المقسّمة إلى صفحات.</returns>
    private async Task<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>> BuildPagedResponseAsync(
        IQueryable<DiscoverContent> dbQuery,
        DiscoverContentQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var lowerSearch = query.Search.ToLower();
            dbQuery = dbQuery.Where(c =>
                c.TitleAr.ToLower().Contains(lowerSearch) ||
                c.TitleEn.ToLower().Contains(lowerSearch) ||
                c.BodyAr.ToLower().Contains(lowerSearch) ||
                c.BodyEn.ToLower().Contains(lowerSearch));
        }

        var totalEntries = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToResponse).ToList();

        var totalPages   = query.PageSize > 0 ? (int)Math.Ceiling((double)totalEntries / query.PageSize) : 0;
        var previousPage = query.PageNumber > 1 ? (int?)(query.PageNumber - 1) : null;
        var nextPage     = query.PageNumber < totalPages ? (int?)(query.PageNumber + 1) : null;

        var paginatedResponse = new PaginatedResponse<DiscoverContentResponseDto>
        {
            Items = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage  = query.PageNumber,
                NextPage     = nextPage,
                PerPage      = query.PageSize,
                PreviousPage = previousPage,
                TotalEntries = totalEntries,
                TotalPages   = totalPages
            }
        };

        return ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>.Ok(paginatedResponse);
    }

    /// <summary>
    /// يحوّل كائن DiscoverContent إلى DiscoverContentResponseDto مختصر للقوائم.
    /// </summary>
    private static DiscoverContentResponseDto MapToResponse(DiscoverContent content)
    {
        return new DiscoverContentResponseDto
        {
            ContentId     = content.ContentId,
            TitleAr       = content.TitleAr,
            TitleEn       = content.TitleEn,
            CoverImageUrl = content.CoverImageUrl
        };
    }

    /// <summary>
    /// يحوّل كائن DiscoverContent إلى DiscoverContentDetailsDto الكامل.
    /// </summary>
    private static DiscoverContentDetailsDto MapToDetails(DiscoverContent content)
    {
        return new DiscoverContentDetailsDto
        {
            ContentId     = content.ContentId,
            TitleAr       = content.TitleAr,
            TitleEn       = content.TitleEn,
            BodyAr        = content.BodyAr,
            BodyEn        = content.BodyEn,
            CoverImageUrl = content.CoverImageUrl
        };
    }

    /// <summary>
    /// يبني تعبيرًا شرطيًا OR قابلًا للترجمة إلى SQL للفلترة بالكلمات المفتاحية.
    /// يتحقق من كل كلمة مفتاحية في الحقول: TitleAr وTitleEn وBodyAr وBodyEn.
    /// </summary>
    /// <param name="keywords">قائمة الكلمات المفتاحية للقسم.</param>
    /// <returns>
    /// Expression OR مركّب يمكن لـ EF Core ترجمته إلى SQL LIKE،
    /// أو null إذا كانت القائمة فارغة.
    /// </returns>
    /// <remarks>
    /// السبب: EF Core لا يستطيع ترجمة keywords.Any(k => field.Contains(k)) إلى SQL
    /// لأن keywords مجموعة في الذاكرة واللامبدا الداخلية غير قابلة للترجمة.
    /// الحل: بناء شجرة تعبيرات OR يدويًا بحيث تكون كل قيمة مفتاحية ثابتة في التعبير.
    /// </remarks>
    private static Expression<Func<DiscoverContent, bool>>? BuildKeywordPredicate(
        IReadOnlyList<string> keywords)
    {
        if (keywords == null || keywords.Count == 0)
            return null;

        // The four text properties to search in
        string[] searchProperties = { "TitleAr", "TitleEn", "BodyAr", "BodyEn" };

        var parameter  = Expression.Parameter(typeof(DiscoverContent), "c");
        var toLowerMethod    = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var containsMethod   = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

        Expression? combined = null;

        foreach (var keyword in keywords)
        {
            // Lowercase the keyword once on the C# side — it becomes a constant in the SQL
            var lowerKeyword = Expression.Constant(keyword.ToLower());

            Expression? keywordCondition = null;

            foreach (var propertyName in searchProperties)
            {
                // c.{Property}.ToLower().Contains(lowerKeyword)
                var property       = Expression.Property(parameter, propertyName);
                var propertyLower  = Expression.Call(property, toLowerMethod);
                var containsCall   = Expression.Call(propertyLower, containsMethod, lowerKeyword);

                keywordCondition = keywordCondition == null
                    ? (Expression)containsCall
                    : Expression.OrElse(keywordCondition, containsCall);
            }

            if (keywordCondition != null)
            {
                combined = combined == null
                    ? keywordCondition
                    : Expression.OrElse(combined, keywordCondition);
            }
        }

        if (combined == null)
            return null;

        return Expression.Lambda<Func<DiscoverContent, bool>>(combined, parameter);
    }
}
