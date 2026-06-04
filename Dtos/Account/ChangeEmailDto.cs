using System.ComponentModel.DataAnnotations;

namespace Had360back.Dtos.Account;

/// <summary>
/// Input DTO for PUT /api/account/email.
/// Requires current password to confirm identity before changing email.
/// </summary>
public class ChangeEmailDto
{
    [Required(ErrorMessage = "New email is required.")]
    [EmailAddress(ErrorMessage = "The email address format is invalid.")]
    public string NewEmail { get; set; } = null!;

    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = null!;
}
