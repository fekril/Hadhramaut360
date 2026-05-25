using System;

namespace Had360back.Dtos.Organizations;

/// <summary>
/// بيانات الملف الشخصي للمؤسسة.
/// </summary>
public class OrganizationProfileDto
{
    /// <summary>
    /// معرّف المستخدم الخاص بالمؤسسة.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// البريد الإلكتروني للمؤسسة.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// رقم الهاتف للمؤسسة.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// اسم المؤسسة باللغة العربية.
    /// </summary>
    public string OrgNameAr { get; set; } = null!;

    /// <summary>
    /// اسم المؤسسة باللغة الإنجليزية.
    /// </summary>
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
    public string AddressAr { get; set; } = null!;

    /// <summary>
    /// العنوان باللغة الإنجليزية.
    /// </summary>
    public string AddressEn { get; set; } = null!;

    /// <summary>
    /// رابط الخريطة للمؤسسة.
    /// </summary>
    public string? MapUrl { get; set; }

    /// <summary>
    /// رابط شعار المؤسسة.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// تاريخ إنشاء الحساب.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ آخر تحديث للحساب.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
