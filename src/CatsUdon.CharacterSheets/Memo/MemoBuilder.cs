using System.Text;

namespace CatsUdon.CharacterSheets.Memo;

public class MemoBuilder
{
    private readonly StringBuilder stringBuilder = new();

    public MemoBuilder BeginSize(int size)
    {
        stringBuilder.Append($"<size={size}>");
        return this;
    }

    public MemoBuilder EndSize()
    {
        stringBuilder.Append($"</size>");
        return this;
    }

    public MemoBuilder NewLine()
    {
        stringBuilder.Append('\n');
        return this;
    }

    public MemoBuilder Text(string text)
    {
        stringBuilder.Append(text);
        return this;
    }

    public MemoBuilder TextPrefixedOptional(string prefix, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return this;
        }

        return Text($"{prefix}：{text}");
    }

    public MemoBuilder Header(string header)
    {
        BeginSize(16);
        stringBuilder.Append($"◆ {header}");
        EndSize();
        NewLine();
        return this;
    }

    public MemoBuilder SubHeader(string header)
    {
        BeginSize(14);
        stringBuilder.Append($"■ {header}");
        EndSize();
        NewLine();
        return this;
    }

    public override string ToString() => stringBuilder.ToString();
}
