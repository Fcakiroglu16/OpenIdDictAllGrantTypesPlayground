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
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AuthController(IOpenIddictScopeManager scopeManager, UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _scopeManager = scopeManager;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new ArgumentNullException("HttpContext.GetOpenIddictServerRequest()");

        if (!request.IsPasswordGrantType())
            throw new NotImplementedException("The specified grant type is not implemented.");

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
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

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
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Add the claims that will be persisted in the tokens.
        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id)
            .AddClaim(OpenIddictConstants.Claims.Email, user.Email)
            .AddClaim(OpenIddictConstants.Claims.Name, user.UserName)
            .AddClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());
        
        // Set the list of scopes granted to the client application.
        identity.SetScopes(new[]
        { OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles,
            "microservice1.read",
            "microservice1.write"
        });

        identity.SetResources(new[] { "resource-microservice1" });

        identity.SetDestinations(claim =>
        {
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
            
         
             return new[] { OpenIddictConstants.Destinations.AccessToken,OpenIddictConstants.Destinations.IdentityToken };
        });

      
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    
    
    
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        return Ok();
        // var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        //
        // return Ok(new
        // {
        //     Name = claimsPrincipal.GetClaim(OpenIddictConstants.Claims.Subject),
        //     Occupation = "Developer",
        //     Age = 43
        // });
    }
    

}