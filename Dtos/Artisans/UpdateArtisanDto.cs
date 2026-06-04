using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.Artisans;

/// <summary>
/// DTO لتحديث بيانات الحرفي النصية فقط (بدون ميديا).
/// إدارة الميديا لها endpoints منفصلة.
/// </summary>
public class UpdateArtisanDto
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
}
