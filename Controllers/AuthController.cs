using Had360back.Dtos.Auth;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// تسجيل حساب مستخدم عادي جديد مع إمكانية رفع صورة شخصية اختيارية.
    /// </summary>
    /// <param name="dto">بيانات التسجيل للمستخدم</param>
    /// <returns>نتيجة عملية التسجيل وتوكن الدخول</returns>
    [HttpPost("register-user")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegisterUser([FromForm] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterUserAsync(dto);

        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok(new { result.Message, result.Data });
    }

    /// <summary>
    /// تسجيل حساب مؤسسة جديد مع إمكانية رفع شعار للمؤسسة.
    /// </summary>
    /// <param name="dto">بيانات التسجيل للمؤسسة</param>
    /// <returns>نتيجة عملية التسجيل وتوكن الدخول للمؤسسة</returns>
    [HttpPost("register-organization")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegisterOrganization([FromForm] RegisterOrganizationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterOrganizationAsync(dto);

        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok(new { result.Message, result.Data });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
            return Unauthorized(new { result.Message });

        return Ok(new { result.Message, result.Data });
    }
}
