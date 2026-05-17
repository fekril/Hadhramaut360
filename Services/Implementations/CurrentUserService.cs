using System.Security.Claims;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Had360back.Services.Implementations;

/// <summary>
/// يستخرج معلومات المستخدم الحالي من JWT claims عبر IHttpContextAccessor.
/// لا يقرأ userId من الـ request body أو query string أو route.
/// معرّف المستخدم يأتي فقط من JWT claims.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// يُرجع معرّف المستخدم من claim النوع NameIdentifier.
    /// يُرجع null إذا كان المستخدم غير مُسجّل دخوله أو إذا لم يكن الـ claim موجودًا أو غير قابل للتحويل.
    /// لا يرمي استثناء في حالة المستخدم الضيف.
    /// </summary>
    public int? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            return null;

        return int.TryParse(claim.Value, out var userId) ? userId : null;
    }

    /// <summary>
    /// يُرجع true إذا كان المستخدم الحالي مُسجّل دخوله عبر JWT.
    /// يُرجع false للمستخدم الضيف أو إذا لم يكن هناك HttpContext.
    /// </summary>
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }
}
