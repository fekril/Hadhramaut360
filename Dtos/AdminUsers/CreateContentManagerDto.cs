using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.Dtos.AdminUsers;

/// <summary>
/// Input DTO for creating a new ContentManager account.
/// Used by Admin via POST /api/admin/users/content-managers.
/// Profile image is optional; if provided it is saved using the existing
/// user profile image storage method.
/// </summary>
public class CreateContentManagerDto
{
    [Required(ErrorMessage = "User name is required.")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "The email address format is invalid.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = null!;

    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Optional profile image file for the content manager account.
    /// </summary>
    public IFormFile? ProfileImageFile { get; set; }
}
