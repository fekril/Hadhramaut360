using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.CommunityPostReports;

/// <summary>
/// معاملات الاستعلام لصفحة تقارير البلاغات الخاصة بالأدمن.
/// </summary>
public class CommunityPostReportQueryDto
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
