using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Landmarks;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Controller مسؤول عن إدارة جميع واجهات API الخاصة بالمعالم السياحية والثقافية.
/// يوفر عمليات عرض المعالم، عرض التفاصيل، الإضافة، التعديل، الحذف، وإدارة الصور أو الميديا المرتبطة بالمعالم.
/// </summary>
[ApiController]
[Route(ApiRoutes.Landmarks.Base)]
public class LandmarksController : ControllerBase
{
    private readonly ILandmarkService _landmarkService;

    public LandmarksController(ILandmarkService landmarkService)
    {
        _landmarkService = landmarkService;
    }

    /// <summary>
    /// يجلب قائمة المعالم المتاحة في النظام.
    /// </summary>
    /// <param name="query">
    /// يحتوي على خيارات البحث والفلترة والتقسيم إلى صفحات، مثل كلمة البحث، رقم التصنيف، رقم الصفحة، وحجم الصفحة.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة من LandmarkResponseDto، وهي نسخة مختصرة من بيانات المعالم.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    /// 
    /// يستخدم هذا الـ Endpoint لعرض المعالم في صفحة المعالم العامة.
    /// يمكن استخدامه للبحث عن معلم معين أو فلترة المعالم حسب التصنيف.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<LandmarkResponseDto>>>> GetAll([FromQuery] LandmarkQueryDto query)
    {
        var result = await _landmarkService.GetAllAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب تفاصيل معلم محدد حسب رقم المعلم.
    /// </summary>
    /// <param name="id">
    /// رقم المعلم المراد عرض تفاصيله.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على LandmarkDetailsDto إذا تم العثور على المعلم.
    /// في حالة عدم وجود المعلم يرجع استجابة خطأ برمز 404.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    /// 
    /// يستخدم هذا الـ Endpoint في صفحة تفاصيل المعلم.
    /// يعرض بيانات المعلم الكاملة مثل العنوان، الوصف، الموقع، التصنيفات، والصور المرتبطة به.
    /// </remarks>
    [HttpGet(ApiRoutes.Landmarks.ById)]
    public async Task<ActionResult<ApiResponse<LandmarkDetailsDto>>> GetById(int id)
    {
        var result = await _landmarkService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// ينشئ معلمًا جديدًا بدون رفع صور أو ملفات ميديا.
    /// </summary>
    /// <param name="dto">
    /// بيانات المعلم الجديد مثل العنوان، الوصف، الموقع، رابط الخريطة، والتصنيفات المرتبطة.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل المعلم بعد إنشائه بنجاح.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Requires ManageContent policy
    /// Body Type: JSON
    /// 
    /// هذا الـ Endpoint مخصص للمستخدمين الذين لديهم صلاحية إدارة المحتوى، مثل الأدمن أو مدير المحتوى.
    /// يتم استخدامه عندما تريد إضافة بيانات معلم فقط بدون صور.
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<LandmarkDetailsDto>>> Create([FromBody] CreateLandmarkDto dto)
    {
        var result = await _landmarkService.CreateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// ينشئ معلمًا جديدًا مع رفع الصور أو ملفات الميديا المرتبطة به.
    /// </summary>
    /// <param name="dto">
    /// بيانات المعلم الجديد بالإضافة إلى الملفات المرفوعة من نوع multipart/form-data.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل المعلم الجديد مع التصنيفات والميديا المرتبطة به.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Requires ManageContent policy
    /// Content-Type: multipart/form-data
    /// 
    /// يستخدم هذا الـ Endpoint عند إضافة معلم جديد مع صورة أو أكثر.
    /// يقوم السيرفس بحفظ بيانات المعلم في قاعدة البيانات، ثم حفظ الملفات، ثم ربط روابط الملفات بالمعلم.
    /// </remarks>
    [HttpPost(ApiRoutes.Landmarks.WithMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<LandmarkDetailsDto>>> CreateWithMedia([FromForm] CreateLandmarkWithMediaDto dto)
    {
        var result = await _landmarkService.CreateWithMediaAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// يحدث بيانات معلم موجود حسب رقم المعلم.
    /// </summary>
    /// <param name="id">
    /// رقم المعلم المراد تعديله.
    /// </param>
    /// <param name="dto">
    /// البيانات الجديدة للمعلم، مثل العنوان، الوصف، الموقع، رابط الخريطة، والتصنيفات.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل المعلم بعد التعديل.
    /// في حالة عدم وجود المعلم يرجع خطأ 404.
    /// </returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Requires ManageContent policy
    /// Body Type: JSON
    /// 
    /// يستخدم هذا الـ Endpoint لتعديل البيانات النصية والتصنيفات الخاصة بالمعلم.
    /// لا يستخدم لتحديث الصور، لأن تحديث الصور له Endpoint مستقل.
    /// </remarks>
    [HttpPut(ApiRoutes.Landmarks.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<LandmarkDetailsDto>>> Update(int id, [FromBody] UpdateLandmarkDto dto)
    {
        var result = await _landmarkService.UpdateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// يحذف معلمًا موجودًا حسب رقم المعلم.
    /// </summary>
    /// <param name="id">
    /// رقم المعلم المراد حذفه.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse بقيمة true عند نجاح الحذف.
    /// في حالة عدم وجود المعلم يرجع خطأ 404.
    /// </returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Requires ManageContent policy
    /// 
    /// يستخدم هذا الـ Endpoint لحذف المعلم من قاعدة البيانات.
    /// كما يقوم السيرفس بحذف ملفات الميديا المرتبطة بالمعلم من التخزين.
    /// </remarks>
    [HttpDelete(ApiRoutes.Landmarks.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _landmarkService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// يضيف ملف ميديا جديد إلى معلم موجود.
    /// </summary>
    /// <param name="id">
    /// رقم المعلم الذي سيتم إضافة الميديا له.
    /// </param>
    /// <param name="dto">
    /// يحتوي على الملف المراد رفعه وربطه بالمعلم.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على بيانات الميديا التي تم رفعها.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Requires ManageContent policy
    /// Content-Type: multipart/form-data
    /// 
    /// يستخدم هذا الـ Endpoint لإضافة صورة أو ميديا جديدة لمعلم موجود مسبقًا.
    /// يتم حفظ الملف أولًا، ثم يتم حفظ رابطه في جدول LandmarkMedia.
    /// </remarks>
    [HttpPost(ApiRoutes.Landmarks.AddMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<LandmarkMediaResponseDto>>> AddMedia(int id, [FromForm] AddLandmarkMediaDto dto)
    {
        var result = await _landmarkService.AddMediaAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// يحدث ملف ميديا موجود بمعلم عن طريق استبداله بملف جديد.
    /// </summary>
    /// <param name="mediaId">
    /// رقم الميديا المراد تحديثها.
    /// </param>
    /// <param name="dto">
    /// يحتوي على الملف الجديد الذي سيستبدل الملف القديم.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على بيانات الميديا بعد التحديث.
    /// </returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Requires ManageContent policy
    /// Content-Type: multipart/form-data
    /// 
    /// يستخدم هذا الـ Endpoint لتغيير صورة أو ملف ميديا مرتبط بمعلم.
    /// يقوم السيرفس بحفظ الملف الجديد، تحديث الرابط في قاعدة البيانات، ثم حذف الملف القديم.
    /// </remarks>
    [HttpPut(ApiRoutes.Landmarks.UpdateMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<LandmarkMediaResponseDto>>> UpdateMedia(int mediaId, [FromForm] UpdateLandmarkMediaDto dto)
    {
        var result = await _landmarkService.UpdateMediaAsync(mediaId, dto);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// يحذف ملف ميديا مرتبط بمعلم.
    /// </summary>
    /// <param name="mediaId">
    /// رقم الميديا المراد حذفها.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse بقيمة true عند نجاح حذف الميديا.
    /// في حالة عدم وجود الميديا يرجع خطأ 404.
    /// </returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Requires ManageContent policy
    /// 
    /// يستخدم هذا الـ Endpoint لحذف صورة أو ملف ميديا من قاعدة البيانات ومن التخزين.
    /// </remarks>
    [HttpDelete(ApiRoutes.Landmarks.DeleteMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMedia(int mediaId)
    {
        var result = await _landmarkService.DeleteMediaAsync(mediaId);
        return StatusCode(result.StatusCode, result);
    }
}
