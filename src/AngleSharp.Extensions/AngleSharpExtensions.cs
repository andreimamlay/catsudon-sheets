using AngleSharp.Dom;

namespace AngleSharp.Html.Dom;
public static class AngleSharpExtensions
{
    public static string QuerySelectorText(this IHtmlDocument document, string selector)
    {
        var selectedElement = document.QuerySelector(selector);
        if (selectedElement is null)
        {
            return string.Empty;
        }

        return selectedElement.TextContent?.Trim() ?? string.Empty;
    }

    public static string QuerySelectorText(this IElement element, string selector)
    {
        var selectedElement = element.QuerySelector(selector);
        if (selectedElement is null)
        {
            return string.Empty;
        }

        return selectedElement.TextContent?.Trim() ?? string.Empty;
    }


    public static string QuerySelectorValue(this IHtmlDocument document, string selector)
    {
        var selectedElement = document.QuerySelector(selector);
        if (selectedElement is null)
        {
            return string.Empty;
        }

        var text = selectedElement switch
        {
            IHtmlInputElement inputElement => inputElement.Value,
            IHtmlSelectElement selectElement => GetSelectElementValue(selectElement),
            _ => selectedElement.TextContent
        };

        return text?.Trim() ?? string.Empty;
    }

    public static string QuerySelectorValue(this IElement element, string selector)
    {
        var selectedElement = element.QuerySelector(selector);
        if (selectedElement is null)
        {
            return string.Empty;
        }

        var text = selectedElement switch
        {
            IHtmlInputElement inputElement => inputElement.Value,
            IHtmlSelectElement selectElement => GetSelectElementValue(selectElement),
            _ => selectedElement.TextContent
        };

        return text?.Trim() ?? string.Empty;
    }



    private static string GetSelectElementValue(IHtmlSelectElement selectElement)
    {
        return selectElement.Options.FirstOrDefault(o => o.IsSelected)?.Value ?? string.Empty;
    }

    public static int QuerySelectorNumber(this IHtmlDocument document, string selector)
    {
        var textValue = document.QuerySelectorText(selector);
        if (int.TryParse(textValue, out var number))
        {
            return number;
        }

        return 0;
    }

    public static int? QuerySelectorNumberOptional(this IHtmlDocument document, string selector)
    {
        var textValue = document.QuerySelectorText(selector);
        if (int.TryParse(textValue, out var number))
        {
            return number;
        }

        return default;
    }

    public static int QuerySelectorNumber(this IElement element, string selector)
    {
        var textValue = element.QuerySelectorText(selector);
        if (int.TryParse(textValue, out var number))
        {
            return number;
        }

        return 0;
    }

    public static int? QuerySelectorNumberOptional(this IElement element, string selector)
    {
        var textValue = element.QuerySelectorText(selector);
        if (int.TryParse(textValue, out var number))
        {
            return number;
        }

        return default;
    }
}
