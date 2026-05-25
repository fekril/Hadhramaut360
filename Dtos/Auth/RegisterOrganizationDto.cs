using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.Dtos.Auth;

public class RegisterOrganizationDto : RegisterUserDto
{
    [Required]
    [MaxLength(150)]
    public string OrgNameAr { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string OrgNameEn { get; set; } = null!;

    public string? DescriptionAr { get; set; }

    public string? DescriptionEn { get; set; }

    [Required]
    [MaxLength(150)]
    public string AddressAr { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string AddressEn { get; set; } = null!;

    [MaxLength(500)]
    public string? MapUrl { get; set; }

    /// <summary>
    /// ملف شعار المؤسسة.
    /// </summary>
    public IFormFile? LogoFile { get; set; }
}
