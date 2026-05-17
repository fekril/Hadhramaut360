namespace Had360back.DTOs.Favorites;

/// <summary>
/// DTO يُمثّل حالة المعلم في مفضلة المستخدم الحالي.
/// يُستخدم في استجابات Status وToggle.
/// </summary>
public class FavoriteStatusResponseDto
{
    /// <summary>رقم المعلم المستعلم عنه.</summary>
    public int LandmarkId { get; set; }

    /// <summary>
    /// true إذا كان المعلم في مفضلة المستخدم الحالي، false إذا لم يكن.
    /// يُستخدم من الفرونت إند لتحديد حالة أيقونة القلب.
    /// </summary>
    public bool IsFavorite { get; set; }
}
