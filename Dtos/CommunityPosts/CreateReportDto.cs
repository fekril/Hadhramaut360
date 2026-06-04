using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO لإرسال بلاغ على منشور مجتمعي.
/// </summary>
public class CreateReportDto
{
    /// <summary>
    /// سبب الإبلاغ. مطلوب.
    /// </summary>
    [Required]
    public string Reason { get; set; } = null!;
}
