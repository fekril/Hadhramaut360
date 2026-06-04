using System.Collections.Generic;

namespace Had360back.Dtos.Account;

/// <summary>
/// Profile data returned by GET /api/account/me and after any profile update.
/// Does NOT expose password hashes or security-sensitive Identity fields.
/// </summary>
public class AccountProfileDto
{
    public int          UserId          { get; set; }
    public string       FullName        { get; set; } = null!;
    public string?      Email           { get; set; }
    public string?      PhoneNumber     { get; set; }
    public string?      ProfileImageUrl { get; set; }
    public List<string> Roles           { get; set; } = new();
}
