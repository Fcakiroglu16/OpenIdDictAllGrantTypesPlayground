using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIdDictAllGrantTypes.Web.Models;

namespace OpenIdDictAllGrantTypes.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
   
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    public AuthController( UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
    
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new ArgumentNullException("HttpContext.GetOpenIddictServerRequest()");

        if (request.IsPasswordGrantType())

        {
            
              var user = await _userManager.FindByEmailAsync(request.Username);
        if (user == null)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The email or password is invalid."
            });

            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Validate the username/password parameters and ensure the account is not locked out.
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (!result.Succeeded)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "The email or password is invalid."
            });
            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        // Add the claims that will be persisted in the tokens.
        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id)
            .AddClaim(OpenIddictConstants.Claims.Email, user.Email)
            .AddClaim(OpenIddictConstants.Claims.Name, user.UserName)
            .AddClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

        // Set the list of scopes granted to the client application.
        identity.SetScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles, OpenIddictConstants.Scopes.OfflineAccess, "microservice1.read",
            "microservice1.write");
       
        identity.SetResources("resource-microservice1");

        identity.SetDestinations(claim =>
        {
            //Examples
            // if (claim.Type == OpenIddictConstants.Claims.Subject  )
            // {
            //     return new[]
            //         { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken }; 
            // }
            // if (claim.Type == OpenIddictConstants.Claims.Name  )
            // {
            //     return new[]
            //         { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken }; 
            // }

            return new[]
                { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken };
        });

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        
        else if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the device coauthorization code/de/refresh token.
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            // Retrieve the user profile corresponding to the refresh token.
            var user = await _userManager.FindByIdAsync(principal.GetClaim(OpenIddictConstants.Claims.Subject));
            if (user == null)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            principal.SetDestinations(_=>  new[]
                { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken });

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
      
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    { 
        return Ok( await  _userManager.FindByNameAsync(User.Identity.Name));
    }
}