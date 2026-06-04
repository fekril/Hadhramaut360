using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات تحديث ملف ميديا موجود باستبداله بملف جديد.
/// يُستخدم في PUT /events/media/{mediaId} بصيغة multipart/form-data.
/// </summary>
public class UpdateEventMediaDto
{
    /// <summary>
    /// الملف الجديد الذي سيحل محل الملف القديم. مطلوب. الحد الأقصى للحجم 10MB.
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;
}
