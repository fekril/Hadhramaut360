using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO لإنشاء منشور مجتمعي جديد مع إمكانية رفع صور عبر multipart/form-data.
/// </summary>
public class CreateCommunityPostWithMediaDto
{
    /// <summary>
    /// نص المنشور. مطلوب.
    /// </summary>
    [Required]
    public string ContentText { get; set; } = null!;

    /// <summary>
    /// قائمة الصور المراد رفعها. اختيارية. الحد الأقصى 5 صور.
    /// </summary>
    public List<IFormFile>? Files { get; set; }
}
