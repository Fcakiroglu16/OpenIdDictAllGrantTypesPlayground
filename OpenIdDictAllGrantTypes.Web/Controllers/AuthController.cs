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
    private readonly IOpenIddictApplicationManager _applicationManager;

    public AuthController(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new ArgumentNullException("HttpContext.GetOpenIddictServerRequest()");

        if (!request.IsClientCredentialsGrantType())
            throw new NotImplementedException("The specified grant type is not implemented.");

        
        
        
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
        if (application == null)
        {
            throw new InvalidOperationException("The application details cannot be found in the database.");
        }
        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Add the claims that will be persisted in the tokens (use the client_id as the subject identifier).
        identity.AddClaim(OpenIddictConstants.Claims.Subject, await _applicationManager.GetClientIdAsync(application));
        identity.AddClaim(OpenIddictConstants.Claims.Name, await _applicationManager.GetDisplayNameAsync(application));
        // Set the list of scopes granted to the client application in access_token.
        identity.SetScopes(request.GetScopes());
        identity.SetResources("resource-microservice1");
        identity.SetDestinations(_=> new string[]{ OpenIddictConstants.Destinations.AccessToken});
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }


}