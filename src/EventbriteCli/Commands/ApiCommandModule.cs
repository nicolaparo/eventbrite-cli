using System.CommandLine;
using EventbriteCli.Api;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>api get &lt;path&gt;</c>: raw read access to any API v3 endpoint not covered by a dedicated command.</summary>
internal sealed class ApiCommandModule(ResourceReader reader) : ICommandModule
{
    public Command Create()
    {
        var path = new Argument<string>("path") { Description = "Path relative to https://www.eventbriteapi.com/v3/, e.g. users/me/ or events/123/attendees/" };
        var query = new Option<string[]>("--query", "-q")
        {
            Description = "Query parameter as name=value (repeatable).",
            AllowMultipleArgumentsPerToken = false,
        };
        var items = new Option<string?>("--items")
        {
            Description = "Name of the list property (e.g. events) to follow pagination and merge all pages.",
        };
        var limit = CommonOptions.Limit();

        var get = new Command("get", "GET any Eventbrite API v3 endpoint and print the JSON response.") { path, query, items, limit };
        get.SetHandler((parseResult, cancellationToken) =>
        {
            var relativePath = NormalizePath(parseResult.GetValue(path)!);
            var parameters = ParseQuery(parseResult.GetValue(query) ?? []);
            var itemsProperty = parseResult.GetValue(items);

            return string.IsNullOrWhiteSpace(itemsProperty)
                ? reader.ShowItemAsync(relativePath, parameters, [], OutputFormat.Json, cancellationToken)
                : reader.ShowListAsync(relativePath, itemsProperty, parameters, [], OutputFormat.Json, parseResult.GetValue(limit), cancellationToken);
        });

        return new Command("api", "Raw read-only access to the Eventbrite API.") { get };
    }

    /// <summary>
    /// Rejects absolute URLs (the bearer token must never leave the Eventbrite host) and appends the trailing slash
    /// Eventbrite expects, since a redirect would drop the Authorization header.
    /// </summary>
    internal static string NormalizePath(string path)
    {
        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        var (resource, queryString) = queryIndex < 0 ? (trimmed, string.Empty) : (trimmed[..queryIndex], trimmed[queryIndex..]);

        if (resource.Contains(':', StringComparison.Ordinal) || resource.StartsWith("//", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Pass a path relative to the API base address, not an absolute URL.");
        }

        resource = resource.TrimStart('/');
        if (resource.StartsWith("v3/", StringComparison.OrdinalIgnoreCase))
        {
            resource = resource[3..];
        }

        return (resource.EndsWith('/') ? resource : resource + "/") + queryString;
    }

    internal static QueryParameters ParseQuery(IEnumerable<string> pairs)
    {
        var parameters = new QueryParameters();
        foreach (var pair in pairs)
        {
            var separator = pair.IndexOf('=', StringComparison.Ordinal);
            if (separator <= 0)
            {
                throw new InvalidOperationException($"Invalid query parameter '{pair}', expected name=value.");
            }

            parameters.Add(pair[..separator], pair[(separator + 1)..]);
        }

        return parameters;
    }
}
