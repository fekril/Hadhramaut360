using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.Dtos.Organizations;

/// <summary>
/// بيانات تحديث الملف الشخصي للمؤسسة.
/// </summary>
public class UpdateOrganizationProfileDto
{
    /// <summary>
    /// رقم الهاتف الخاص بالمؤسسة.
    /// </summary>
    [Phone(ErrorMessage = "رقم الهاتف غير صالح.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// اسم المؤسسة باللغة العربية.
    /// </summary>
    [Required(ErrorMessage = "اسم المؤسسة بالعربية مطلوب.")]
    [MaxLength(150, ErrorMessage = "اسم المؤسسة بالعربية يجب ألا يتجاوز 150 حرفًا.")]
    public string OrgNameAr { get; set; } = null!;

    /// <summary>
    /// اسم المؤسسة باللغة الإنجليزية.
    /// </summary>
    [Required(ErrorMessage = "اسم المؤسسة بالإنجليزية مطلوب.")]
    [MaxLength(150, ErrorMessage = "اسم المؤسسة بالإنجليزية يجب ألا يتجاوز 150 حرفًا.")]
    public string OrgNameEn { get; set; } = null!;

    /// <summary>
    /// وصف المؤسسة باللغة العربية.
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// وصف المؤسسة باللغة الإنجليزية.
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// العنوان باللغة العربية.
    /// </summary>
    [Required(ErrorMessage = "العنوان بالعربية مطلوب.")]
    [MaxLength(150, ErrorMessage = "العنوان بالعربية يجب ألا يتجاوز 150 حرفًا.")]
    public string AddressAr { get; set; } = null!;

    /// <summary>
    /// العنوان باللغة الإنجليزية.
    /// </summary>
    [Required(ErrorMessage = "العنوان بالإنجليزية مطلوب.")]
    [MaxLength(150, ErrorMessage = "العنوان بالإنجليزية يجب ألا يتجاوز 150 حرفًا.")]
    public string AddressEn { get; set; } = null!;

    /// <summary>
    /// رابط الخريطة للمؤسسة.
    /// </summary>
    [MaxLength(500, ErrorMessage = "رابط الخريطة يجب ألا يتجاوز 500 حرفًا.")]
    public string? MapUrl { get; set; }

    /// <summary>
    /// ملف شعار المؤسسة الجديد (اختياري).
    /// </summary>
    public IFormFile? LogoFile { get; set; }
}
