using System;

namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات الفعالية المختصرة المستخدمة في قائمة الفعاليات وقائمة فعاليات المنظمة.
/// تحتوي على صورة الغلاف المحسوبة من أول صورة مرتبطة بالفعالية.
/// </summary>
public class EventResponseDto
{
    /// <summary>
    /// رقم الفعالية الفريد.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// عنوان الفعالية بالعربية.
    /// </summary>
    public string TitleAr { get; set; } = string.Empty;

    /// <summary>
    /// عنوان الفعالية بالإنجليزية.
    /// </summary>
    public string TitleEn { get; set; } = string.Empty;

    /// <summary>
    /// عنوان مكان الفعالية بالعربية.
    /// </summary>
    public string AddressAr { get; set; } = string.Empty;

    /// <summary>
    /// عنوان مكان الفعالية بالإنجليزية.
    /// </summary>
    public string AddressEn { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ بداية الفعالية.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// تاريخ نهاية الفعالية.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// رابط صورة الغلاف. يُحسب من أول صورة مرتبطة بالفعالية. قد يكون null إذا لم تكن هناك صور.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// رقم المستخدم المنظمة التي أنشأت الفعالية.
    /// </summary>
    public int OrganizationUserId { get; set; }

    /// <summary>
    /// اسم المنظمة المنظِّمة للفعالية بالعربية.
    /// </summary>
    public string OrganizationNameAr { get; set; } = string.Empty;

    /// <summary>
    /// اسم المنظمة المنظِّمة للفعالية بالإنجليزية.
    /// </summary>
    public string OrganizationNameEn { get; set; } = string.Empty;

    /// <summary>
    /// رابط شعار المنظمة. قد يكون null إذا لم يتم رفع شعار.
    /// </summary>
    public string? OrganizationLogoUrl { get; set; }
}
