using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.Events;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// تنفيذ سيرفس إدارة الفعاليات.
/// يوفر منطق الأعمال لجميع عمليات الفعاليات وميديا الفعاليات.
/// </summary>
public class EventService : IEventService
{
    private readonly DbContextApp _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly UserManager<User> _userManager;

    public EventService(
        DbContextApp context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        UserManager<User> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// يُرجع معرّف المستخدم الحالي أو -1 إذا كان غير مُسجّل دخوله.
    /// </summary>
    private int GetCurrentUserId() => _currentUserService.GetUserId() ?? -1;

    /// <summary>
    /// يتحقق إذا كان المستخدم الحالي في أحد الأدوار المعطاة.
    /// </summary>
    private async Task<bool> CurrentUserIsInRoleAsync(params string[] roles)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue) return false;

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null) return false;

        foreach (var role in roles)
        {
            if (await _userManager.IsInRoleAsync(user, role))
                return true;
        }
        return false;
    }

    /// <summary>
    /// يتحقق إذا كان المستخدم الحالي يملك صلاحية إدارة الفعالية.
    /// الأدمن ومدير المحتوى يملكون الصلاحية دائمًا.
    /// المنظمة تملك الصلاحية فقط إذا كانت هي صاحبة الفعالية.
    /// </summary>
    private async Task<bool> CanManageEventAsync(Event eventEntity)
    {
        if (await CurrentUserIsInRoleAsync(IdentityRoles.Admin, IdentityRoles.ContentManager))
            return true;

        var currentUserId = GetCurrentUserId();
        return eventEntity.OrgUserId == currentUserId;
    }

    /// <summary>
    /// يُطبّق منطق تطبيع قيم الصفحة لمنع القيم غير الصالحة.
    /// </summary>
    private static (int pageNumber, int pageSize) NormalizePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        return (pageNumber, pageSize);
    }

    // ─── Mapping ────────────────────────────────────────────────────────────────

    /// <summary>
    /// يحوّل كائن EventMedia إلى EventMediaResponseDto.
    /// </summary>
    private static EventMediaResponseDto MapToMediaResponse(EventMedia media) => new()
    {
        MediaId = media.MediaId,
        EventId = media.EventId,
        MediaUrl = media.MediaUrl,
        MediaType = media.MediaType,
        CreatedAt = media.CreatedAt
    };

    /// <summary>
    /// يحوّل كائن Event إلى EventResponseDto مختصر للقوائم.
    /// صورة الغلاف تُحسب من أول عنصر في EventMedia مرتبًا بـ MediaId تصاعديًا.
    /// </summary>
    private static EventResponseDto MapToEventResponse(Event ev) => new()
    {
        EventId = ev.EventId,
        TitleAr = ev.TitleAr,
        TitleEn = ev.TitleEn,
        AddressAr = ev.AddressAr,
        AddressEn = ev.AddressEn,
        StartDate = ev.StartDate,
        EndDate = ev.EndDate,
        CoverImageUrl = ev.EventMedia.OrderBy(m => m.MediaId).FirstOrDefault()?.MediaUrl,
        OrganizationUserId = ev.OrgUserId,
        OrganizationNameAr = ev.Organization.OrgNameAr,
        OrganizationNameEn = ev.Organization.OrgNameEn,
        OrganizationLogoUrl = ev.Organization.LogoUrl
    };

    /// <summary>
    /// يحوّل كائن Organization إلى EventOrganizationDto.
    /// رقم الهاتف يُؤخذ من Organization.User.PhoneNumber.
    /// </summary>
    private static EventOrganizationDto MapToOrganizationDto(Organization org) => new()
    {
        UserId = org.UserId,
        OrgNameAr = org.OrgNameAr,
        OrgNameEn = org.OrgNameEn,
        LogoUrl = org.LogoUrl,
        DescriptionAr = org.DescriptionAr,
        DescriptionEn = org.DescriptionEn,
        AddressAr = org.AddressAr,
        AddressEn = org.AddressEn,
        MapUrl = org.MapUrl,
        PhoneNumber = org.User?.PhoneNumber
    };

    /// <summary>
    /// يحوّل كائن Event إلى EventDetailsDto كامل.
    /// </summary>
    private static EventDetailsDto MapToEventDetails(Event ev) => new()
    {
        EventId = ev.EventId,
        TitleAr = ev.TitleAr,
        TitleEn = ev.TitleEn,
        DescriptionAr = ev.DescriptionAr,
        DescriptionEn = ev.DescriptionEn,
        AddressAr = ev.AddressAr,
        AddressEn = ev.AddressEn,
        MapUrl = ev.MapUrl,
        FormUrl = ev.FormUrl,
        StartDate = ev.StartDate,
        EndDate = ev.EndDate,
        CreatedAt = ev.CreatedAt,
        UpdatedAt = ev.UpdatedAt,
        Media = ev.EventMedia.OrderBy(m => m.MediaId).Select(MapToMediaResponse).ToList(),
        Organization = MapToOrganizationDto(ev.Organization)
    };

    /// <summary>
    /// يحوّل كائن Event إلى EventEditDto لصفحة التعديل.
    /// </summary>
    private static EventEditDto MapToEventEdit(Event ev) => new()
    {
        EventId = ev.EventId,
        TitleAr = ev.TitleAr,
        TitleEn = ev.TitleEn,
        DescriptionAr = ev.DescriptionAr,
        DescriptionEn = ev.DescriptionEn,
        AddressAr = ev.AddressAr,
        AddressEn = ev.AddressEn,
        MapUrl = ev.MapUrl,
        FormUrl = ev.FormUrl,
        StartDate = ev.StartDate,
        EndDate = ev.EndDate,
        Media = ev.EventMedia.OrderBy(m => m.MediaId).Select(MapToMediaResponse).ToList()
    };

    // ─── Pagination Helper ───────────────────────────────────────────────────────

    private static PaginatedResponse<EventResponseDto> BuildPaginatedResponse(
        List<EventResponseDto> dtos, int totalEntries, int currentPage, int perPage)
    {
        var totalPages = perPage > 0 ? (int)Math.Ceiling((double)totalEntries / perPage) : 0;
        return new PaginatedResponse<EventResponseDto>
        {
            Items = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage = currentPage,
                NextPage = currentPage < totalPages ? (int?)(currentPage + 1) : null,
                PerPage = perPage,
                PreviousPage = currentPage > 1 ? (int?)(currentPage - 1) : null,
                TotalEntries = totalEntries,
                TotalPages = totalPages
            }
        };
    }

    // ─── Query Builder ───────────────────────────────────────────────────────────

    /// <summary>
    /// يبني استعلام IQueryable مشترك لتطبيق فلاتر البحث والتاريخ على الفعاليات.
    /// </summary>
    private IQueryable<Event> BuildFilteredQuery(EventQueryDto query)
    {
        var dbQuery = _context.Set<Event>()
            .Include(e => e.Organization)
                .ThenInclude(o => o.User)
            .Include(e => e.EventMedia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var lowerSearch = query.Search.ToLower();
            dbQuery = dbQuery.Where(e =>
                e.TitleAr.ToLower().Contains(lowerSearch) ||
                e.TitleEn.ToLower().Contains(lowerSearch) ||
                e.DescriptionAr.ToLower().Contains(lowerSearch) ||
                e.DescriptionEn.ToLower().Contains(lowerSearch) ||
                e.AddressAr.ToLower().Contains(lowerSearch) ||
                e.AddressEn.ToLower().Contains(lowerSearch) ||
                e.Organization.OrgNameAr.ToLower().Contains(lowerSearch) ||
                e.Organization.OrgNameEn.ToLower().Contains(lowerSearch));
        }

        if (query.FromDate.HasValue)
            dbQuery = dbQuery.Where(e => e.StartDate >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            dbQuery = dbQuery.Where(e => e.StartDate <= query.ToDate.Value);

        if (query.UpcomingOnly == true)
            dbQuery = dbQuery.Where(e => e.EndDate >= DateTime.UtcNow);

        return dbQuery;
    }

    // ─── Public Methods ──────────────────────────────────────────────────────────

    /// <summary>
    /// يجلب قائمة الفعاليات للعموم مع دعم البحث والفلترة والتقسيم إلى صفحات.
    /// يرتب النتائج تصاعديًا حسب تاريخ البداية.
    /// </summary>
    /// <param name="query">معاملات الاستعلام والفلترة والصفحات.</param>
    /// <returns>ApiResponse يحتوي على قائمة منقّحة من EventResponseDto.</returns>
    public async Task<ApiResponse<PaginatedResponse<EventResponseDto>>> GetAllAsync(EventQueryDto query)
    {
        var (pageNumber, pageSize) = NormalizePagination(query.PageNumber, query.PageSize);

        var dbQuery = BuildFilteredQuery(query);

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderBy(e => e.StartDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToEventResponse).ToList();

        return ApiResponse<PaginatedResponse<EventResponseDto>>.Ok(
            BuildPaginatedResponse(dtos, totalItems, pageNumber, pageSize));
    }

    /// <summary>
    /// يجلب التفاصيل الكاملة لفعالية واحدة حسب رقمها.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <returns>ApiResponse يحتوي على EventDetailsDto أو خطأ 404.</returns>
    public async Task<ApiResponse<EventDetailsDto>> GetByIdAsync(int id)
    {
        var ev = await _context.Set<Event>()
            .Include(e => e.Organization)
                .ThenInclude(o => o.User)
            .Include(e => e.EventMedia)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev == null)
            return ApiResponse<EventDetailsDto>.Fail("Event not found.", null, 404);

        return ApiResponse<EventDetailsDto>.Ok(MapToEventDetails(ev));
    }

    /// <summary>
    /// يجلب بيانات الفعالية في صيغة التعديل للمستخدمين المصرح لهم.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <returns>ApiResponse يحتوي على EventEditDto أو خطأ 404 أو 403.</returns>
    public async Task<ApiResponse<EventEditDto>> GetEditAsync(int id)
    {
        var ev = await _context.Set<Event>()
            .Include(e => e.EventMedia)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev == null)
            return ApiResponse<EventEditDto>.Fail("Event not found.", null, 404);

        if (!await CanManageEventAsync(ev))
            return ApiResponse<EventEditDto>.Fail("You are not authorized to edit this event.", null, 403);

        return ApiResponse<EventEditDto>.Ok(MapToEventEdit(ev));
    }

    /// <summary>
    /// يجلب قائمة الفعاليات التي أنشأتها المنظمة الحالية.
    /// يرتب النتائج تنازليًا حسب تاريخ الإنشاء.
    /// </summary>
    /// <param name="query">معاملات الاستعلام والفلترة والصفحات.</param>
    /// <returns>ApiResponse يحتوي على قائمة منقّحة من EventResponseDto.</returns>
    public async Task<ApiResponse<PaginatedResponse<EventResponseDto>>> GetMyEventsAsync(EventQueryDto query)
    {
        var (pageNumber, pageSize) = NormalizePagination(query.PageNumber, query.PageSize);
        var currentUserId = GetCurrentUserId();

        var dbQuery = BuildFilteredQuery(query)
            .Where(e => e.OrgUserId == currentUserId);

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToEventResponse).ToList();

        return ApiResponse<PaginatedResponse<EventResponseDto>>.Ok(
            BuildPaginatedResponse(dtos, totalItems, pageNumber, pageSize));
    }

    /// <summary>
    /// ينشئ فعالية جديدة بدون ملفات ميديا.
    /// رقم المنظمة يُؤخذ تلقائيًا من JWT ولا يُقبل من الطلب.
    /// </summary>
    /// <param name="dto">بيانات الفعالية الجديدة.</param>
    /// <returns>ApiResponse يحتوي على تفاصيل الفعالية بعد الإنشاء.</returns>
    public async Task<ApiResponse<EventDetailsDto>> CreateAsync(CreateEventDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            return ApiResponse<EventDetailsDto>.Fail("EndDate must be greater than or equal to StartDate.");

        var currentUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var ev = new Event
        {
            OrgUserId = currentUserId,
            TitleAr = dto.TitleAr,
            TitleEn = dto.TitleEn,
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
            AddressAr = dto.AddressAr,
            AddressEn = dto.AddressEn,
            MapUrl = dto.MapUrl,
            FormUrl = dto.FormUrl,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Set<Event>().Add(ev);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(ev.EventId);
    }

    /// <summary>
    /// ينشئ فعالية جديدة مع رفع ملفات ميديا في نفس الطلب.
    /// يستخدم Transaction لضمان اتساق البيانات.
    /// في حالة الفشل يتم التراجع عن العملية وحذف الملفات المرفوعة.
    /// </summary>
    /// <param name="dto">بيانات الفعالية والملفات المراد رفعها.</param>
    /// <returns>ApiResponse يحتوي على تفاصيل الفعالية الجديدة مع الميديا.</returns>
    public async Task<ApiResponse<EventDetailsDto>> CreateWithMediaAsync(CreateEventWithMediaDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            return ApiResponse<EventDetailsDto>.Fail("EndDate must be greater than or equal to StartDate.");

        if (dto.Files != null && dto.Files.Count > 10)
            return ApiResponse<EventDetailsDto>.Fail("Maximum 10 files allowed.");

        var currentUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        using var transaction = await _context.Database.BeginTransactionAsync();
        var savedFileUrls = new List<string>();

        try
        {
            var ev = new Event
            {
                OrgUserId = currentUserId,
                TitleAr = dto.TitleAr,
                TitleEn = dto.TitleEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                AddressAr = dto.AddressAr,
                AddressEn = dto.AddressEn,
                MapUrl = dto.MapUrl,
                FormUrl = dto.FormUrl,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Set<Event>().Add(ev);
            await _context.SaveChangesAsync(); // needed to get EventId

            if (dto.Files != null && dto.Files.Count > 0)
            {
                foreach (var file in dto.Files)
                {
                    var fileUrl = await _fileStorageService.SaveEventMediaAsync(ev.EventId, file);
                    savedFileUrls.Add(fileUrl);

                    _context.Set<EventMedia>().Add(new EventMedia
                    {
                        EventId = ev.EventId,
                        MediaUrl = fileUrl,
                        MediaType = MediaType.Image,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return await GetByIdAsync(ev.EventId);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            foreach (var url in savedFileUrls)
                await _fileStorageService.DeleteFileAsync(url);

            return ApiResponse<EventDetailsDto>.Fail(
                "An error occurred while creating the event. The operation has been rolled back.", null, 500);
        }
    }

    /// <summary>
    /// يحدّث بيانات فعالية موجودة. يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// لا يحدّث الميديا.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <param name="dto">البيانات الجديدة للفعالية.</param>
    /// <returns>ApiResponse يحتوي على التفاصيل المحدّثة أو خطأ 404 أو 403.</returns>
    public async Task<ApiResponse<EventDetailsDto>> UpdateAsync(int id, UpdateEventDto dto)
    {
        var ev = await _context.Set<Event>()
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev == null)
            return ApiResponse<EventDetailsDto>.Fail("Event not found.", null, 404);

        if (!await CanManageEventAsync(ev))
            return ApiResponse<EventDetailsDto>.Fail("You are not authorized to update this event.", null, 403);

        if (dto.EndDate < dto.StartDate)
            return ApiResponse<EventDetailsDto>.Fail("EndDate must be greater than or equal to StartDate.");

        ev.TitleAr = dto.TitleAr;
        ev.TitleEn = dto.TitleEn;
        ev.DescriptionAr = dto.DescriptionAr;
        ev.DescriptionEn = dto.DescriptionEn;
        ev.AddressAr = dto.AddressAr;
        ev.AddressEn = dto.AddressEn;
        ev.MapUrl = dto.MapUrl;
        ev.FormUrl = dto.FormUrl;
        ev.StartDate = dto.StartDate;
        ev.EndDate = dto.EndDate;
        ev.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(ev.EventId);
    }

    /// <summary>
    /// يحذف فعالية موجودة وجميع ملفات ميديا المرتبطة بها.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="id">رقم الفعالية.</param>
    /// <returns>ApiResponse يحتوي على true عند نجاح الحذف أو خطأ 404 أو 403.</returns>
    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var ev = await _context.Set<Event>()
            .Include(e => e.EventMedia)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev == null)
            return ApiResponse<bool>.Fail("Event not found.", null, 404);

        if (!await CanManageEventAsync(ev))
            return ApiResponse<bool>.Fail("You are not authorized to delete this event.", null, 403);

        var mediaUrls = ev.EventMedia.Select(m => m.MediaUrl).ToList();

        _context.Set<Event>().Remove(ev);
        await _context.SaveChangesAsync();

        foreach (var url in mediaUrls)
            await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Event deleted successfully.");
    }

    /// <summary>
    /// يضيف ملف ميديا جديد لفعالية موجودة.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// في حالة فشل حفظ قاعدة البيانات يتم حذف الملف المرفوع.
    /// </summary>
    /// <param name="eventId">رقم الفعالية.</param>
    /// <param name="dto">الملف المراد رفعه.</param>
    /// <returns>ApiResponse يحتوي على EventMediaResponseDto بعد الرفع برمز 201.</returns>
    public async Task<ApiResponse<EventMediaResponseDto>> AddMediaAsync(int eventId, AddEventMediaDto dto)
    {
        var ev = await _context.Set<Event>().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev == null)
            return ApiResponse<EventMediaResponseDto>.Fail("Event not found.", null, 404);

        if (!await CanManageEventAsync(ev))
            return ApiResponse<EventMediaResponseDto>.Fail("You are not authorized to manage media for this event.", null, 403);

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SaveEventMediaAsync(eventId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<EventMediaResponseDto>.Fail(ex.Message);
        }

        var media = new EventMedia
        {
            EventId = eventId,
            MediaUrl = savedFileUrl,
            MediaType = MediaType.Image,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Set<EventMedia>().Add(media);
            await _context.SaveChangesAsync();

            return ApiResponse<EventMediaResponseDto>.Ok(MapToMediaResponse(media), "Media uploaded successfully.", 201);
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<EventMediaResponseDto>.Fail("Failed to save media to database.", null, 500);
        }
    }

    /// <summary>
    /// يحدّث ملف ميديا موجود باستبداله بملف جديد.
    /// يحذف الملف القديم بعد نجاح تحديث قاعدة البيانات.
    /// في حالة فشل الحفظ يحذف الملف الجديد.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا.</param>
    /// <param name="dto">الملف الجديد.</param>
    /// <returns>ApiResponse يحتوي على EventMediaResponseDto بعد التحديث.</returns>
    public async Task<ApiResponse<EventMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateEventMediaDto dto)
    {
        var media = await _context.Set<EventMedia>()
            .Include(m => m.Event)
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media == null)
            return ApiResponse<EventMediaResponseDto>.Fail("Media not found.", null, 404);

        if (!await CanManageEventAsync(media.Event))
            return ApiResponse<EventMediaResponseDto>.Fail("You are not authorized to manage media for this event.", null, 403);

        var oldUrl = media.MediaUrl;

        string savedFileUrl;
        try
        {
            savedFileUrl = await _fileStorageService.SaveEventMediaAsync(media.EventId, dto.File);
        }
        catch (Exception ex)
        {
            return ApiResponse<EventMediaResponseDto>.Fail(ex.Message);
        }

        media.MediaUrl = savedFileUrl;

        try
        {
            await _context.SaveChangesAsync();
            await _fileStorageService.DeleteFileAsync(oldUrl);

            return ApiResponse<EventMediaResponseDto>.Ok(MapToMediaResponse(media), "Media updated successfully.");
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(savedFileUrl);
            return ApiResponse<EventMediaResponseDto>.Fail("Failed to update media in database.", null, 500);
        }
    }

    /// <summary>
    /// يحذف ملف ميديا من قاعدة البيانات ومن التخزين الفعلي.
    /// يُسمح فقط لصاحب الفعالية أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا.</param>
    /// <returns>ApiResponse يحتوي على true عند نجاح الحذف.</returns>
    public async Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId)
    {
        var media = await _context.Set<EventMedia>()
            .Include(m => m.Event)
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media == null)
            return ApiResponse<bool>.Fail("Media not found.", null, 404);

        if (!await CanManageEventAsync(media.Event))
            return ApiResponse<bool>.Fail("You are not authorized to manage media for this event.", null, 403);

        var url = media.MediaUrl;
        _context.Set<EventMedia>().Remove(media);
        await _context.SaveChangesAsync();

        await _fileStorageService.DeleteFileAsync(url);

        return ApiResponse<bool>.Ok(true, "Media deleted successfully.");
    }
}
