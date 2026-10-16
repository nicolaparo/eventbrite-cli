using EventbriteCli.Api;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary>Fetches resources from the API and hands them to the output writer.</summary>
internal sealed class ResourceReader(IEventbriteClient client, IOutputWriter output)
{
    public async Task ShowItemAsync(string path, QueryParameters query, IReadOnlyList<Column> columns, OutputFormat format, CancellationToken cancellationToken)
    {
        var item = await client.GetAsync(path, query, cancellationToken);
        await output.WriteItemAsync(item, columns, format, cancellationToken);
    }

    public async Task ShowListAsync(
        string path,
        string itemsProperty,
        QueryParameters query,
        IReadOnlyList<Column> columns,
        OutputFormat format,
        int limit,
        CancellationToken cancellationToken)
    {
        var items = client.GetPagedAsync(path, itemsProperty, query, cancellationToken);
        var page = limit > 0 ? items.Take(limit) : items;
        await output.WriteItemsAsync(await page.ToListAsync(cancellationToken), columns, format, cancellationToken);
    }
}
