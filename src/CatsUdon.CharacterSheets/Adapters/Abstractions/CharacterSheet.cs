using CatsUdon.CharacterSheets.CCFolia;

namespace CatsUdon.CharacterSheets.Adapters.Abstractions;

public class CharacterSheet
{
    public required CCFoliaCharacter CCFoliaCharacter { get; set; }
    public List<string> AdditionalTextSheets { get; set; } = [];
}
