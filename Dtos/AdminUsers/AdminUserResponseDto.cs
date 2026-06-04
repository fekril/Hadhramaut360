using System.Collections.Generic;

namespace Had360back.Dtos.AdminUsers;

/// <summary>
/// Frontend-focused user response DTO for admin user management.
/// Does NOT expose password hashes or any security-sensitive fields.
/// </summary>
public class AdminUserResponseDto
{
    public int             UserId          { get; set; }
    public string?         UserName        { get; set; }
    public string?         Email           { get; set; }
    public string?         PhoneNumber     { get; set; }
    public string?         ProfileImageUrl { get; set; }
    public List<string>    Roles           { get; set; } = new();
}
