using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.Landmarks;

public class LandmarkQueryDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
