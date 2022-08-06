using System.ComponentModel.DataAnnotations;

namespace OpenIdDictAllGrantTypes.Web.Models;

public class SigninViewModel
{
    [EmailAddress] public string Email { get; set; }

    [DataType(DataType.Password)] public string Password { get; set; }

    public bool RememberMe { get; set; }
}