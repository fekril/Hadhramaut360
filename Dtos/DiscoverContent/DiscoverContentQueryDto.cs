using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.DiscoverContent;

public class DiscoverContentQueryDto
{
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
