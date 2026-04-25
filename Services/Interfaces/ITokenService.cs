using Had360back.Models;

namespace Had360back.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, IList<string> roles);
}
