using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Artisans;

/// <summary>
/// DTO لإضافة ملف ميديا واحد لحرفي موجود.
/// </summary>
public class AddArtisanMediaDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
