using System.Diagnostics;

namespace CatsUdon.CharacterSheets.CCFolia;


[DebuggerDisplay("{Label} {Value} / {Max}")]
public class CCFoliaStatus
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Max { get; set; }
}
