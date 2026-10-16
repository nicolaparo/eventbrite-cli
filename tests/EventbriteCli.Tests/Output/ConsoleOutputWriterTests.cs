using System.Text.Json;
using EventbriteCli.Output;

namespace EventbriteCli.Tests.Output;

public sealed class ConsoleOutputWriterTests
{
    private static readonly IReadOnlyList<Column> Columns = [new("ID", "id"), new("NAME", "name.text")];

    private static readonly JsonElement[] Items =
    [
        JsonDocument.Parse("""{"id":"1","name":{"text":"Short"}}""").RootElement,
        JsonDocument.Parse("""{"id":"22","name":{"text":"Multi\nline"}}""").RootElement,
    ];

    [Fact]
    public async Task WriteItemsAsTableAlignsColumnsAsync()
    {
        using var writer = new StringWriter();

        await new ConsoleOutputWriter(writer).WriteItemsAsync(Items, Columns, OutputFormat.Table, TestContext.Current.CancellationToken);

        var lines = writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["ID  NAME", "1   Short", "22  Multi line", "(2 items)"], lines);
    }

    [Fact]
    public async Task WriteItemsAsJsonKeepsCompletePayloadAsync()
    {
        using var writer = new StringWriter();

        await new ConsoleOutputWriter(writer).WriteItemsAsync(Items, Columns, OutputFormat.Json, TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(writer.ToString());
        Assert.Equal(2, document.RootElement.GetArrayLength());
        Assert.Equal("Multi\nline", document.RootElement[1].GetProperty("name").GetProperty("text").GetString());
    }

    [Fact]
    public async Task WriteItemAsTableShowsFieldValueRowsAsync()
    {
        using var writer = new StringWriter();

        await new ConsoleOutputWriter(writer).WriteItemAsync(Items[0], Columns, OutputFormat.Table, TestContext.Current.CancellationToken);

        var lines = writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["FIELD  VALUE", "ID     1", "NAME   Short"], lines);
    }
}
