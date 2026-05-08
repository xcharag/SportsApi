using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));
       
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, Type openGenericInterface)
    {
        var types = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface)
                .Select(i => new { Interface = i, Implementation = t }));

        foreach (var type in types)
        {
            services.AddScoped(type.Interface, type.Implementation);
        }
    }
}