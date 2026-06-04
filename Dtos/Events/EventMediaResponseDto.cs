using System;

namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات الفعالية المختصرة للميديا المرتبطة بها.
/// تُستخدم داخل EventDetailsDto و EventEditDto.
/// </summary>
public class EventMediaResponseDto
{
    /// <summary>
    /// رقم سجل الميديا الفريد.
    /// </summary>
    public int MediaId { get; set; }

    /// <summary>
    /// رقم الفعالية المرتبطة.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// الرابط النسبي للملف المخزن.
    /// </summary>
    public string MediaUrl { get; set; } = string.Empty;

    /// <summary>
    /// نوع الميديا (صورة، فيديو...).
    /// </summary>
    public Had360back.Models.MediaType MediaType { get; set; }

    /// <summary>
    /// تاريخ رفع الملف.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
