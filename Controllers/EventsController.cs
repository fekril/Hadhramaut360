using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Events;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Controller مسؤول عن إدارة جميع واجهات API الخاصة بالفعاليات.
/// يوفر عمليات العرض العام، عرض التفاصيل، إنشاء الفعاليات، التعديل، الحذف، وإدارة ميديا الفعاليات.
/// </summary>
[ApiController]
[Route(ApiRoutes.Events.Base)]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// يجلب قائمة جميع الفعاليات المتاحة للعموم.
    /// يدعم البحث، الفلترة بالتاريخ، فلترة القادمة فقط، والتقسيم إلى صفحات.
    /// </summary>
    /// <param name="query">
    /// معاملات الاستعلام: Search، FromDate، ToDate، UpcomingOnly، PageNumber، PageSize.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة منقّحة من EventResponseDto مع بيانات الصفحات.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    /// 
    /// يستخدم هذا الـ Endpoint لعرض صفحة الفعاليات العامة في الفرونت إند.
    /// كل فعالية تحتوي على صورة الغلاف المأخوذة من أول صورة مرتبطة بها واسم المنظمة وشعارها.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<EventResponseDto>>>> GetAll(
        [FromQuery] EventQueryDto query)
    {
        var result = await _eventService.GetAllAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب التفاصيل الكاملة لفعالية واحدة.
    /// </summary>
    /// <param name="id">رقم الفعالية المطلوبة.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على EventDetailsDto عند العثور على الفعالية.
    /// يرجع 404 إذا لم توجد الفعالية.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    /// 
    /// يستخدم هذا الـ Endpoint في صفحة تفاصيل الفعالية.
    /// يتضمن كامل بيانات الفعالية، جميع الصور، وبيانات المنظمة الكاملة.
    /// </remarks>
    [HttpGet(ApiRoutes.Events.ById)]
    public async Task<ActionResult<ApiResponse<EventDetailsDto>>> GetById(int id)
    {
        var result = await _eventService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب بيانات الفعالية في صيغة التعديل للمستخدمين المصرح لهم.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على EventEditDto.
    /// يرجع 404 إذا لم توجد الفعالية.
    /// يرجع 403 إذا لم يكن المستخدم مصرحًا له بتعديل الفعالية.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Authenticated - صاحب الفعالية أو الأدمن أو مدير المحتوى
    /// 
    /// يستخدم هذا الـ Endpoint لتحميل بيانات صفحة التعديل.
    /// لا يحتوي على بيانات المنظمة الكاملة.
    /// </remarks>
    [Authorize]
    [HttpGet(ApiRoutes.Events.Edit)]
    public async Task<ActionResult<ApiResponse<EventEditDto>>> GetEdit(int id)
    {
        var result = await _eventService.GetEditAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب قائمة الفعاليات التي أنشأتها المنظمة الحالية.
    /// </summary>
    /// <param name="query">معاملات البحث والفلترة والصفحات.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة منقّحة من EventResponseDto الخاصة بالمنظمة مع بيانات الصفحات.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: OrganizationOnly
    /// 
    /// يستخدم هذا الـ Endpoint في لوحة تحكم المنظمة لعرض فعالياتها.
    /// يرتب النتائج تنازليًا حسب تاريخ الإنشاء.
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.OrganizationOnly)]
    [HttpGet(ApiRoutes.Events.My)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<EventResponseDto>>>> GetMyEvents(
        [FromQuery] EventQueryDto query)
    {
        var result = await _eventService.GetMyEventsAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// ينشئ فعالية جديدة بدون ملفات ميديا.
    /// </summary>
    /// <param name="dto">بيانات الفعالية الجديدة بصيغة JSON.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل الفعالية الجديدة بعد الإنشاء.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: OrganizationOnly
    /// Body Type: JSON
    /// 
    /// رقم المنظمة يُؤخذ تلقائيًا من JWT ولا يُقبل من الطلب.
    /// الفعالية تظهر فورًا للعموم بعد الإنشاء.
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.OrganizationOnly)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<EventDetailsDto>>> Create([FromBody] CreateEventDto dto)
    {
        var result = await _eventService.CreateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// ينشئ فعالية جديدة مع رفع ملفات ميديا في نفس الطلب.
    /// </summary>
    /// <param name="dto">بيانات الفعالية والملفات بصيغة multipart/form-data.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل الفعالية الجديدة مع الميديا المرفوعة.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: OrganizationOnly
    /// Content-Type: multipart/form-data
    /// 
    /// الملفات اختيارية. الحد الأقصى 10 ملفات، كل ملف 10MB كحد أقصى.
    /// يستخدم Transaction لضمان اتساق البيانات في حالة الفشل.
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.OrganizationOnly)]
    [HttpPost(ApiRoutes.Events.WithMedia)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<EventDetailsDto>>> CreateWithMedia(
        [FromForm] CreateEventWithMediaDto dto)
    {
        var result = await _eventService.CreateWithMediaAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحدّث بيانات فعالية موجودة.
    /// </summary>
    /// <param name="id">رقم الفعالية المراد تعديلها.</param>
    /// <param name="dto">البيانات الجديدة للفعالية بصيغة JSON.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل الفعالية بعد التعديل.
    /// يرجع 404 إذا لم توجد الفعالية.
    /// يرجع 403 إذا لم يكن المستخدم مصرحًا له.
    /// </returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Authenticated - صاحب الفعالية أو الأدمن أو مدير المحتوى
    /// Body Type: JSON
    /// 
    /// يحدّث البيانات النصية والتواريخ فقط. إدارة الميديا لها نقاط نهاية منفصلة.
    /// </remarks>
    [Authorize]
    [HttpPut(ApiRoutes.Events.ById)]
    public async Task<ActionResult<ApiResponse<EventDetailsDto>>> Update(
        int id, [FromBody] UpdateEventDto dto)
    {
        var result = await _eventService.UpdateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف فعالية موجودة وجميع ملفات ميديا المرتبطة بها.
    /// </summary>
    /// <param name="id">رقم الفعالية المراد حذفها.</param>
    /// <returns>
    /// يرجع ApiResponse بقيمة true عند نجاح الحذف.
    /// يرجع 404 إذا لم توجد الفعالية.
    /// يرجع 403 إذا لم يكن المستخدم مصرحًا له.
    /// </returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Authenticated - صاحب الفعالية أو الأدمن أو مدير المحتوى
    /// 
    /// يحذف الفعالية من قاعدة البيانات وجميع ملفات الميديا من التخزين الفعلي.
    /// </remarks>
    [Authorize]
    [HttpDelete(ApiRoutes.Events.ById)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _eventService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يضيف ملف ميديا جديد لفعالية موجودة.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <param name="dto">الملف المراد رفعه بصيغة multipart/form-data.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على بيانات الميديا المضافة برمز 201.
    /// يرجع 404 إذا لم توجد الفعالية.
    /// يرجع 403 إذا لم يكن المستخدم مصرحًا له.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Authenticated - صاحب الفعالية أو الأدمن أو مدير المحتوى
    /// Content-Type: multipart/form-data
    /// 
    /// يرفع ملف صورة واحد ويربطه بالفعالية.
    /// </remarks>
    [Authorize]
    [HttpPost(ApiRoutes.Events.AddMedia)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<EventMediaResponseDto>>> AddMedia(
        int id, [FromForm] AddEventMediaDto dto)
    {
        var result = await _eventService.AddMediaAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحدّث ملف ميديا موجود باستبداله بملف جديد.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا المراد تحديثه.</param>
    /// <param name="dto">الملف الجديد بصيغة multipart/form-data.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على بيانات الميديا بعد التحديث.
    /// يرجع 404 إذا لم يوجد سجل الميديا.
    /// يرجع 403 إذا لم يكن المستخدم مصرحًا له.
    /// </returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Authenticated - صاحب الفعالية أو الأدمن أو مدير المحتوى
    /// Content-Type: multipart/form-data
    /// 
    /// يحفظ الملف الجديد أولًا، يحدّث الرابط في قاعدة البيانات، ثم يحذف الملف القديم.
    /// </remarks>
    [Authorize]
    [HttpPut(ApiRoutes.Events.UpdateMedia)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<EventMediaResponseDto>>> UpdateMedia(
        int mediaId, [FromForm] UpdateEventMediaDto dto)
    {
        var result = await _eventService.UpdateMediaAsync(mediaId, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف ملف ميديا من الفعالية من قاعدة البيانات ومن التخزين الفعلي.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا المراد حذفه.</param>
    /// <returns>
    /// يرجع ApiResponse بقيمة true عند نجاح الحذف.
    /// يرجع 404 إذا لم يوجد سجل الميديا.
    /// يرجع 403 إذا لم يكن المستخدم مصرحًا له.
    /// </returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Authenticated - صاحب الفعالية أو الأدمن أو مدير المحتوى
    /// 
    /// يحذف سجل الميديا من قاعدة البيانات ثم يحذف الملف الفعلي من التخزين.
    /// </remarks>
    [Authorize]
    [HttpDelete(ApiRoutes.Events.DeleteMedia)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMedia(int mediaId)
    {
        var result = await _eventService.DeleteMediaAsync(mediaId);
        return StatusCode(result.StatusCode, result);
    }
}
