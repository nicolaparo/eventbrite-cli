using EventbriteCli.Api;

namespace EventbriteCli.Tests.Api;

public sealed class QueryParametersTests
{
    [Fact]
    public void AppendToSkipsEmptyValuesAndEscapes()
    {
        var query = new QueryParameters().Add("name_filter", "rock & roll").Add("status", null).Add("expand", " ");

        Assert.Equal("events/?name_filter=rock%20%26%20roll", query.AppendTo("events/"));
    }

    [Fact]
    public void WithReplacesExistingValueWithoutMutatingOriginal()
    {
        var original = new QueryParameters().Add("page", "1").Add("status", "live");

        var next = original.With("page", "2");

        Assert.Equal("x/?page=1&status=live", original.AppendTo("x/"));
        Assert.Equal("x/?status=live&page=2", next.AppendTo("x/"));
    }

    [Fact]
    public void AppendToUsesAmpersandWhenPathHasQuery()
    {
        Assert.Equal("x/?a=1&b=2", new QueryParameters().Add("b", "2").AppendTo("x/?a=1"));
    }
}
