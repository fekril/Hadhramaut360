namespace Had360back.Dtos.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public string Email { get; set; } = null!;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public int UserId { get; set; }
}
