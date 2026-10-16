using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace EventbriteCli.Api;

internal sealed class EventbriteClient(HttpClient httpClient) : IEventbriteClient
{
    public static readonly Uri BaseAddress = new("https://www.eventbriteapi.com/v3/");

    public async Task<JsonElement> GetAsync(string path, QueryParameters query, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(query.AppendTo(path.TrimStart('/')), cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await ParseAsync(stream, cancellationToken);
        var root = document?.RootElement.Clone() ?? default;

        if (!response.IsSuccessStatusCode || document is null)
        {
            throw new EventbriteApiException(
                response.StatusCode,
                GetString(root, "error") ?? (document is null ? "INVALID_RESPONSE" : null),
                GetString(root, "error_description") ?? response.ReasonPhrase);
        }

        return root;
    }

    public async IAsyncEnumerable<JsonElement> GetPagedAsync(
        string path,
        string itemsProperty,
        QueryParameters query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var pageQuery = query;

        while (true)
        {
            var page = await GetAsync(path, pageQuery, cancellationToken);

            if (!page.TryGetProperty(itemsProperty, out var items) || items.ValueKind != JsonValueKind.Array)
            {
                var available = page.EnumerateObject().Select(property => property.Name).Where(name => name != "pagination");
                throw new InvalidOperationException(
                    $"Response of '{path}' has no '{itemsProperty}' list. Available properties: {string.Join(", ", available)}.");
            }

            foreach (var item in items.EnumerateArray())
            {
                yield return item;
            }

            var next = GetNextPageQuery(page, query);
            if (next is null)
            {
                yield break;
            }

            pageQuery = next;
        }
    }

    /// <summary>
    /// Uses <c>has_more_items</c> when present, otherwise <c>page_number &lt; page_count</c>;
    /// prefers the <c>continuation</c> token and falls back to <c>page</c>.
    /// </summary>
    private static QueryParameters? GetNextPageQuery(JsonElement page, QueryParameters query)
    {
        if (!page.TryGetProperty("pagination", out var pagination))
        {
            return null;
        }

        var pageNumber = GetInt32(pagination, "page_number");
        var hasMore = pagination.TryGetProperty("has_more_items", out var hasMoreItems)
            ? hasMoreItems.ValueKind == JsonValueKind.True
            : pageNumber < GetInt32(pagination, "page_count");

        if (!hasMore)
        {
            return null;
        }

        var continuation = GetString(pagination, "continuation");
        if (continuation is not null)
        {
            return query.With("continuation", continuation);
        }

        return pageNumber is { } number
            ? query.With("page", (number + 1).ToString(CultureInfo.InvariantCulture))
            : null;
    }

    private static int? GetInt32(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;

    private static async Task<JsonDocument?> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? GetString(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
