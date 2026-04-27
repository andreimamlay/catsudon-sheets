using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.Adapters.DndBeyond;
using Microsoft.Extensions.DependencyInjection;

namespace CatsUdon.CharacterSheets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDndBeyond(this IServiceCollection services)
    {
        services.AddSingleton<ICharacterSheetAdapter, DndBeyondAdapter>();

        return services;
    }
}
