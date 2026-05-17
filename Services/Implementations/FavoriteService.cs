using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.Categories;
using Had360back.DTOs.Favorites;
using Had360back.DTOs.Landmarks;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// سيرفس إدارة مفضلات المعالم للمستخدم الحالي.
/// جميع العمليات تعتمد على معرّف المستخدم من JWT فقط —
/// لا تقبل UserId من الـ client أبدًا.
/// </summary>
public class FavoriteService : IFavoriteService
{
    private readonly DbContextApp _context;
    private readonly ICurrentUserService _currentUserService;

    public FavoriteService(DbContextApp context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /Had360/favorites/my
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// يُرجع المعالم المفضّلة للمستخدم الحالي كـ LandmarkResponseDto.
    /// جميع العناصر المُرجَعة تحمل IsFavorite = true.
    /// الفرونت إند يستطيع استخدام نفس مكوّن بطاقة المعلم لصفحتي المعالم والمفضلات.
    /// يُرجع 401 إذا لم يكن المستخدم مُسجّل دخوله.
    /// يُرجع قائمة فارغة بنجاح إذا لم يكن للمستخدم أي مفضلات.
    /// </summary>
    public async Task<ApiResponse<List<LandmarkResponseDto>>> GetMyFavoritesAsync()
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
            return ApiResponse<List<LandmarkResponseDto>>.Fail(
                "User is not authenticated.", null, 401);

        var favorites = await _context.Set<Favorite>()
            .AsNoTracking()
            .Where(f => f.UserId == userId.Value)
            .Include(f => f.Landmark)
                .ThenInclude(l => l.LandmarkMedia)
            .Include(f => f.Landmark)
                .ThenInclude(l => l.LandmarkCategories)
                    .ThenInclude(lc => lc.Category)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var dtos = favorites.Select(f => MapFavoriteToLandmarkResponse(f.Landmark)).ToList();

        return ApiResponse<List<LandmarkResponseDto>>.Ok(
            dtos, "Favorites retrieved successfully.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /Had360/favorites/{landmarkId}
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// يُضيف معلمًا إلى مفضلة المستخدم الحالي.
    /// يُرجع 404 إذا كان المعلم غير موجود.
    /// يُرجع 400 إذا كان المعلم موجودًا مسبقًا في المفضلة.
    /// يُرجع المفضلة المُنشأة مع تفاصيل المعلم بحالة 201.
    /// </summary>
    public async Task<ApiResponse<FavoriteResponseDto>> AddAsync(int landmarkId)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
            return ApiResponse<FavoriteResponseDto>.Fail(
                "User is not authenticated.", null, 401);

        // التحقق من وجود المعلم
        var landmarkExists = await _context.Set<Landmark>()
            .AnyAsync(l => l.LandmarkId == landmarkId);

        if (!landmarkExists)
            return ApiResponse<FavoriteResponseDto>.Fail(
                "Landmark not found.", null, 404);

        // منع التكرار — لا يوجد unique index لذا نتحقق يدويًا
        var alreadyExists = await _context.Set<Favorite>()
            .AnyAsync(f => f.UserId == userId.Value && f.LandmarkId == landmarkId);

        if (alreadyExists)
            return ApiResponse<FavoriteResponseDto>.Fail(
                "Landmark is already in favorites.", null, 400);

        var favorite = new Favorite
        {
            UserId = userId.Value,
            LandmarkId = landmarkId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Favorite>().Add(favorite);
        await _context.SaveChangesAsync();

        // إعادة تحميل المفضلة مع بيانات المعلم والميديا والتصنيفات
        var created = await _context.Set<Favorite>()
            .AsNoTracking()
            .Where(f => f.Id == favorite.Id)
            .Include(f => f.Landmark)
                .ThenInclude(l => l.LandmarkMedia)
            .Include(f => f.Landmark)
                .ThenInclude(l => l.LandmarkCategories)
                    .ThenInclude(lc => lc.Category)
            .FirstAsync();

        return ApiResponse<FavoriteResponseDto>.Ok(
            MapToFavoriteResponse(created),
            "Landmark added to favorites successfully.",
            201);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /Had360/favorites/{landmarkId}
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// يُزيل معلمًا من مفضلة المستخدم الحالي.
    /// يُرجع 404 إذا لم تكن المفضلة موجودة.
    /// يُرجع true بحالة 200 عند نجاح الحذف.
    /// </summary>
    public async Task<ApiResponse<bool>> RemoveAsync(int landmarkId)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
            return ApiResponse<bool>.Fail(
                "User is not authenticated.", null, 401);

        var favorite = await _context.Set<Favorite>()
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.LandmarkId == landmarkId);

        if (favorite == null)
            return ApiResponse<bool>.Fail(
                "Favorite not found.", null, 404);

        _context.Set<Favorite>().Remove(favorite);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Landmark removed from favorites successfully.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /Had360/favorites/{landmarkId}/status
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// يتحقق من حالة مفضلة معلم محدد للمستخدم الحالي.
    /// يُرجع 404 إذا كان المعلم غير موجود.
    /// </summary>
    public async Task<ApiResponse<FavoriteStatusResponseDto>> IsFavoriteAsync(int landmarkId)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
            return ApiResponse<FavoriteStatusResponseDto>.Fail(
                "User is not authenticated.", null, 401);

        var landmarkExists = await _context.Set<Landmark>()
            .AnyAsync(l => l.LandmarkId == landmarkId);

        if (!landmarkExists)
            return ApiResponse<FavoriteStatusResponseDto>.Fail(
                "Landmark not found.", null, 404);

        var isFavorite = await _context.Set<Favorite>()
            .AnyAsync(f => f.UserId == userId.Value && f.LandmarkId == landmarkId);

        return ApiResponse<FavoriteStatusResponseDto>.Ok(
            MapToFavoriteStatus(landmarkId, isFavorite),
            "Favorite status retrieved successfully.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /Had360/favorites/{landmarkId}/toggle
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// يُبدّل حالة مفضلة معلم للمستخدم الحالي.
    /// إذا كان المعلم مفضلًا يُزيله ويُرجع IsFavorite = false.
    /// إذا لم يكن مفضلًا يُضيفه ويُرجع IsFavorite = true.
    /// مُصمَّم لزر القلب في الفرونت إند.
    /// </summary>
    public async Task<ApiResponse<FavoriteStatusResponseDto>> ToggleAsync(int landmarkId)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
            return ApiResponse<FavoriteStatusResponseDto>.Fail(
                "User is not authenticated.", null, 401);

        var landmarkExists = await _context.Set<Landmark>()
            .AnyAsync(l => l.LandmarkId == landmarkId);

        if (!landmarkExists)
            return ApiResponse<FavoriteStatusResponseDto>.Fail(
                "Landmark not found.", null, 404);

        var existing = await _context.Set<Favorite>()
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.LandmarkId == landmarkId);

        if (existing != null)
        {
            // المعلم مفضَّل → أزِله
            _context.Set<Favorite>().Remove(existing);
            await _context.SaveChangesAsync();

            return ApiResponse<FavoriteStatusResponseDto>.Ok(
                MapToFavoriteStatus(landmarkId, isFavorite: false),
                "Landmark removed from favorites.");
        }
        else
        {
            // المعلم غير مفضَّل → أضِفه
            var favorite = new Favorite
            {
                UserId = userId.Value,
                LandmarkId = landmarkId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<Favorite>().Add(favorite);
            await _context.SaveChangesAsync();

            return ApiResponse<FavoriteStatusResponseDto>.Ok(
                MapToFavoriteStatus(landmarkId, isFavorite: true),
                "Landmark added to favorites.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private Mapping Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// يُحوّل كائن Favorite مع بيانات المعلم المُحمَّلة إلى FavoriteResponseDto.
    /// يفترض أن Landmark وLandmarkMedia وLandmarkCategories → Category قد تم تحميلها مسبقًا.
    /// </summary>
    private FavoriteResponseDto MapToFavoriteResponse(Favorite favorite)
    {
        var landmark = favorite.Landmark;

        return new FavoriteResponseDto
        {
            FavoriteId       = favorite.Id,
            LandmarkId       = landmark.LandmarkId,
            TitleAr          = landmark.TitleAr,
            TitleEn          = landmark.TitleEn,
            LocationTextAr   = landmark.LocationTextAr,
            LocationTextEn   = landmark.LocationTextEn,
            MapUrl           = landmark.MapUrl,
            CoverMediaUrl    = landmark.LandmarkMedia.FirstOrDefault()?.MediaUrl,
            CreatedAt        = favorite.CreatedAt,
            CategoryNamesAr  = landmark.LandmarkCategories
                                    .Select(lc => lc.Category.CategoryNameAr)
                                    .ToList(),
            CategoryNamesEn  = landmark.LandmarkCategories
                                    .Select(lc => lc.Category.CategoryNameEn)
                                    .ToList()
        };
    }

    /// <summary>يُنشئ FavoriteStatusResponseDto للمعلم المحدد بحالة المفضلة المُمرَّرة.</summary>
    private FavoriteStatusResponseDto MapToFavoriteStatus(int landmarkId, bool isFavorite)
    {
        return new FavoriteStatusResponseDto
        {
            LandmarkId = landmarkId,
            IsFavorite = isFavorite
        };
    }

    /// <summary>
    /// يُحوّل كائن Landmark (من مفضّلة) إلى LandmarkResponseDto.
    /// IsFavorite دائمًا true لأن المعلم مؤكّد أنه في مفضلة المستخدم.
    /// يفترض أن LandmarkMedia وLandmarkCategories → Category قد تم تحميلها مسبقًا.
    /// </summary>
    private LandmarkResponseDto MapFavoriteToLandmarkResponse(Landmark landmark)
    {
        return new LandmarkResponseDto
        {
            LandmarkId     = landmark.LandmarkId,
            TitleAr        = landmark.TitleAr,
            TitleEn        = landmark.TitleEn,
            LocationTextAr = landmark.LocationTextAr,
            LocationTextEn = landmark.LocationTextEn,
            MapUrl         = landmark.MapUrl,
            CreatedAt      = landmark.CreatedAt,
            UpdatedAt      = landmark.UpdatedAt,
            CoverMediaUrl  = landmark.LandmarkMedia
                                 .OrderBy(m => m.MediaId)
                                 .Select(m => m.MediaUrl)
                                 .FirstOrDefault(),
            Categories     = landmark.LandmarkCategories
                                 .Select(lc => new CategoryResponseDto
                                 {
                                     CategoryId     = lc.Category.CategoryId,
                                     CategoryNameAr = lc.Category.CategoryNameAr,
                                     CategoryNameEn = lc.Category.CategoryNameEn,
                                     CreatedAt      = lc.Category.CreatedAt
                                 })
                                 .ToList(),
            IsFavorite = true  // جميع عناصر القائمة مأخوذة من مفضلة المستخدم → IsFavorite = true دائمًا
        };
    }
}
