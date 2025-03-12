using System.Text;

namespace CatsUdon.CharacterSheets.TextSheets;

public enum CatsudonTextTypes
{
    Text,
    ResourceController
}

public class TextSheetWriter(CatsudonTextTypes type)
{
    private const string CTStart = "%CTS%";
    private const string CTEnd = "%CTE%";
    private const string CTRCStart = "%CTRCS%";
    private const string CTRCEnd = "%CTRCE%";

    private readonly StringBuilder Builder = new();

    public TextSheetWriter Append<T>(T value)
    {
        Builder.Append($"①{Format(value)}");
        return this;
    }

    public TextSheetWriter Append<T1, T2>(T1 value1, T2 value2)
    {
        Builder.Append($"①{Format(value1)}②{Format(value2)}");
        return this;
    }

    public TextSheetWriter Append<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        Builder.Append($"①{Format(value1)}②{Format(value2)}②{Format(value3)}");
        return this;
    }

    public TextSheetWriter Append<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        Builder.Append($"①{Format(value1)}②{Format(value2)}②{Format(value3)}②{Format(value4)}");
        return this;
    }

    public TextSheetWriter Append<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        Builder.Append($"①{Format(value1)}②{Format(value2)}②{Format(value3)}②{Format(value4)}②{Format(value5)}");
        return this;
    }

    private string Format<T>(T value)
    {
        if (value is bool boolean)
        {
            return boolean ? "True" : "False";
        }

        return value?.ToString() ?? string.Empty;
    }

    public override string ToString()
    {
        return type switch
        {
            CatsudonTextTypes.Text => $"{CTStart}{Builder}{CTEnd}",
            CatsudonTextTypes.ResourceController => $"{CTRCStart}{Builder}{CTRCEnd}",
            _ => throw new InvalidOperationException("Invalid Text Type")
        };
    }
}
