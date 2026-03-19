using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using CodeCoach.Api.Auth;
using CodeCoach.Application.Extensions;
using CodeCoach.Infrastructure.Extensions;
using CodeCoach.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<AuthCookieOptions>(options =>
{
    options.AccessTokenCookieName =
        builder.Configuration["AuthCookies:AccessTokenCookieName"] ?? options.AccessTokenCookieName;
    options.RefreshTokenCookieName =
        builder.Configuration["AuthCookies:RefreshTokenCookieName"] ?? options.RefreshTokenCookieName;

    if (Enum.TryParse<SameSiteMode>(builder.Configuration["AuthCookies:SameSite"], true, out var sameSite))
    {
        options.SameSite = sameSite;
    }

    if (bool.TryParse(builder.Configuration["AuthCookies:HttpOnly"], out var httpOnly))
    {
        options.HttpOnly = httpOnly;
    }

    if (bool.TryParse(builder.Configuration["AuthCookies:Secure"], out var secure))
    {
        options.Secure = secure;
    }
});
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var signingKey = builder.Configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is missing.");
        var accessTokenCookieName =
            builder.Configuration["AuthCookies:AccessTokenCookieName"] ?? "codecoach_access_token";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            NameClaimType = ClaimTypes.NameIdentifier
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(accessTokenCookieName, out var accessToken) &&
                    !string.IsNullOrWhiteSpace(accessToken))
                {
                    context.Token = accessToken;
                }
                else
                {
                    context.NoResult();
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
