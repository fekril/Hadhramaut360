namespace Had360back.Services.Interfaces;

/// <summary>
/// سيرفس للوصول إلى بيانات المستخدم الحالي من JWT Token.
/// يُحقن في أي سيرفس يحتاج إلى معرفة هوية المستخدم الحالي.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// يُرجع معرّف المستخدم الحالي المستخرج من JWT claims.
    /// يُرجع null إذا كان المستخدم غير مُسجّل دخوله.
    /// </summary>
    int? GetUserId();

    /// <summary>
    /// يُرجع true إذا كان المستخدم الحالي مُسجّل دخوله.
    /// يُرجع false للمستخدم الضيف/العام.
    /// </summary>
    bool IsAuthenticated();
}
