using System.Text.Json;

namespace EventbriteCli.Output;

/// <summary>
/// Writes JSON (the complete API payload) or a compact table (the configured columns).
/// A single item rendered as a table is shown as field/value rows.
/// </summary>
internal sealed class ConsoleOutputWriter(TextWriter writer) : IOutputWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task WriteItemAsync(JsonElement item, IReadOnlyList<Column> columns, OutputFormat format, CancellationToken cancellationToken)
    {
        if (format == OutputFormat.Json || columns.Count == 0)
        {
            await WriteJsonAsync(item, cancellationToken);
            return;
        }

        var rows = columns.Select(column => new[] { column.Header, column.ValueOf(item) }).ToList();
        await WriteTableAsync(["FIELD", "VALUE"], rows, cancellationToken);
    }

    public async Task WriteItemsAsync(IReadOnlyList<JsonElement> items, IReadOnlyList<Column> columns, OutputFormat format, CancellationToken cancellationToken)
    {
        if (format == OutputFormat.Json || columns.Count == 0)
        {
            await WriteJsonAsync(items, cancellationToken);
            return;
        }

        var rows = items.Select(item => columns.Select(column => column.ValueOf(item)).ToArray()).ToList();
        await WriteTableAsync([.. columns.Select(column => column.Header)], rows, cancellationToken);
        await writer.WriteLineAsync($"({items.Count} item{(items.Count == 1 ? string.Empty : "s")})".AsMemory(), cancellationToken);
    }

    private async Task WriteJsonAsync<T>(T value, CancellationToken cancellationToken) =>
        await writer.WriteLineAsync(JsonSerializer.Serialize(value, JsonOptions).AsMemory(), cancellationToken);

    private async Task WriteTableAsync(string[] headers, IReadOnlyList<string[]> rows, CancellationToken cancellationToken)
    {
        var widths = headers
            .Select((header, index) => rows.Select(row => Flatten(row[index]).Length).Append(header.Length).Max())
            .ToArray();

        await WriteRowAsync(headers, widths, cancellationToken);
        foreach (var row in rows)
        {
            await WriteRowAsync(row, widths, cancellationToken);
        }
    }

    private async Task WriteRowAsync(string[] cells, int[] widths, CancellationToken cancellationToken)
    {
        var line = string.Join("  ", cells.Select((cell, index) => Flatten(cell).PadRight(widths[index]))).TrimEnd();
        await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
    }

    private static string Flatten(string value) => value.ReplaceLineEndings(" ");
}
