using System.Net.Http;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using CodeCoach.Api.Controllers;
using CodeCoach.Infrastructure.Data;

namespace CodeCoach.Api.Tests.Integration;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<RoomsController>
{
    private readonly string _databaseName = $"codecoach-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Port=5432;Database=codecoach-tests;Username=postgres;Password=postgres",
                ["Jwt:Issuer"] = "CodeCoach.Tests",
                ["Jwt:Audience"] = "CodeCoach.Tests.Client",
                ["Jwt:SigningKey"] = "codecoach-tests-signing-key-1234567890",
                ["Jwt:ExpirationMinutes"] = "60",
                ["RefreshTokens:ExpirationDays"] = "7",
                ["AuthCookies:AccessTokenCookieName"] = AuthClientTestHelper.AccessTokenCookieName,
                ["AuthCookies:RefreshTokenCookieName"] = AuthClientTestHelper.RefreshTokenCookieName,
                ["AuthCookies:SameSite"] = "Lax",
                ["AuthCookies:HttpOnly"] = "true",
                ["AuthCookies:Secure"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        });
    }

    public HttpClient CreateApiClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    public async Task<TResult> ExecuteDbContextAsync<TResult>(
        Func<ApplicationDbContext, Task<TResult>> action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(dbContext);
    }

    public async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(dbContext);
    }
}
