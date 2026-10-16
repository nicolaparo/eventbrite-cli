using System.Text.Json;

namespace EventbriteCli.Output;

/// <summary>Renders API resources to the console.</summary>
internal interface IOutputWriter
{
    Task WriteItemAsync(JsonElement item, IReadOnlyList<Column> columns, OutputFormat format, CancellationToken cancellationToken);

    Task WriteItemsAsync(IReadOnlyList<JsonElement> items, IReadOnlyList<Column> columns, OutputFormat format, CancellationToken cancellationToken);
}
