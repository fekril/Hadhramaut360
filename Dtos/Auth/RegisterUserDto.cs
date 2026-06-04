using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.Dtos.Auth;

public class RegisterUserDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = null!;

    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Optional profile image file for the user account.
    /// </summary>
    public IFormFile? ProfileImageFile { get; set; }
}
