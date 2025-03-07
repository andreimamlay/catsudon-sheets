namespace CatsUdon.CharacterSheets;
public readonly struct Modifier(int value)
{
    public int Value { get; } = value;

    public static implicit operator Modifier(int value) => new(value);
    public static implicit operator int(Modifier modifier) => modifier.Value;

    public override string ToString() => Value switch
    {
        > 0 => $"+{Value}",
        < 0 => Value.ToString(),
        0 => string.Empty
    };
}
