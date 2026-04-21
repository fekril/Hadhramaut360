using Had360back.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Had360back.Data
{
    public class DbContextApp : IdentityDbContext<User,Role,int>
    {

    }
}
