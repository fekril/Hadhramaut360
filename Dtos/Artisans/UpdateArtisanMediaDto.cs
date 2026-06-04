using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Artisans;

/// <summary>
/// DTO لاستبدال ملف ميديا موجود بملف جديد.
/// يُحافظ على قيمة IsCover الحالية.
/// </summary>
public class UpdateArtisanMediaDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
