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

    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterUserAsync(dto);

        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok(new { result.Message, result.Data });
    }

    [HttpPost("register-organization")]
    public async Task<IActionResult> RegisterOrganization([FromBody] RegisterOrganizationDto dto)
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
