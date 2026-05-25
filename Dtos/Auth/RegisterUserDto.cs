using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.Dtos.Auth;

public class RegisterUserDto
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = null!;

    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// ملف الصورة الشخصية للمستخدم.
    /// </summary>
    public IFormFile? ProfileImageFile { get; set; }
}
