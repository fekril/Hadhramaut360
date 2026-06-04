using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Artisans;

/// <summary>
/// DTO لإنشاء حرفي جديد مع رفع ملفات الميديا عبر multipart/form-data.
/// </summary>
public class CreateArtisanWithMediaDto
{
    [Required]
    [MaxLength(150)]
    public string NameAr { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string NameEn { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = null!;

    [Required]
    public string DescriptionAr { get; set; } = null!;

    [Required]
    public string DescriptionEn { get; set; } = null!;

    [MaxLength(255)]
    public string? LocationTextAr { get; set; }

    [MaxLength(255)]
    public string? LocationTextEn { get; set; }

    [MaxLength(500)]
    public string? MapUrl { get; set; }

    /// <summary>
    /// قائمة الملفات المراد رفعها. أول ملف يُعيَّن صورة غلاف (IsCover = true).
    /// الحد الأقصى 10 ملفات.
    /// </summary>
    public List<IFormFile>? Files { get; set; }
}
