using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OpenIdDictAllGrantTypes.Web.Models;
using UdemyIdentity.Models;

namespace OpenIdDictAllGrantTypes.Web.Extensions;

public static class IdentityExtension
{
    public static void AddOpenIdDictWithExtension(this IServiceCollection collection)
    {
        collection.AddOpenIddict()
            .AddCore(options => { options.UseEntityFrameworkCore().UseDbContext<AppIdentityDbContext>(); })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
             
                options.AllowPasswordFlow();
                options.AcceptAnonymousClients();
                options.SetTokenEndpointUris("/connect/token");
                options.SetUserinfoEndpointUris("/connect/userinfo");
                options.AddEphemeralEncryptionKey();
                options.AddEphemeralSigningKey();
                options.DisableAccessTokenEncryption();
                // options.RegisterScopes("microservice1.read",
                //     "microservice1.write"); // Registers the specified scopes as supported scopes
                options
                    .UseAspNetCore() //Registers the OpenIddict server services for ASP.NET Core in the DI container.
                    .EnableTokenEndpointPassthrough() // OpenID Connect requests are initially handled by OpenIddict.
                    .EnableUserinfoEndpointPassthrough();
                
                
               
            }).AddValidation(options =>
            {
                options.UseLocalServer();
          
            });
    }

    public static void AddIdentityWithExtension(this IServiceCollection collection)
    {
        collection.AddIdentity<AppUser, AppRole>(opts =>
        {
            //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.useroptions.allowedusernamecharacters?view=aspnetcore-2.2

            opts.User.RequireUniqueEmail = true;
            opts.User.AllowedUserNameCharacters =
                "abcçdefgğhıijklmnoçpqrsştuüvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

            opts.Password.RequiredLength = 4;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Password.RequireLowercase = false;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireDigit = false;
        }).AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

        collection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
            {
                opts.LoginPath = new PathString("/Home/Signin");
                opts.LogoutPath = new PathString("/Home/Signout");
                opts.SlidingExpiration = true;
                opts.ExpireTimeSpan = TimeSpan.FromDays(60);
            });
    }
}