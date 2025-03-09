using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets;
public readonly partial struct Die
{
    [GeneratedRegex(@"^(?<count>\d+)d(?<sides>\d+)(?<modifier>(\+|-)\d+)?$")]
    private static partial Regex DieRegex { get; }

    public static bool TryParse(string input, [NotNullWhen(true)] out Die? die)
    {
        die = default;

        var match = DieRegex.Match(input);
        if (!match.Success)
        {
            return false;
        }

        var count = int.Parse(match.Groups["count"].Value);
        var sides = int.Parse(match.Groups["sides"].Value);
        Modifier? modifier = match.Groups["modifier"].Success
            ? int.Parse(match.Groups["modifier"].Value)
            : default;
        
        die = new Die()
        {
            Count = count,
            Sides = sides,
            Modifier = modifier
        };

        return true;
    }
    
    public int Count { get; init; }
    public int Sides { get; init; }
    public Modifier? Modifier { get; init; }


    public override string ToString()
    {
        if (Modifier.HasValue)
        {
            return $"{Count}d{Sides}{Modifier}";
        }

        return $"{Count}d{Sides}";
    }
}
