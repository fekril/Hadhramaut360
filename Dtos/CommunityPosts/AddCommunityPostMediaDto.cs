using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO لإضافة ملف ميديا واحد لمنشور موجود.
/// </summary>
public class AddCommunityPostMediaDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
