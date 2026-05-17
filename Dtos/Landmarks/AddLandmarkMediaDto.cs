using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Landmarks;

public class AddLandmarkMediaDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
