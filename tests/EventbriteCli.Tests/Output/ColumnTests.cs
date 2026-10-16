using System.Text.Json;
using EventbriteCli.Output;

namespace EventbriteCli.Tests.Output;

public sealed class ColumnTests
{
    private static readonly JsonElement Event = JsonDocument.Parse(
        """{"id":"42","name":{"text":"Meetup"},"capacity":100,"online_event":false,"emails":[{"email":"a@b.c"}],"logo":null}""").RootElement;

    [Theory]
    [InlineData("id", "42")]
    [InlineData("name.text", "Meetup")]
    [InlineData("capacity", "100")]
    [InlineData("online_event", "false")]
    [InlineData("emails.0.email", "a@b.c")]
    [InlineData("emails.1.email", "")]
    [InlineData("logo", "")]
    [InlineData("missing.path", "")]
    public void ValueOfResolvesDottedPaths(string path, string expected)
    {
        Assert.Equal(expected, new Column("H", path).ValueOf(Event));
    }

    [Fact]
    public void ValueOfReturnsRawJsonForObjects()
    {
        Assert.Equal("""{"text":"Meetup"}""", new Column("H", "name").ValueOf(Event));
    }
}
