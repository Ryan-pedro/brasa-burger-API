using Brasa.Api.Auth;
using Brasa.Api.Endpoints.Admin;
using Brasa.Api.Endpoints.Agent;
using Brasa.Api.Endpoints.Development;
using Brasa.Api.Endpoints.Kitchen;
using Brasa.Api.Endpoints.Store;
using Brasa.Api.Endpoints.Webhooks;
using Brasa.Api.Health;
using Brasa.Api.Http;
using Brasa.Api.Jobs;
using Brasa.Api.Observability;
using Brasa.Api.Operations;
using Brasa.Api.Realtime;
using Brasa.Application.Ordering.Management;
using Brasa.Application.Payments.Online;
using Brasa.Application.Printing.Agents;
using Brasa.Application.Printing.Jobs;
using Brasa.Application.Privacy;
using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Observability;
using Brasa.Application.Shared.Outbox;
using Brasa.Application.Shared.Realtime;
using Brasa.Application;
using Brasa.Infrastructure.Payments;
using Brasa.Infrastructure.Persistence;
using Brasa.Infrastructure.Storage;
using Brasa.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();

builder.Services.AddOpenApi();

// Respostas de erro no formato Problem Details (RFC 9457).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ConcurrencyExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(provider =>
    provider.GetRequiredService<IConfiguration>().GetConnectionString("Database")
    ?? throw new InvalidOperationException("A string de conexão 'Database' não está configurada."));

builder.Services.PostConfigure<ImageStorageOptions>(options =>
    options.Path = Path.Combine(builder.Environment.ContentRootPath, options.Path));

builder.Services.AddSignalR();
builder.Services.AddSingleton<IRealtimeNotifier, HubNotifier>();
builder.Services.AddPublicRateLimits(builder.Configuration);
builder.Services.AddTrustedReverseProxy(builder.Configuration);
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

if (builder.Configuration.GetValue(JobRegistration.EnabledSetting, true))
{
    builder.Services.AddJob<DispatchOutboxJob>(provider => provider.GetRequiredService<OutboxSignal>());
    builder.Services.AddJob<RejectStaleOrdersJob>(TimeSpan.FromMinutes(1));
    builder.Services.AddJob<PurgeExpiredRecordsJob>(TimeSpan.FromHours(1));
    builder.Services.AddJob<RecoverPrintJobsJob>(TimeSpan.FromSeconds(10));
    builder.Services.AddJob<MarkSilentAgentsOfflineJob>(TimeSpan.FromSeconds(30));
    builder.Services.AddJob<RemoveExpiredPersonalDataJob>(TimeSpan.FromHours(6));
    builder.Services.AddJob<ProcessPaymentWebhooksJob>(provider => provider.GetRequiredService<PaymentWebhookSignal>());
    builder.Services.AddJob<ReconcilePixPaymentsJob>(TimeSpan.FromMinutes(1));
    builder.Services.AddJob<SampleOperationalMetricsJob>(TimeSpan.FromSeconds(30));
}

// Sessão do painel: cookie inacessível ao JavaScript, preso ao host.
builder.Services
    .AddAuthentication()
    .AddCookie(AdminAuth.Scheme)
    .AddScheme<AuthenticationSchemeOptions, AgentAuthenticationHandler>(AgentAuth.Scheme, configureOptions: null);

builder.Services.AddSingleton<IAgentTokens, AgentTokens>();

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
        .RequireAuthenticatedUser())
    .AddPolicy(AgentAuth.Scheme, policy => policy
        .AddAuthenticationSchemes(AgentAuth.Scheme)
        .RequireAuthenticatedUser());

var usesFakePixGateway = builder.Configuration[$"{PixGatewayOptions.Section}:Provider"] == PixGatewayOptions.FakeProvider;
var isDevelopmentOrTesting = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing");
if (usesFakePixGateway && !isDevelopmentOrTesting)
{
    throw new InvalidOperationException("O gateway falso de Pix só pode ser usado em desenvolvimento e testes.");
}

var app = builder.Build();

if (await OperationCommands.TryRunAsync(app, args))
{
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Somente em desenvolvimento: aplica as migrations e cria as lojas e os usuários de demonstração.
    await DevelopmentDatabase.MigrateAndSeedAsync(app.Services);
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

var images = app.Services.GetRequiredService<IOptions<ImageStorageOptions>>().Value;
Directory.CreateDirectory(images.Path);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(images.Path),
    RequestPath = ImageStorageOptions.PublicPath,
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        context.Context.Response.Headers.XContentTypeOptions = "nosniff";
    },
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Toda a API fica sob /api/v1. O TLS é terminado no proxy de borda, não aqui.
var api = app.MapGroup("/api/v1");

api.MapGet("/health", () => TypedResults.Ok(new HealthResponse("ok", DateTimeOffset.UtcNow)))
    .WithName("GetHealth");

api.MapHealthChecks("/health/ready").WithName("GetReadiness");

api.MapStoreEndpoints();
api.MapAdminEndpoints();
api.MapKitchenEndpoints();
api.MapAgentEndpoints();
api.MapWebhookEndpoints();

if (isDevelopmentOrTesting)
{
    api.MapDevelopmentEndpoints();
}
api.MapRealtimeHubs();

app.Run();

internal sealed record HealthResponse(string Status, DateTimeOffset CheckedAt);
