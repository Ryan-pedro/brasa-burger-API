using Brasa.Api.Auth;
using Brasa.Api.Endpoints.Admin;
using Brasa.Api.Endpoints.Store;
using Brasa.Application;
using Brasa.Infrastructure;
using Brasa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Respostas de erro no formato Problem Details (RFC 9457).
builder.Services.AddProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(provider =>
    provider.GetRequiredService<IConfiguration>().GetConnectionString("Database")
    ?? throw new InvalidOperationException("A string de conexão 'Database' não está configurada."));

// Sessão do painel: cookie inacessível ao JavaScript, preso ao host.
builder.Services
    .AddAuthentication()
    .AddCookie(AdminAuth.Scheme);

builder.Services
    .AddOptions<CookieAuthenticationOptions>(AdminAuth.Scheme)
    .Configure<IHostEnvironment>((options, environment) =>
    {
        // Desenvolvimento e testes rodam em HTTP; nos demais ambientes o cookie exige HTTPS.
        var plainHttp = environment.IsDevelopment() || environment.IsEnvironment("Testing");

        options.Cookie.Name = plainHttp ? AdminAuth.DevelopmentCookieName : AdminAuth.SecureCookieName;
        options.Cookie.SecurePolicy = plainHttp ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";

        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;

        // É uma API: sem redirecionamento para página de login, só o código de status.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AdminAuth.Scheme, policy => policy
        .AddAuthenticationSchemes(AdminAuth.Scheme)
        .RequireAuthenticatedUser());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Somente em desenvolvimento: aplica as migrations e cria as lojas e os usuários de demonstração.
    await DevelopmentDatabase.MigrateAndSeedAsync(app.Services);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

// Toda a API fica sob /api/v1. O TLS é terminado no proxy de borda, não aqui.
var api = app.MapGroup("/api/v1");

api.MapGet("/health", () => TypedResults.Ok(new HealthResponse("ok", DateTimeOffset.UtcNow)))
    .WithName("GetHealth");

api.MapStoreEndpoints();
api.MapAdminEndpoints();

app.Run();

internal sealed record HealthResponse(string Status, DateTimeOffset CheckedAt);
