using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIdDictAllGrantTypes.Web.Models;

namespace OpenIdDictAllGrantTypes.Web.Seeds;

public class TestDataSeed
{
    public static async Task SeedOpenIdDicit(IOpenIddictApplicationManager openIddictApplicationManager)
    {
        if (await openIddictApplicationManager.FindByClientIdAsync("client2") is null)
            await openIddictApplicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "client2",
                ClientSecret = "secret",
                DisplayName = "client 2",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "microservice1.read",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "microservice1.write"
                }
            });
    }

    public static async Task SeedIdentity(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (!userManager.Users.Any())
        {
            var newUser = new AppUser { UserName = "admin", Email = "admin@outlook.com" };

            await roleManager.CreateAsync(new AppRole { Name = "Manager" });
            await roleManager.CreateAsync(new AppRole { Name = "Editor" });

            await userManager.CreateAsync(newUser, "Password12*");
            await userManager.AddToRoleAsync(newUser, "Manager");
            await userManager.AddToRoleAsync(newUser, "Editor");
        }
    }
}