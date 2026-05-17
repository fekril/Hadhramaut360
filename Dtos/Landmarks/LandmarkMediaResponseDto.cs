using System;
using Had360back.Models;

namespace Had360back.DTOs.Landmarks;

public class LandmarkMediaResponseDto
{
    public int MediaId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public MediaType MediaType { get; set; }
    public DateTime CreatedAt { get; set; }
}
