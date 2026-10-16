using System.Text.Json;

namespace EventbriteCli.Api;

/// <summary>Read-only access to the Eventbrite Platform API v3.</summary>
internal interface IEventbriteClient
{
    /// <summary>Gets a single resource. <paramref name="path"/> is relative to the API base address.</summary>
    Task<JsonElement> GetAsync(string path, QueryParameters query, CancellationToken cancellationToken);

    /// <summary>Enumerates the items of a paginated collection, following continuation tokens transparently.</summary>
    IAsyncEnumerable<JsonElement> GetPagedAsync(string path, string itemsProperty, QueryParameters query, CancellationToken cancellationToken);
}
