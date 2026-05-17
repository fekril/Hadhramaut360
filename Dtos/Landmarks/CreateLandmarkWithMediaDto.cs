using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Landmarks;

public class CreateLandmarkWithMediaDto
{
    [Required]
    [MaxLength(200)]
    public string TitleAr { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = null!;

    [Required]
    public string DescriptionAr { get; set; } = null!;

    [Required]
    public string DescriptionEn { get; set; } = null!;

    [MaxLength(255)]
    public string? LocationTextAr { get; set; }

    [MaxLength(255)]
    public string? LocationTextEn { get; set; }

    [Required]
    [MaxLength(1000)]
    public string MapUrl { get; set; } = null!;

    [Required]
    public List<int> CategoryIds { get; set; } = new();

    public List<IFormFile>? Files { get; set; }
}
