using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Events;

namespace Had360back.Services.Interfaces;

/// <summary>
/// واجهة سيرفس إدارة الفعاليات.
/// توفر عمليات الجلب، الإنشاء، التعديل، الحذف، وإدارة الميديا الخاصة بالفعاليات.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// يجلب قائمة الفعاليات المتاحة للعموم مع دعم البحث، الفلترة، والتقسيم إلى صفحات.
    /// </summary>
    /// <param name="query">معاملات البحث والفلترة والتقسيم إلى صفحات.</param>
    /// <returns>ApiResponse يحتوي على قائمة منقّحة من EventResponseDto.</returns>
    Task<ApiResponse<PaginatedResponse<EventResponseDto>>> GetAllAsync(EventQueryDto query);

    /// <summary>
    /// يجلب التفاصيل الكاملة لفعالية واحدة حسب رقمها.
    /// </summary>
    /// <param name="id">رقم الفعالية المطلوبة.</param>
    /// <returns>ApiResponse يحتوي على EventDetailsDto أو خطأ 404 إذا لم تكن موجودة.</returns>
    Task<ApiResponse<EventDetailsDto>> GetByIdAsync(int id);

    /// <summary>
    /// يجلب بيانات الفعالية في صيغة التعديل للمستخدمين المصرح لهم.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <returns>ApiResponse يحتوي على EventEditDto أو خطأ 404 أو 403.</returns>
    Task<ApiResponse<EventEditDto>> GetEditAsync(int id);

    /// <summary>
    /// يجلب قائمة الفعاليات التي أنشأتها المنظمة الحالية.
    /// </summary>
    /// <param name="query">معاملات البحث والفلترة والتقسيم إلى صفحات.</param>
    /// <returns>ApiResponse يحتوي على قائمة منقّحة من EventResponseDto.</returns>
    Task<ApiResponse<PaginatedResponse<EventResponseDto>>> GetMyEventsAsync(EventQueryDto query);

    /// <summary>
    /// ينشئ فعالية جديدة بدون ملفات ميديا.
    /// رقم المنظمة يُؤخذ من JWT وليس من الطلب.
    /// </summary>
    /// <param name="dto">بيانات الفعالية الجديدة.</param>
    /// <returns>ApiResponse يحتوي على تفاصيل الفعالية بعد الإنشاء.</returns>
    Task<ApiResponse<EventDetailsDto>> CreateAsync(CreateEventDto dto);

    /// <summary>
    /// ينشئ فعالية جديدة مع رفع ملفات ميديا في نفس الطلب.
    /// يستخدم Transaction لضمان اتساق البيانات.
    /// </summary>
    /// <param name="dto">بيانات الفعالية والملفات المراد رفعها.</param>
    /// <returns>ApiResponse يحتوي على تفاصيل الفعالية الجديدة مع الميديا.</returns>
    Task<ApiResponse<EventDetailsDto>> CreateWithMediaAsync(CreateEventWithMediaDto dto);

    /// <summary>
    /// يحدّث بيانات فعالية موجودة. يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// لا يحدّث الميديا، إدارة الميديا لها نقاط نهاية منفصلة.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <param name="dto">البيانات الجديدة للفعالية.</param>
    /// <returns>ApiResponse يحتوي على التفاصيل المحدّثة أو خطأ 404 أو 403.</returns>
    Task<ApiResponse<EventDetailsDto>> UpdateAsync(int id, UpdateEventDto dto);

    /// <summary>
    /// يحذف فعالية موجودة وجميع ملفات ميديا المرتبطة بها.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <returns>ApiResponse يحتوي على true عند نجاح الحذف أو خطأ 404 أو 403.</returns>
    Task<ApiResponse<bool>> DeleteAsync(int id);

    /// <summary>
    /// يضيف ملف ميديا جديد لفعالية موجودة.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="eventId">رقم الفعالية.</param>
    /// <param name="dto">الملف المراد رفعه.</param>
    /// <returns>ApiResponse يحتوي على EventMediaResponseDto بعد الرفع.</returns>
    Task<ApiResponse<EventMediaResponseDto>> AddMediaAsync(int eventId, AddEventMediaDto dto);

    /// <summary>
    /// يحدّث ملف ميديا موجود باستبداله بملف جديد.
    /// يحذف الملف القديم بعد نجاح تحديث قاعدة البيانات.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا.</param>
    /// <param name="dto">الملف الجديد.</param>
    /// <returns>ApiResponse يحتوي على EventMediaResponseDto بعد التحديث.</returns>
    Task<ApiResponse<EventMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateEventMediaDto dto);

    /// <summary>
    /// يحذف ملف ميديا من قاعدة البيانات ومن التخزين الفعلي.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا.</param>
    /// <returns>ApiResponse يحتوي على true عند نجاح الحذف.</returns>
    Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId);
}
