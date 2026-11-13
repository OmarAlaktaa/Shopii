using Serilog;
using Shopii.API.Extensions;
using Shopii.API.Options;
using Shopii.API.Services;
using Shopii.Application;
using Shopii.Application.Interfaces.Services;
using Shopii.Infrastructure;
using Shopii.Infrastructure.Persistence.Migrations;

// Stage 1: bootstrap logger. This exists before configuration is available,
// so startup failures are never silent.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Shopii API");

    var builder = WebApplication.CreateBuilder(args);

    // Stage 2: replace the bootstrap logger with the fully configured one.
    builder.AddShopiiSerilog();

    builder.Services.AddControllers();
    builder.Services.AddShopiiHealthChecks();
    builder.Services.AddShopiiRateLimiting(builder.Configuration);

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<ICorrelationContext, HttpCorrelationContext>();

    builder.Services
        .AddOptions<AuditOptions>()
        .Bind(builder.Configuration.GetSection(AuditOptions.SectionName))
        .Validate(
            options => options.RetentionDays is >= 0,
            "Audit:RetentionDays must be configured and zero or greater.")
        .ValidateOnStart();

    builder.Services.AddHostedService<AuditLogRetentionService>();

    var app = builder.Build();

    // Apply the hand-authored SQLite migrations before the first request. The
    // runner is idempotent and backs up the database before a schema rebuild.
    await app.Services.ApplyDatabaseMigrationsAsync();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerDocumentation();

        // Auto-login while developing: injects a fake user into every request
        // that does NOT carry an Authorization header. Never registered in
        // Production. See DevelopmentAuthenticationExtensions for overrides.
        await app.Services.SeedDevelopmentUserAsync(app.Configuration);
    }

    app.UseShopiiApiPipeline();

    await app.RunAsync();
}
catch (Exception ex)
{
    // Startup or host failure: the bootstrap logger is still alive here if
    // UseSerilog has not run yet, so this is never silent.
    Log.Fatal(ex, "Shopii API terminated unexpectedly");
}
finally
{
    // Flush buffered events before the process exits.
    Log.CloseAndFlush();
}
