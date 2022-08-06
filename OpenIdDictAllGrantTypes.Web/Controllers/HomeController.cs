using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIdDictAllGrantTypes.Web.Models;

namespace OpenIdDictAllGrantTypes.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Signin()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signin(SigninViewModel request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null) throw new Exception("Your email or password is incorrect");

        var checkPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!checkPassword) throw new Exception("Your email or password is incorrect");

        await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, false);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Signout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}