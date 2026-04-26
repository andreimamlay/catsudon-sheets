using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.Adapters.DndJp;
using Microsoft.Extensions.DependencyInjection;

namespace CatsUdon.CharacterSheets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDndJp(this IServiceCollection services)
    {
        services.AddSingleton<ICharacterSheetAdapter, DndJpAdapter>();

        return services;
    }
}
