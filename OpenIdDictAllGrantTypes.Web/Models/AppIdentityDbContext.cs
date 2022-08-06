using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIdDictAllGrantTypes.Web.Models;

namespace UdemyIdentity.Models;

public class AppIdentityDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
    {
    }
}