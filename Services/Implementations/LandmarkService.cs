using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.Categories;
using Had360back.DTOs.Landmarks;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

public class LandmarkService : ILandmarkService
{
    private readonly DbContextApp _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public LandmarkService(
        DbContextApp context,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }
    /// <summary>
    /// يجلب قائمة المعالم من قاعدة البيانات مع دعم البحث، الفلترة حسب التصنيف، والتقسيم إلى صفحات.
    /// </summary>
    /// <param name="query">
    /// يحتوي على بيانات الاستعلام مثل كلمة البحث، رقم التصنيف، رقم الصفحة، وحجم الصفحة.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على قائمة LandmarkResponseDto.
    /// كل عنصر يمثل بيانات مختصرة عن معلم، مع أول صورة كصورة غلاف والتصنيفات المرتبطة.
    /// </returns>
    /// <remarks>
    /// هذه الدالة تستخدم Include لجلب التصنيفات والميديا المرتبطة بكل معلم.
    /// يتم ترتيب النتائج من الأحدث إلى الأقدم حسب CreatedAt.
    /// </remarks>
    public async Task<ApiResponse<PaginatedResponse<LandmarkResponseDto>>> GetAllAsync(LandmarkQueryDto query)
    {
        var dbQuery = _context.Set<Landmark>()
            .Include(l => l.LandmarkCategories)
                .ThenInclude(lc => lc.Category)
            .Include(l => l.LandmarkMedia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var lowerSearch = query.Search.ToLower();
            dbQuery = dbQuery.Where(l =>
                l.TitleAr.ToLower().Contains(lowerSearch) ||
                l.TitleEn.ToLower().Contains(lowerSearch) ||
                l.DescriptionAr.ToLower().Contains(lowerSearch) ||
                l.DescriptionEn.ToLower().Contains(lowerSearch));
        }

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(l => l.LandmarkCategories.Any(lc => lc.CategoryId == query.CategoryId.Value));
        }

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        // تحميل معرّفات المعالم المفضّلة للمستخدم الحالي دفعةً واحدة لتجنّب N+1 queries.
        // المستخدمون الضيوف يحصلون دائمًا على IsFavorite = false.
        var favoriteLandmarkIds = new HashSet<int>();
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId.HasValue)
        {
            var ids = await _context.Set<Favorite>()
                .Where(f => f.UserId == currentUserId.Value)
                .Select(f => f.LandmarkId)
                .ToListAsync();
            favoriteLandmarkIds = new HashSet<int>(ids);
        }

        var dtos = items.Select(l => MapToLandmarkResponse(l, favoriteLandmarkIds)).ToList();

        var totalEntries = totalItems;
        var currentPage = query.PageNumber;
        var perPage = query.PageSize;
        var totalPages = perPage > 0 ? (int)Math.Ceiling((double)totalEntries / perPage) : 0;
        var previousPage = currentPage > 1 ? (int?)(currentPage - 1) : null;
        var nextPage = currentPage < totalPages ? (int?)(currentPage + 1) : null;

        var paginatedResponse = new PaginatedResponse<LandmarkResponseDto>
        {
            Items = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage = currentPage,
                NextPage = nextPage,
                PerPage = perPage,
                PreviousPage = previousPage,
                TotalEntries = totalEntries,
                TotalPages = totalPages
            }
        };

        return ApiResponse<PaginatedResponse<LandmarkResponseDto>>.Ok(paginatedResponse);
    }
    
    
    /// <summary>
    /// يجلب تفاصيل معلم واحد حسب رقم المعلم.
    /// </summary>
    /// <param name="id">
    /// رقم المعلم المطلوب جلب تفاصيله.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على LandmarkDetailsDto عند العثور على المعلم.
    /// يرجع خطأ 404 إذا لم يتم العثور على المعلم.
    /// </returns>
    /// <remarks>
    /// هذه الدالة ترجع تفاصيل كاملة تشمل بيانات المعلم، التصنيفات، وجميع ملفات الميديا المرتبطة به.
    /// </remarks>
    public async Task<ApiResponse<LandmarkDetailsDto>> GetByIdAsync(int id)
    {
        var landmark = await _context.Set<Landmark>()
            .Include(l => l.LandmarkCategories)
                .ThenInclude(lc => lc.Category)
            .Include(l => l.LandmarkMedia)
            .FirstOrDefaultAsync(l => l.LandmarkId == id);

        if (landmark == null)
            return ApiResponse<LandmarkDetailsDto>.Fail("Landmark not found.", null, 404);

        // حساب IsFavorite بـ AnyAsync واحدة — لا يُعيد تحميل بيانات زائدة.
        var currentUserId = _currentUserService.GetUserId();
        var isFavorite = false;
        if (currentUserId.HasValue)
        {
            isFavorite = await _context.Set<Favorite>()
                .AnyAsync(f => f.UserId == currentUserId.Value && f.LandmarkId == id);
        }

        return ApiResponse<LandmarkDetailsDto>.Ok(MapToLandmarkDetails(landmark, isFavorite));
    }


    /// <summary>
    /// ينشئ معلمًا جديدًا بدون ملفات ميديا.
    /// </summary>
    /// <param name="dto">
    /// بيانات المعلم الجديد مثل العنوان، الوصف، الموقع، رابط الخريطة، وقائمة التصنيفات.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على تفاصيل المعلم بعد إنشائه.
    /// يرجع خطأ إذا لم يتم إرسال تصنيفات أو إذا كانت إحدى التصنيفات غير موجودة.
    /// </returns>
    /// <remarks>
    /// تتحقق الدالة أولًا من وجود تصنيف واحد على الأقل.
    /// ثم تتحقق أن جميع التصنيفات المرسلة موجودة في قاعدة البيانات.
    /// بعد ذلك تنشئ المعلم وتربطه بالتصنيفات ثم تحفظه.
    /// </remarks>
    public async Task<ApiResponse<LandmarkDetailsDto>> CreateAsync(CreateLandmarkDto dto)
    {
        if (dto.CategoryIds == null || !dto.CategoryIds.Any())
            return ApiResponse<LandmarkDetailsDto>.Fail("At least one category is required.");

        var validCategoriesCount = await _context.Set<Category>()
            .CountAsync(c => dto.CategoryIds.Contains(c.CategoryId));

        var distinctCategoryIds = dto.CategoryIds.Distinct().ToList();

        if (validCategoriesCount != distinctCategoryIds.Count)
            return ApiResponse<LandmarkDetailsDto>.Fail("One or more category IDs are invalid.");

        var landmark = new Landmark
        {
            TitleAr = dto.TitleAr,
            TitleEn = dto.TitleEn,
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
            LocationTextAr = dto.LocationTextAr,
            LocationTextEn = dto.LocationTextEn,
            MapUrl = dto.MapUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var catId in distinctCategoryIds)
        {
            landmark.LandmarkCategories.Add(new LandmarkCategory { CategoryId = catId });
        }

        _context.Set<Landmark>().Add(landmark);
        await _context.SaveChangesAsync();

        // Reload to get full details (if needed) or just construct from what we have
        return await GetByIdAsync(landmark.LandmarkId);
    }


    /// <summary>
    /// ينشئ معلمًا جديدًا مع إمكانية رفع ملفات ميديا مرتبطة به.
    /// </summary>
    /// <param name="dto">
    /// بيانات المعلم الجديد بالإضافة إلى قائمة الملفات المراد رفعها.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على تفاصيل المعلم الجديد مع التصنيفات والميديا.
    /// يرجع خطأ في حالة التصنيفات غير الصحيحة أو تجاوز عدد أو حجم الملفات المسموح.
    /// </returns>
    /// <remarks>
    /// تتحقق الدالة من التصنيفات، ثم من عدد الملفات بحيث لا يتجاوز 10 ملفات.
    /// كما تتحقق أن حجم كل ملف لا يتجاوز 5MB.
    /// تستخدم Transaction لضمان أن إنشاء المعلم وحفظ الميديا يتمان كعملية واحدة.
    /// في حالة حدوث خطأ يتم التراجع عن تغييرات قاعدة البيانات وحذف الملفات التي تم رفعها.
    /// </remarks>
    public async Task<ApiResponse<LandmarkDetailsDto>> CreateWithMediaAsync(CreateLandmarkWithMediaDto dto)
    {
        if (dto.CategoryIds == null || !dto.CategoryIds.Any())
            return ApiResponse<LandmarkDetailsDto>.Fail("At least one category is required.");

        var distinctCategoryIds = dto.CategoryIds.Distinct().ToList();
        var validCategoriesCount = await _context.Set<Category>()
            .CountAsync(c => distinctCategoryIds.Contains(c.CategoryId));

        if (validCategoriesCount != distinctCategoryIds.Count)
            return ApiResponse<LandmarkDetailsDto>.Fail("One or more category IDs are invalid.");

        if (dto.Files != null && dto.Files.Count > 10)
            return ApiResponse<LandmarkDetailsDto>.Fail("Maximum 10 files allowed.");

        // Additional file validation can be done here or in FileStorageService
        if (dto.Files != null)
        {
            foreach (var file in dto.Files)
            {
                if (file.Length > 10 * 1024 * 1024)
                    return ApiResponse<LandmarkDetailsDto>.Fail("One or more files exceed the 10MB size limit.");
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        var savedFileUrls = new List<string>();

        try
        {
            var landmark = new Landmark
            {
                TitleAr = dto.TitleAr,
                TitleEn = dto.TitleEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                LocationTextAr = dto.LocationTextAr,
                LocationTextEn = dto.LocationTextEn,
                MapUrl = dto.MapUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var catId in distinctCategoryIds)
            {
                landmark.LandmarkCategories.Add(new LandmarkCategory { CategoryId = catId });
            }

            _context.Set<Landmark>().Add(landmark);
            await _context.SaveChangesAsync();

            if (dto.Files != null && dto.Files.Any())
            {
                foreach (var file in dto.Files)
                {
                    var fileUrl = await _fileStorageService.SaveLandmarkMediaAsync(landmark.LandmarkId, file);
                    savedFileUrls.Add(fileUrl);

                    var media = new LandmarkMedia
                    {
                        LandmarkId = landmark.LandmarkId,
                        MediaUrl = fileUrl,
                        MediaType = MediaType.Image,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Set<LandmarkMedia>().Add(media);
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return await GetByIdAsync(landmark.LandmarkId);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            // Rollback files
            foreach (var url in savedFileUrls)
            {
                await _fileStorageService.DeleteFileAsync(url);
            }

            return ApiResponse<LandmarkDetailsDto>.Fail("An error occurred while creating landmark with media. Operation rolled back.", null, 500);
        }
    }


    /// <summary>
    /// يحدث بيانات معلم موجود مع تحديث التصنيفات المرتبطة به.
    /// </summary>
    /// <param name="id">
    /// رقم المعلم المراد تعديله.
    /// </param>
    /// <param name="dto">
    /// البيانات الجديدة للمعلم، وتشمل العنوان، الوصف، الموقع، رابط الخريطة، والتصنيفات.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على تفاصيل المعلم بعد التحديث.
    /// يرجع خطأ 404 إذا لم يتم العثور على المعلم.
    /// يرجع خطأ إذا لم يتم إرسال تصنيفات أو إذا كانت التصنيفات غير صحيحة.
    /// </returns>
    /// <remarks>
    /// تستخدم الدالة Transaction لأن العملية تعدل بيانات المعلم وعلاقاته بالتصنيفات.
    /// يتم حذف علاقات التصنيفات القديمة ثم إضافة التصنيفات الجديدة.
    /// </remarks>
    public async Task<ApiResponse<LandmarkDetailsDto>> UpdateAsync(int id, UpdateLandmarkDto dto)
    {
        var landmark = await _context.Set<Landmark>()
            .Include(l => l.LandmarkCategories)
            .FirstOrDefaultAsync(l => l.LandmarkId == id);

        if (landmark == null)
            return ApiResponse<LandmarkDetailsDto>.Fail("Landmark not found.", null, 404);

        if (dto.CategoryIds == null || !dto.CategoryIds.Any())
            return ApiResponse<LandmarkDetailsDto>.Fail("At least one category is required.");

        var distinctCategoryIds = dto.CategoryIds.Distinct().ToList();
        var validCategoriesCount = await _context.Set<Category>()
            .CountAsync(c => distinctCategoryIds.Contains(c.CategoryId));

        if (validCategoriesCount != distinctCategoryIds.Count)
            return ApiResponse<LandmarkDetailsDto>.Fail("One or more category IDs are invalid.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            landmark.TitleAr = dto.TitleAr;
            landmark.TitleEn = dto.TitleEn;
            landmark.DescriptionAr = dto.DescriptionAr;
            landmark.DescriptionEn = dto.DescriptionEn;
            landmark.LocationTextAr = dto.LocationTextAr;
            landmark.LocationTextEn = dto.LocationTextEn;
            landmark.MapUrl = dto.MapUrl;
            landmark.UpdatedAt = DateTime.UtcNow;

            _context.Set<LandmarkCategory>().RemoveRange(landmark.LandmarkCategories);
            
            foreach (var catId in distinctCategoryIds)
            {
                landmark.LandmarkCategories.Add(new LandmarkCategory { CategoryId = catId });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(landmark.LandmarkId);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return ApiResponse<LandmarkDetailsDto>.Fail("An error occurred while updating the landmark.", null, 500);
        }
    }


    /// <summary>
    /// يحذف معلمًا موجودًا وجميع ملفات الميديا المرتبطة به.
    /// </summary>
    /// <param name="id">
    /// رقم المعلم المراد حذفه.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على true عند نجاح الحذف.
    /// يرجع خطأ 404 إذا لم يتم العثور على المعلم.
    /// </returns>
    /// <remarks>
    /// تقوم الدالة أولًا بجلب المعلم مع ملفات الميديا المرتبطة به.
    /// بعد حذف المعلم من قاعدة البيانات، يتم حذف الملفات الفعلية من التخزين باستخدام FileStorageService.
    /// </remarks>
    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var landmark = await _context.Set<Landmark>()
            .Include(l => l.LandmarkMedia)
            .FirstOrDefaultAsync(l => l.LandmarkId == id);

        if (landmark == null)
            return ApiResponse<bool>.Fail("Landmark not found.", null, 404);

        var mediaUrls = landmark.LandmarkMedia.Select(m => m.MediaUrl).ToList();

        _context.Set<Landmark>().Remove(landmark);
        await _context.SaveChangesAsync();

        foreach (var url in mediaUrls)
        {
            await _fileStorageService.DeleteFileAsync(url);
        }

        return ApiResponse<bool>.Ok(true, "Landmark deleted successfully");
    }


    /// <summary>
    /// يضيف ملف ميديا جديد إلى معلم موجود.
    /// </summary>
    /// <param name="landmarkId">
    /// رقم المعلم الذي سيتم ربط ملف الميديا به.
    /// </param>
    /// <param name="dto">
    /// يحتوي على الملف المراد رفعه.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على بيانات الميديا بعد رفعها وحفظها.
    /// يرجع خطأ 404 إذا لم يكن المعلم موجودًا.
    /// </returns>
    /// <remarks>
    /// تتحقق الدالة أولًا من وجود المعلم.
    /// ثم تحفظ الملف باستخدام FileStorageService.
    /// بعد نجاح رفع الملف، يتم حفظ رابط الملف في جدول LandmarkMedia.
    /// إذا فشل حفظ السجل في قاعدة البيانات، يتم حذف الملف الذي تم رفعه لتجنب وجود ملفات غير مرتبطة.
    /// </remarks>
    public async Task<ApiResponse<LandmarkMediaResponseDto>> AddMediaAsync(int landmarkId, AddLandmarkMediaDto dto)
    {
        var exists = await _context.Set<Landmark>().AnyAsync(l => l.LandmarkId == landmarkId);
        if (!exists)
            return ApiResponse<LandmarkMediaResponseDto>.Fail("Landmark not found.", null, 404);

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SaveLandmarkMediaAsync(landmarkId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<LandmarkMediaResponseDto>.Fail(ex.Message);
        }

        var media = new LandmarkMedia
        {
            LandmarkId = landmarkId,
            MediaUrl = savedFileUrl,
            MediaType = MediaType.Image,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Set<LandmarkMedia>().Add(media);
            await _context.SaveChangesAsync();

            return ApiResponse<LandmarkMediaResponseDto>.Ok(MapToMediaResponse(media), "Media uploaded successfully", 201);
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<LandmarkMediaResponseDto>.Fail("Failed to save media to database.", null, 500);
        }
    }


    /// <summary>
    /// يحدث ملف ميديا موجود عن طريق استبداله بملف جديد.
    /// </summary>
    /// <param name="mediaId">
    /// رقم ملف الميديا المراد تحديثه.
    /// </param>
    /// <param name="dto">
    /// يحتوي على الملف الجديد الذي سيحل محل الملف القديم.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على بيانات الميديا بعد التحديث.
    /// يرجع خطأ 404 إذا لم يتم العثور على الميديا.
    /// </returns>
    /// <remarks>
    /// تحفظ الدالة رابط الملف القديم قبل التحديث.
    /// ثم ترفع الملف الجديد وتحدث الرابط في قاعدة البيانات.
    /// بعد نجاح تحديث قاعدة البيانات، يتم حذف الملف القديم من التخزين.
    /// إذا فشل تحديث قاعدة البيانات، يتم حذف الملف الجديد حتى لا يبقى ملف غير مستخدم.
    /// </remarks>
    public async Task<ApiResponse<LandmarkMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateLandmarkMediaDto dto)
    {
        var media = await _context.Set<LandmarkMedia>().FirstOrDefaultAsync(m => m.MediaId == mediaId);
        if (media == null)
            return ApiResponse<LandmarkMediaResponseDto>.Fail("Media not found.", null, 404);

        var oldUrl = media.MediaUrl;

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SaveLandmarkMediaAsync(media.LandmarkId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<LandmarkMediaResponseDto>.Fail(ex.Message);
        }

        media.MediaUrl = savedFileUrl;

        try
        {
            await _context.SaveChangesAsync();
            await _fileStorageService.DeleteFileAsync(oldUrl);
            
            return ApiResponse<LandmarkMediaResponseDto>.Ok(MapToMediaResponse(media), "Media updated successfully");
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<LandmarkMediaResponseDto>.Fail("Failed to update media in database.", null, 500);
        }
    }


    /// <summary>
    /// يحذف ملف ميديا من قاعدة البيانات ومن التخزين.
    /// </summary>
    /// <param name="mediaId">
    /// رقم ملف الميديا المراد حذفه.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على true عند نجاح الحذف.
    /// يرجع خطأ 404 إذا لم يتم العثور على الميديا.
    /// </returns>
    /// <remarks>
    /// تقوم الدالة بجلب سجل الميديا أولًا للحصول على رابط الملف.
    /// ثم تحذف السجل من قاعدة البيانات.
    /// بعد ذلك تحذف الملف الفعلي من التخزين باستخدام FileStorageService.
    /// </remarks>
    public async Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId)
    {
        var media = await _context.Set<LandmarkMedia>().FirstOrDefaultAsync(m => m.MediaId == mediaId);
        if (media == null)
            return ApiResponse<bool>.Fail("Media not found.", null, 404);

        var url = media.MediaUrl;
        _context.Set<LandmarkMedia>().Remove(media);
        await _context.SaveChangesAsync();

        await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Media deleted successfully");
    }

    // Private Mapping Methods
    /// <summary>
    /// يحول كائن Category من قاعدة البيانات إلى CategoryResponseDto.
    /// </summary>
    /// <param name="category">
    /// كائن التصنيف القادم من قاعدة البيانات.
    /// </param>
    /// <returns>
    /// DTO يحتوي على بيانات التصنيف المناسبة للإرجاع في API Response.
    /// </returns>
    /// <remarks>
    /// تستخدم هذه الدالة داخل دوال تحويل المعالم لإرجاع التصنيفات بشكل منظم للفرونت إند.
    /// </remarks>
    private CategoryResponseDto MapToCategoryResponse(Category category)
    {
        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            CategoryNameAr = category.CategoryNameAr,
            CategoryNameEn = category.CategoryNameEn,
            CreatedAt = category.CreatedAt
        };
    }


    /// <summary>
    /// يحول كائن Landmark إلى LandmarkResponseDto مختصر.
    /// </summary>
    /// <param name="landmark">
    /// كائن المعلم القادم من قاعدة البيانات مع التصنيفات والميديا.
    /// </param>
    /// <returns>
    /// DTO مختصر يحتوي على بيانات المعلم الأساسية، صورة الغلاف، والتصنيفات.
    /// </returns>
    /// <remarks>
    /// تستخدم هذه الدالة غالبًا في قائمة المعالم.
    /// يتم اختيار أول ملف ميديا كصورة غلاف للمعلم.
    /// </remarks>
    /// <param name="favoriteLandmarkIds">
    /// HashSet يحتوي على معرّفات المعالم المفضّلة للمستخدم الحالي.
    /// يجب أن يُحمَّل مرة واحدة فقط قبل الاستدعاء لتجنّب N+1 queries.
    /// يُمرَّر HashSet فارغ للمستخدمين الضيوف.
    /// </param>
    private LandmarkResponseDto MapToLandmarkResponse(Landmark landmark, HashSet<int> favoriteLandmarkIds)
    {
        return new LandmarkResponseDto
        {
            LandmarkId = landmark.LandmarkId,
            TitleAr = landmark.TitleAr,
            TitleEn = landmark.TitleEn,
            LocationTextAr = landmark.LocationTextAr,
            LocationTextEn = landmark.LocationTextEn,
            MapUrl = landmark.MapUrl,
            CreatedAt = landmark.CreatedAt,
            UpdatedAt = landmark.UpdatedAt,
            CoverMediaUrl = landmark.LandmarkMedia.FirstOrDefault()?.MediaUrl,
            Categories = landmark.LandmarkCategories.Select(lc => MapToCategoryResponse(lc.Category)).ToList(),
            IsFavorite = favoriteLandmarkIds.Contains(landmark.LandmarkId)
        };
    }


    /// <summary>
    /// يحول كائن Landmark إلى LandmarkDetailsDto يحتوي على تفاصيل كاملة.
    /// </summary>
    /// <param name="landmark">
    /// كائن المعلم القادم من قاعدة البيانات مع التصنيفات وجميع ملفات الميديا.
    /// </param>
    /// <returns>
    /// DTO يحتوي على تفاصيل المعلم الكاملة، التصنيفات، وجميع ملفات الميديا.
    /// </returns>
    /// <remarks>
    /// تستخدم هذه الدالة عند عرض تفاصيل معلم واحد.
    /// الفرق بينها وبين MapToLandmarkResponse أنها ترجع الوصف الكامل وجميع ملفات الميديا.
    /// </remarks>
    /// <param name="isFavorite">
    /// true إذا كان المستخدم الحالي قد أضاف هذا المعلم إلى مفضلته.
    /// false للمستخدمين الضيوف أو إذا لم يكن المعلم في المفضلة.
    /// </param>
    private LandmarkDetailsDto MapToLandmarkDetails(Landmark landmark, bool isFavorite)
    {
        return new LandmarkDetailsDto
        {
            LandmarkId = landmark.LandmarkId,
            TitleAr = landmark.TitleAr,
            TitleEn = landmark.TitleEn,
            DescriptionAr = landmark.DescriptionAr,
            DescriptionEn = landmark.DescriptionEn,
            LocationTextAr = landmark.LocationTextAr,
            LocationTextEn = landmark.LocationTextEn,
            MapUrl = landmark.MapUrl,
            CreatedAt = landmark.CreatedAt,
            UpdatedAt = landmark.UpdatedAt,
            Categories = landmark.LandmarkCategories.Select(lc => MapToCategoryResponse(lc.Category)).ToList(),
            Media = landmark.LandmarkMedia.Select(MapToMediaResponse).ToList(),
            IsFavorite = isFavorite
        };
    }


    /// <summary>
    /// يحول كائن LandmarkMedia إلى LandmarkMediaResponseDto.
    /// </summary>
    /// <param name="media">
    /// كائن الميديا القادم من قاعدة البيانات.
    /// </param>
    /// <returns>
    /// DTO يحتوي على رقم الميديا، رابط الملف، نوع الميديا، وتاريخ الإنشاء.
    /// </returns>
    /// <remarks>
    /// تستخدم هذه الدالة عند إرجاع ميديا المعلم في التفاصيل أو بعد عمليات الإضافة والتعديل.
    /// </remarks>
    private LandmarkMediaResponseDto MapToMediaResponse(LandmarkMedia media)
    {
        return new LandmarkMediaResponseDto
        {
            MediaId = media.MediaId,
            MediaUrl = media.MediaUrl,
            MediaType = media.MediaType,
            CreatedAt = media.CreatedAt
        };
    }
}
