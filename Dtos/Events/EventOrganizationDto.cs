namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات المنظمة المنظِّمة للفعالية المستخدمة في صفحة تفاصيل الفعالية.
/// يحتوي على معلومات المنظمة الكاملة بما فيها رقم الهاتف.
/// </summary>
public class EventOrganizationDto
{
    /// <summary>
    /// رقم مستخدم المنظمة.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// اسم المنظمة بالعربية.
    /// </summary>
    public string OrgNameAr { get; set; } = string.Empty;

    /// <summary>
    /// اسم المنظمة بالإنجليزية.
    /// </summary>
    public string OrgNameEn { get; set; } = string.Empty;

    /// <summary>
    /// رابط شعار المنظمة. قد يكون null.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// وصف المنظمة بالعربية. قد يكون null.
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// وصف المنظمة بالإنجليزية. قد يكون null.
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// عنوان المنظمة بالعربية.
    /// </summary>
    public string AddressAr { get; set; } = string.Empty;

    /// <summary>
    /// عنوان المنظمة بالإنجليزية.
    /// </summary>
    public string AddressEn { get; set; } = string.Empty;

    /// <summary>
    /// رابط خريطة موقع المنظمة. قد يكون null.
    /// </summary>
    public string? MapUrl { get; set; }

    /// <summary>
    /// رقم هاتف المنظمة مأخوذ من بيانات المستخدم (User.PhoneNumber). قد يكون null.
    /// </summary>
    public string? PhoneNumber { get; set; }
}
