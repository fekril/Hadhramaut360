using System;
using System.Collections.Generic;

namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات الفعالية المستخدمة في صفحة تعديل الفعالية.
/// تحتوي فقط على حقول الفعالية القابلة للتعديل والميديا الحالية.
/// لا تتضمن بيانات المنظمة الكاملة.
/// </summary>
public class EventEditDto
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
    /// وصف الفعالية بالعربية.
    /// </summary>
    public string DescriptionAr { get; set; } = string.Empty;

    /// <summary>
    /// وصف الفعالية بالإنجليزية.
    /// </summary>
    public string DescriptionEn { get; set; } = string.Empty;

    /// <summary>
    /// عنوان مكان الفعالية بالعربية.
    /// </summary>
    public string AddressAr { get; set; } = string.Empty;

    /// <summary>
    /// عنوان مكان الفعالية بالإنجليزية.
    /// </summary>
    public string AddressEn { get; set; } = string.Empty;

    /// <summary>
    /// رابط خريطة موقع الفعالية. اختياري.
    /// </summary>
    public string? MapUrl { get; set; }

    /// <summary>
    /// رابط نموذج التسجيل في الفعالية. اختياري.
    /// </summary>
    public string? FormUrl { get; set; }

    /// <summary>
    /// تاريخ بداية الفعالية.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// تاريخ نهاية الفعالية.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// قائمة ملفات الميديا الحالية المرتبطة بالفعالية.
    /// </summary>
    public List<EventMediaResponseDto> Media { get; set; } = new();
}
