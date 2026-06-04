using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO لتحديث نص المنشور المجتمعي فقط.
/// إدارة الميديا لها endpoints منفصلة.
/// </summary>
public class UpdateCommunityPostDto
{
    /// <summary>
    /// النص الجديد للمنشور. مطلوب.
    /// </summary>
    [Required]
    public string ContentText { get; set; } = null!;
}
