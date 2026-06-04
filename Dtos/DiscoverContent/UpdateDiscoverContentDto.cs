using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.DiscoverContent;

/// <summary>
/// بيانات تحديث عنصر محتوى اكتشاف موجود.
/// يُستخدم في PUT /discover-content/{id} بصيغة multipart/form-data.
/// إذا لم يُرسل ملف جديد، تُحافظ العملية على صورة الغلاف الحالية.
/// </summary>
public class UpdateDiscoverContentDto
{
    [Required]
    [MaxLength(200)]
    public string TitleAr { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = null!;

    [Required]
    public string BodyAr { get; set; } = null!;

    [Required]
    public string BodyEn { get; set; } = null!;

    /// <summary>
    /// ملف صورة الغلاف الجديد (اختياري). إذا أُرسل يستبدل الصورة القديمة.
    /// الحد الأقصى 5MB. الصيغ المقبولة: jpg، jpeg، png، webp.
    /// </summary>
    public IFormFile? CoverImage { get; set; }
}
