using System.CommandLine;
using EventbriteCli.Api;

namespace EventbriteCli.Commands;

/// <summary>A string option forwarded verbatim as a query parameter, e.g. <c>--time-filter</c> to <c>time_filter</c>.</summary>
internal sealed record QueryOption(Option<string?> Option, string Parameter)
{
    public static QueryOption Create(string parameter, string description, bool required = false) =>
        new(new Option<string?>("--" + parameter.Replace('_', '-').Replace('.', '-')) { Description = description, Required = required }, parameter);

    public static QueryParameters Apply(ParseResult parseResult, IEnumerable<QueryOption> options)
    {
        var query = new QueryParameters();
        foreach (var option in options)
        {
            query.Add(option.Parameter, parseResult.GetValue(option.Option));
        }

        return query;
    }
}
