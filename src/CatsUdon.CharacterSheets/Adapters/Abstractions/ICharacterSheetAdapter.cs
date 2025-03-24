using CatsUdon.CharacterSheets.CCFolia;

namespace CatsUdon.CharacterSheets.Adapters.Abstractions;

public interface ICharacterSheetAdapter
{
    bool CanConvert(string url);
    Task<CharacterSheet> Convert(string url);
    GameSystemInfo[] SupportedGameSystems { get; }
}
