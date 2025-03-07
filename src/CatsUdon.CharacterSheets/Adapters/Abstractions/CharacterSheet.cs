using CatsUdon.CharacterSheets.CCFolia;

namespace CatsUdon.CharacterSheets.Adapters.Abstractions;

public class CharacterSheet
{
    public required Character CCFoliaCharacter { get; set; }
    public List<string> AdditionalTextSheets { get; set; } = [];
}
