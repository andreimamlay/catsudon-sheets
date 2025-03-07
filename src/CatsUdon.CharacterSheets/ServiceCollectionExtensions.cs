using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.Adapters.DndJp;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CatsUdon.CharacterSheets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCharacterSheets(this IServiceCollection services)
    {
        var adapters = typeof(ServiceCollectionExtensions).Assembly.GetTypes()
            .Where(t => !t.IsInterface)
            .Where(t => t.IsAssignableTo(typeof(ICharacterSheetAdapter)))
            .ToArray();

        foreach (var adapter in adapters)
        {
            services.AddSingleton(typeof(ICharacterSheetAdapter), adapter);
        }

        return services;
    }
}
