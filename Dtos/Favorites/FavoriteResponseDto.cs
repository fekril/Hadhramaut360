using System;
using System.Collections.Generic;

namespace Had360back.DTOs.Favorites;

/// <summary>
/// DTO يُمثّل معلمًا مفضلًا للمستخدم الحالي.
/// يُستخدم في استجابة قائمة المفضلات.
/// </summary>
public class FavoriteResponseDto
{
    /// <summary>رقم المفضلة في قاعدة البيانات.</summary>
    public int FavoriteId { get; set; }

    /// <summary>رقم المعلم المرتبط بهذه المفضلة.</summary>
    public int LandmarkId { get; set; }

    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;

    public string? LocationTextAr { get; set; }
    public string? LocationTextEn { get; set; }

    public string MapUrl { get; set; } = null!;

    /// <summary>
    /// رابط أول صورة مرتبطة بالمعلم. يُستخدم كصورة غلاف في الكارد.
    /// يكون null إذا لم يكن للمعلم أي صور.
    /// </summary>
    public string? CoverMediaUrl { get; set; }

    /// <summary>تاريخ إضافة المعلم إلى المفضلة.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>أسماء تصنيفات المعلم بالعربية.</summary>
    public List<string> CategoryNamesAr { get; set; } = new();

    /// <summary>أسماء تصنيفات المعلم بالإنجليزية.</summary>
    public List<string> CategoryNamesEn { get; set; } = new();
}
