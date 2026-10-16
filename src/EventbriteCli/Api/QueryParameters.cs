using System.Text;

namespace EventbriteCli.Api;

/// <summary>Query string builder that ignores empty values.</summary>
internal sealed class QueryParameters
{
    private readonly List<KeyValuePair<string, string>> parameters = [];

    public static QueryParameters Empty => new();

    public QueryParameters Add(string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add(new(name, value));
        }

        return this;
    }

    public QueryParameters AddRange(string name, IEnumerable<string>? values)
    {
        if (values is not null && values.Any())
        {
            Add(name, string.Join(',', values));
        }

        return this;
    }

    public QueryParameters With(string name, string? value)
    {
        var copy = new QueryParameters();
        copy.parameters.AddRange(parameters.Where(parameter => parameter.Key != name));
        return copy.Add(name, value);
    }

    public string AppendTo(string path)
    {
        if (parameters.Count == 0)
        {
            return path;
        }

        var builder = new StringBuilder(path).Append(path.Contains('?') ? '&' : '?');
        builder.AppendJoin('&', parameters.Select(parameter =>
            $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));
        return builder.ToString();
    }
}
