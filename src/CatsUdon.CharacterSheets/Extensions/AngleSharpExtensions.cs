using AngleSharp.Dom;

namespace AngleSharp.Html.Dom;
public static class AngleSharpExtensions
{
    public static string QuerySelectorText(this IHtmlDocument document, string selector)
    {
        return document.QuerySelector(selector)?.TextContent.Trim() ?? string.Empty;
    }

    public static string QuerySelectorText(this IElement element, string selector)
    {
        return element.QuerySelector(selector)?.TextContent.Trim() ?? string.Empty;
    }

    public static int QuerySelectorNumber(this IHtmlDocument document, string selector)
    {
        if (int.TryParse(document.QuerySelector(selector)?.TextContent.Trim(), out var number))
        {
            return number;
        }

        return 0;
    }

    public static int? QuerySelectorNumberOptional(this IHtmlDocument document, string selector)
    {
        if (int.TryParse(document.QuerySelector(selector)?.TextContent.Trim(), out var number))
        {
            return number;
        }

        return default;
    }

    public static int QuerySelectorNumber(this IElement element, string selector)
    {
        if (int.TryParse(element.QuerySelector(selector)?.TextContent.Trim(), out var number))
        {
            return number;
        }

        return 0;
    }

    public static int? QuerySelectorNumberOptional(this IElement element, string selector)
    {
        if (int.TryParse(element.QuerySelector(selector)?.TextContent.Trim(), out var number))
        {
            return number;
        }

        return default;
    }
}
