using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Had360back.DTOs.Events;

/// <summary>
/// بيانات إنشاء فعالية جديدة مع رفع ملفات ميديا في نفس الطلب.
/// يُستخدم في POST /events/with-media بصيغة multipart/form-data.
/// الملفات اختيارية. الحد الأقصى 10 ملفات، كل ملف 10MB كحد أقصى.
/// </summary>
public class CreateEventWithMediaDto : CreateEventDto
{
    /// <summary>
    /// قائمة الملفات المراد رفعها مع الفعالية. اختيارية. الحد الأقصى 10 ملفات.
    /// </summary>
    public List<IFormFile>? Files { get; set; }
}
