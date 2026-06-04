using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات إضافة ملف ميديا لفعالية موجودة.
/// يُستخدم في POST /events/{id}/media بصيغة multipart/form-data.
/// </summary>
public class AddEventMediaDto
{
    /// <summary>
    /// الملف المراد رفعه وربطه بالفعالية. مطلوب. الحد الأقصى للحجم 10MB.
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;
}
