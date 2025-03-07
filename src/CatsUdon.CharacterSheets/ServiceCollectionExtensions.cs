using CatsUdon.CharacterSheets.Adapters;
using CatsUdon.CharacterSheets.Adapters.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CatsUdon.CharacterSheets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCharacterSheets(this IServiceCollection services)
    {
        services.AddSingleton<ICharacterSheetAdapter, DndJp>();
        return services;
    }
}
