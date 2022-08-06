using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIdDictAllGrantTypes.Web.Extensions;
using OpenIdDictAllGrantTypes.Web.Models;
using OpenIdDictAllGrantTypes.Web.Seeds;
using UdemyIdentity.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppIdentityDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration["ConnectionStrings:Default"]);
    opts.UseOpenIddict();
});

builder.Services.AddOpenIdDictWithExtension();
builder.Services.AddIdentityWithExtension();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var openIddictApplicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    await TestDataSeed.SeedIdentity(userManager,roleManager);
    await TestDataSeed.SeedOpenIdDicit(openIddictApplicationManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();