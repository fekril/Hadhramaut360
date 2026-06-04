using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// معاملات الاستعلام للحصول على قائمة منشورات المجتمع مع التقسيم إلى صفحات.
/// </summary>
public class CommunityPostQueryDto
{
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
