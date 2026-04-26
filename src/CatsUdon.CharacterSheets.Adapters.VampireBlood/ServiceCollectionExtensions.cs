using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.Adapters.VampireBlood;
using Microsoft.Extensions.DependencyInjection;

namespace CatsUdon.CharacterSheets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVampireBlood(this IServiceCollection services)
    {
        services.AddSingleton<ICharacterSheetAdapter, VampireBloodAdapter>();

        return services;
    }
}
