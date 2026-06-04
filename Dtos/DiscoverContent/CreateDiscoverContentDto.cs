using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.DiscoverContent;

/// <summary>
/// بيانات إنشاء عنصر محتوى اكتشاف جديد.
/// يُستخدم في POST /discover-content بصيغة multipart/form-data.
/// صورة الغلاف اختيارية. الحد الأقصى للحجم 5MB.
/// </summary>
public class CreateDiscoverContentDto
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
    /// ملف صورة الغلاف (اختياري). الحد الأقصى 5MB. الصيغ المقبولة: jpg، jpeg، png، webp.
    /// </summary>
    public IFormFile? CoverImage { get; set; }
}
