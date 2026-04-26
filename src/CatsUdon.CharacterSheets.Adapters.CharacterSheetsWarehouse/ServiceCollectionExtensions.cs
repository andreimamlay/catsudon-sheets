using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.Adapters.CSA;
using Microsoft.Extensions.DependencyInjection;

namespace CatsUdon.CharacterSheets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCharacterSheetsWarehouse(this IServiceCollection services)
    {
        services.AddSingleton<ICharacterSheetAdapter, ShinobigamiAdapter>();
        services.AddSingleton<ICharacterSheetAdapter, KillDeathBusinessAdapter>();
        services.AddSingleton<ICharacterSheetAdapter, MagicalogiaAdapter>();

        return services;
    }
}
