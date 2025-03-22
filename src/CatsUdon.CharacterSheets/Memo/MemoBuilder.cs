using Microsoft.Extensions.ObjectPool;
using System.Text;

namespace CatsUdon.CharacterSheets.Memo;

public class MemoBuilder : IDisposable
{
    private static readonly ObjectPool<StringBuilder> stringBuilderPool = ObjectPool.Create(new DefaultPooledObjectPolicy<StringBuilder>());

    private readonly StringBuilder stringBuilder = stringBuilderPool.Get();
    private bool disposedValue;

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

    public MemoBuilder IfNotEmpty(string? text, string textToWrite)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return this;
        }

        return Text(textToWrite);
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                stringBuilder.Clear();
                stringBuilderPool.Return(stringBuilder);
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
