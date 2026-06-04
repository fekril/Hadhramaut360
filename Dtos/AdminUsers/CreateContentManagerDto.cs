using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.Dtos.AdminUsers;

/// <summary>
/// Input DTO for creating a new ContentManager account.
/// Used by Admin via POST /api/admin/users/content-managers.
/// FullName is the display name for the account owner.
/// UserName is generated internally — frontend does not supply it.
/// </summary>
public class CreateContentManagerDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

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
