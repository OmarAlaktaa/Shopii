using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Shopii.Application;
using Shopii.Infrastructure;
using Shopii.Infrastructure.Persistence.Migrations;
using Shopii.API.Extensions;
using Shopii.API.Services;
using Shopii.Application.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

// Protect the authentication endpoints from brute-force attempts and CPU
// exhaustion caused by repeated password hashing. Each client IP gets its own
// fixed window, and excess requests are rejected immediately (no queueing)
// with a 429 instead of reaching the handlers.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        // JsonSerializer (rather than WriteAsJsonAsync) keeps the PascalCase
        // "Message" property used by the exception handling middleware.
        var payload = JsonSerializer.Serialize(
            new { Message = "Too many requests. Please try again later." });

        await context.HttpContext.Response.WriteAsync(payload, cancellationToken);
    };

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddApplication();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();


var app = builder.Build();

// Apply the hand-authored SQLite migrations before the first request. The
// runner is idempotent and backs up the database before a schema rebuild.
await app.Services.ApplyDatabaseMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto-login while developing: injects a fake user into every request
    // that does NOT carry an Authorization header, so [Authorize] endpoints
    // can be tested without logging in. Never registered in Production.
    // Override the fake identity per request with headers if needed:
    //   X-Dev-User-Id: 00000000-0000-0000-0000-000000000001
    //   X-Dev-Role: Customer
    //   X-Dev-Email: someone@shopii.local
    //
    // The bypass only supplies claims; user-scoped endpoints still read the
    // database. Ensure the configured id has a row, otherwise endpoints such
    // as GET /api/users/me return 404.
    await app.Services.SeedDevelopmentUserAsync(app.Configuration);

    app.UseDevelopmentAuthentication();
}

app.UseExceptionHandling();

app.UseHttpsRedirection();

// Routing must run before the rate limiter so endpoint metadata such as
// [EnableRateLimiting("auth")] is available when the request is limited.
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
