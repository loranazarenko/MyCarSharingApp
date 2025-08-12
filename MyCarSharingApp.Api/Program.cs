using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCarSharingApp.Api.Helpers;
using MyCarSharingApp.Api.Middleware;
using MyCarSharingApp.Application.Interfaces;     
using MyCarSharingApp.Application.Mappers;
using MyCarSharingApp.Application.Services;
using MyCarSharingApp.Infrastructure;
using MyCarSharingApp.Infrastructure.Repositories;
using Serilog;
using Serilog.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Add logging and configuration
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration);
});

// Add services
// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity 
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(opts => {
        opts.Password.RequireDigit = true;
        opts.Password.RequiredLength = 6;
        opts.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

/*builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = "role";
});*/
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = "role"
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Cars
builder.Services.AddScoped<ICarMapper, MyCarSharingApp.Infrastructure.EF.Mappers.CarMapper>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();

// Users
builder.Services.AddScoped<IUserService, IdentityUserService>();

// Rentals
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IUnitOfWork, MyCarSharingApp.Infrastructure.UnitOfWork>();

// JWT generator
builder.Services.AddScoped<JwtTokenGenerator>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@ex.com";
    var adminUser = await userMgr.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser { UserName = "admin", Email = adminEmail, EmailConfirmed = true };
        var createRes = await userMgr.CreateAsync(adminUser, "Aa_123456");
        if (createRes.Succeeded)
            await userMgr.AddToRoleAsync(adminUser, "Admin");
    }

    if (adminUser == null)
    {
        adminUser = new IdentityUser { UserName = "admin", Email = adminEmail, EmailConfirmed = true };
        var createRes = await userMgr.CreateAsync(adminUser, "Aa_123456");
        if (createRes.Succeeded)
        {
            await userMgr.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            Serilog.Log.Error("Admin creation failed: {Errors}",
                string.Join(", ", createRes.Errors.Select(e => e.Description)));
        }
    }
}

// Middleware
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.Use(async (context, next) =>
{
    var user = context.User;
    if (user?.Identity != null)
    {
        Log.Information("----- Authenticated? {IsAuth}", user.Identity.IsAuthenticated);
        var identitiesInfo = user.Identities.Select((id, idx) => new {
            Index = idx,
            AuthType = id.AuthenticationType,
            IsAuthenticated = id.IsAuthenticated,
            NameClaimType = id.NameClaimType,
            RoleClaimType = id.RoleClaimType,
            Claims = id.Claims.Select(c => $"{c.Type}=>{c.Value}").ToArray()
        }).ToArray();
        foreach (var idInfo in identitiesInfo)
        {
            Log.Information("Identity #{Index} AuthType={AuthType} IsAuth={IsAuthenticated} NameClaimType={NameClaimType} RoleClaimType={RoleClaimType}",
                idInfo.Index, idInfo.AuthType, idInfo.IsAuthenticated, idInfo.NameClaimType, idInfo.RoleClaimType);
            Log.Information("Identity #{Index} Claims: {Claims}", idInfo.Index, string.Join(", ", idInfo.Claims));
        }
    }
    else
    {
        Log.Information("No user identity present");
    }

    await next();
});

app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();
