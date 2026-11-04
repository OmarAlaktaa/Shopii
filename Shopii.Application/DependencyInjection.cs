using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopii.Application.Common.Behaviors;

namespace Shopii.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            // Register every FluentValidation validator found in this assembly
            // and run them through the validation pipeline behavior.
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
