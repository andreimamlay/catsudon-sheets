using CatsUdon.CharacterSheets.Adapters.Abstractions;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters;

public partial class DndJp : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https\:\/\/dndjp.sakura.ne.jp\/OUTPUT.php\?ID=(\d+)$")]
    private static partial Regex UrlMatchRegex { get; }

    public bool CanConvert(string url) => UrlMatchRegex.IsMatch(url);

    public Task<CharacterSheet> Convert(string url)
    {
        throw new NotImplementedException();
    }
}
