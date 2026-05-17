using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Favorites;
using Had360back.DTOs.Landmarks;

namespace Had360back.Services.Interfaces;

/// <summary>
/// عقد السيرفس المسؤول عن إدارة مفضلات المعالم للمستخدم الحالي.
/// جميع العمليات تعتمد على معرّف المستخدم المستخرج من JWT فقط.
/// </summary>
public interface IFavoriteService
{
    /// <summary>يُرجع قائمة المعالم المفضّلة للمستخدم الحالي كـ LandmarkResponseDto مع IsFavorite = true لكل عنصر.</summary>
    Task<ApiResponse<List<LandmarkResponseDto>>> GetMyFavoritesAsync();

    /// <summary>يُضيف معلمًا إلى مفضلة المستخدم الحالي. يرفض التكرار بـ 400.</summary>
    Task<ApiResponse<FavoriteResponseDto>> AddAsync(int landmarkId);

    /// <summary>يُزيل معلمًا من مفضلة المستخدم الحالي. يُرجع 404 إذا لم يكن في المفضلة.</summary>
    Task<ApiResponse<bool>> RemoveAsync(int landmarkId);

    /// <summary>يتحقق من حالة المفضلة لمعلم محدد للمستخدم الحالي.</summary>
    Task<ApiResponse<FavoriteStatusResponseDto>> IsFavoriteAsync(int landmarkId);

    /// <summary>
    /// يُبدّل حالة المفضلة: يُضيف إذا لم تكن مفضلة، ويُزيل إذا كانت مفضلة.
    /// مُصمَّم لزر القلب في الفرونت إند.
    /// </summary>
    Task<ApiResponse<FavoriteStatusResponseDto>> ToggleAsync(int landmarkId);
}
