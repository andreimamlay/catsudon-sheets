using System.Diagnostics;

namespace CatsUdon.CharacterSheets.CCFolia;

[DebuggerDisplay("{Label} {Value}")]
public class CCFoliaParameter
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
