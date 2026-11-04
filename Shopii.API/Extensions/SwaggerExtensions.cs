using Microsoft.OpenApi.Models;

namespace Shopii.API.Extensions
{
    public static class SwaggerExtensions
    {
        // Register Swagger services and configure JWT authentication.
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services)
        {
            // Discover API endpoints for Swagger.
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                // Define JWT Bearer authentication.
                // This adds the Authorize button to Swagger UI.
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description =
                            "Enter your JWT access token."
                    });

                // Tell Swagger to include the Bearer token
                // when executing API requests.
                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new List<string>()
                        }
                    });
            });

            return services;
        }

        // Enable Swagger UI in the Development environment.
        public static WebApplication UseSwaggerDocumentation(
            this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }
    }
}
