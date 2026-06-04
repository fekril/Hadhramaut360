using System;
using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات تعديل فعالية موجودة. يُستخدم في PUT /events/{id}.
/// يحدّث بيانات الفعالية النصية والتواريخ فقط. إدارة الميديا لها نقاط نهاية منفصلة.
/// </summary>
public class UpdateEventDto
{
    /// <summary>
    /// عنوان الفعالية بالعربية. مطلوب. الحد الأقصى 200 حرف.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string TitleAr { get; set; } = null!;

    /// <summary>
    /// عنوان الفعالية بالإنجليزية. مطلوب. الحد الأقصى 200 حرف.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = null!;

    /// <summary>
    /// وصف الفعالية بالعربية. مطلوب.
    /// </summary>
    [Required]
    public string DescriptionAr { get; set; } = null!;

    /// <summary>
    /// وصف الفعالية بالإنجليزية. مطلوب.
    /// </summary>
    [Required]
    public string DescriptionEn { get; set; } = null!;

    /// <summary>
    /// عنوان مكان الفعالية بالعربية. مطلوب. الحد الأقصى 150 حرف.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string AddressAr { get; set; } = null!;

    /// <summary>
    /// عنوان مكان الفعالية بالإنجليزية. مطلوب. الحد الأقصى 150 حرف.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string AddressEn { get; set; } = null!;

    /// <summary>
    /// رابط خريطة موقع الفعالية. اختياري. الحد الأقصى 500 حرف.
    /// </summary>
    [MaxLength(500)]
    public string? MapUrl { get; set; }

    /// <summary>
    /// رابط نموذج التسجيل في الفعالية. اختياري. الحد الأقصى 500 حرف.
    /// </summary>
    [MaxLength(500)]
    public string? FormUrl { get; set; }

    /// <summary>
    /// تاريخ بداية الفعالية. مطلوب.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// تاريخ نهاية الفعالية. مطلوب. يجب أن يكون أكبر من أو يساوي تاريخ البداية.
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }
}
