using System.Globalization;
using System.Text.Json;

namespace EventbriteCli.Output;

/// <summary>A table column bound to a dotted JSON path, e.g. <c>name.text</c> or <c>emails.0.email</c>.</summary>
internal sealed record Column(string Header, string Path)
{
    public string ValueOf(JsonElement element)
    {
        var current = element;
        foreach (var segment in Path.Split('.'))
        {
            if (!TryStep(current, segment, out current))
            {
                return string.Empty;
            }
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString() ?? string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            _ => current.GetRawText(),
        };
    }

    private static bool TryStep(JsonElement current, string segment, out JsonElement next)
    {
        next = default;
        if (current.ValueKind == JsonValueKind.Object)
        {
            return current.TryGetProperty(segment, out next);
        }

        if (current.ValueKind == JsonValueKind.Array
            && int.TryParse(segment, CultureInfo.InvariantCulture, out var index)
            && index >= 0
            && index < current.GetArrayLength())
        {
            next = current[index];
            return true;
        }

        return false;
    }
}
