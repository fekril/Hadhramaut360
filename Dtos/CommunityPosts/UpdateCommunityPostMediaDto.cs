using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO لاستبدال ملف ميديا موجود بملف جديد.
/// </summary>
public class UpdateCommunityPostMediaDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
