using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Favorites;
using Had360back.DTOs.Landmarks;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Controller مسؤول عن إدارة مفضلات المعالم للمستخدم المُسجّل دخوله.
/// جميع الـ Endpoints تتطلب مصادقة JWT.
/// لا يُستخدم ManageContent policy هنا لأن المفضلات ميزة شخصية للمستخدم.
/// </summary>
[ApiController]
[Route(ApiRoutes.Favorites.Base)]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>
    /// يُرجع قائمة المعالم التي أضافها المستخدم الحالي إلى مفضلاته.
    /// </summary>
    /// <returns>
    /// ApiResponse يحتوي على List&lt;LandmarkResponseDto&gt; مع IsFavorite = true لكل عنصر.
    /// الفرونت إند يستخدم نفس LandmarkCard لصفحتي المعالم والمفضلات.
    /// يُرجع قائمة فارغة إذا لم يكن للمستخدم أي مفضلات.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Authenticated users only
    /// Route: GET /Had360/favorites/my
    /// </remarks>
    [HttpGet(ApiRoutes.Favorites.My)]
    public async Task<ActionResult<ApiResponse<List<LandmarkResponseDto>>>> GetMyFavorites()
    {
        var result = await _favoriteService.GetMyFavoritesAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يُضيف معلمًا إلى مفضلة المستخدم الحالي.
    /// </summary>
    /// <param name="landmarkId">رقم المعلم المراد إضافته إلى المفضلة (من الـ route).</param>
    /// <returns>
    /// ApiResponse يحتوي على FavoriteResponseDto بتفاصيل المعلم المُضاف.
    /// يُرجع 400 إذا كان المعلم موجودًا مسبقًا في المفضلة.
    /// يُرجع 404 إذا لم يكن المعلم موجودًا.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Authenticated users only
    /// Route: POST /Had360/favorites/{landmarkId}
    /// لا يتطلب request body.
    /// </remarks>
    [HttpPost(ApiRoutes.Favorites.ByLandmarkId)]
    public async Task<ActionResult<ApiResponse<FavoriteResponseDto>>> Add(int landmarkId)
    {
        var result = await _favoriteService.AddAsync(landmarkId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يُزيل معلمًا من مفضلة المستخدم الحالي.
    /// </summary>
    /// <param name="landmarkId">رقم المعلم المراد إزالته من المفضلة (من الـ route).</param>
    /// <returns>
    /// ApiResponse بقيمة true عند نجاح الحذف.
    /// يُرجع 404 إذا لم يكن المعلم في مفضلة المستخدم.
    /// </returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Authenticated users only
    /// Route: DELETE /Had360/favorites/{landmarkId}
    /// </remarks>
    [HttpDelete(ApiRoutes.Favorites.ByLandmarkId)]
    public async Task<ActionResult<ApiResponse<bool>>> Remove(int landmarkId)
    {
        var result = await _favoriteService.RemoveAsync(landmarkId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يتحقق من حالة مفضلة معلم محدد للمستخدم الحالي.
    /// </summary>
    /// <param name="landmarkId">رقم المعلم المراد التحقق من حالة مفضلته (من الـ route).</param>
    /// <returns>
    /// ApiResponse يحتوي على FavoriteStatusResponseDto.
    /// يُرجع 404 إذا لم يكن المعلم موجودًا.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Authenticated users only
    /// Route: GET /Had360/favorites/{landmarkId}/status
    /// </remarks>
    [HttpGet(ApiRoutes.Favorites.Status)]
    public async Task<ActionResult<ApiResponse<FavoriteStatusResponseDto>>> GetStatus(int landmarkId)
    {
        var result = await _favoriteService.IsFavoriteAsync(landmarkId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يُبدّل حالة مفضلة معلم: يُضيفه إذا لم يكن مفضلًا، أو يُزيله إذا كان مفضلًا.
    /// مُصمَّم لاستخدامه من زر القلب في الفرونت إند.
    /// </summary>
    /// <param name="landmarkId">رقم المعلم المراد تبديل حالة مفضلته (من الـ route).</param>
    /// <returns>
    /// ApiResponse يحتوي على FavoriteStatusResponseDto مع الحالة الجديدة بعد التبديل.
    /// يُرجع 404 إذا لم يكن المعلم موجودًا.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Authenticated users only
    /// Route: POST /Had360/favorites/{landmarkId}/toggle
    ///
    /// عند إضافة المعلم: IsFavorite = true
    /// عند إزالة المعلم: IsFavorite = false
    /// الفرونت إند يحدّث حالة القلب بناءً على data.isFavorite في الاستجابة.
    /// </remarks>
    [HttpPost(ApiRoutes.Favorites.Toggle)]
    public async Task<ActionResult<ApiResponse<FavoriteStatusResponseDto>>> Toggle(int landmarkId)
    {
        var result = await _favoriteService.ToggleAsync(landmarkId);
        return StatusCode(result.StatusCode, result);
    }
}
