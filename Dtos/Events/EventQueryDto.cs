using System;

namespace Had360back.DTOs.Events;

/// <summary>
/// معاملات الاستعلام عند جلب قائمة الفعاليات.
/// يدعم البحث والفلترة والتقسيم إلى صفحات.
/// </summary>
public class EventQueryDto
{
    /// <summary>
    /// كلمة البحث تُطابق عناوين الفعالية ووصفها وعنوانها واسم المنظمة.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// يُرجع الفعاليات التي تبدأ من هذا التاريخ أو بعده.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// يُرجع الفعاليات التي تبدأ حتى هذا التاريخ أو قبله.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// إذا كانت true يُرجع فقط الفعاليات التي لم تنتهِ بعد (EndDate >= UtcNow).
    /// </summary>
    public bool? UpcomingOnly { get; set; }

    /// <summary>
    /// رقم الصفحة المطلوبة. القيمة الافتراضية 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// حجم الصفحة. القيمة الافتراضية 10. الحد الأقصى 100.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
