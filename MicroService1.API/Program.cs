using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// builder.Services.Configure<IdentityOptions>(options =>
// {
//     options.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.Name;
//     options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
//     options.ClaimsIdentity.RoleClaimType = "role";
//     // configure more options if necessary...
// });
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["IdentityServerURL"];
    options.Audience = "resource-microservice1";
    options.RequireHttpsMetadata = false;

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
            {
                var scopeClaims = claimsIdentity.FindFirst("scope");
                if (scopeClaims is not null)
                {
                    claimsIdentity.RemoveClaim(scopeClaims);
                    claimsIdentity.AddClaims(scopeClaims.Value.Split(' ').Select(scope => new Claim("scope", scope)));
                }
            }

            await Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("ReadPolicy", policy => { policy.RequireClaim("scope", "microservice1.read"); });

    opts.AddPolicy("WriteOrReadPolicy",
        policy => { policy.RequireClaim("scope", "microservice1.read", "microservice1.write"); });

    opts.AddPolicy("AdminPolicy", policy => { policy.RequireClaim("scope", "microservice1.admin"); });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();