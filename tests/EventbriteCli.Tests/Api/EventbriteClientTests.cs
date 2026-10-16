using System.Net;
using EventbriteCli.Api;
using EventbriteCli.Tests.Fakes;

namespace EventbriteCli.Tests.Api;

public sealed class EventbriteClientTests
{
    private static (EventbriteClient Client, StubHttpMessageHandler Handler) CreateClient()
    {
        var handler = new StubHttpMessageHandler();
        var client = new EventbriteClient(new HttpClient(handler) { BaseAddress = EventbriteClient.BaseAddress });
        return (client, handler);
    }

    [Fact]
    public async Task GetPagedFollowsContinuationTokensAsync()
    {
        var (client, handler) = CreateClient();
        handler
            .Enqueue("""{"events":[{"id":"1"},{"id":"2"}],"pagination":{"has_more_items":true,"continuation":"abc"}}""")
            .Enqueue("""{"events":[{"id":"3"}],"pagination":{"has_more_items":false}}""");

        var items = await client
            .GetPagedAsync("organizations/9/events/", "events", new QueryParameters().Add("status", "live"), TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["1", "2", "3"], items.Select(item => item.GetProperty("id").GetString()));
        Assert.Equal("https://www.eventbriteapi.com/v3/organizations/9/events/?status=live", handler.Requests[0].AbsoluteUri);
        Assert.Equal("https://www.eventbriteapi.com/v3/organizations/9/events/?status=live&continuation=abc", handler.Requests[1].AbsoluteUri);
    }

    [Fact]
    public async Task GetPagedFallsBackToPageNumberAsync()
    {
        var (client, handler) = CreateClient();
        handler
            .Enqueue("""{"venues":[{"id":"1"}],"pagination":{"has_more_items":true,"page_number":1}}""")
            .Enqueue("""{"venues":[{"id":"2"}],"pagination":{"has_more_items":false,"page_number":2}}""");

        var items = await client
            .GetPagedAsync("organizations/9/venues/", "venues", QueryParameters.Empty, TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, items.Count);
        Assert.EndsWith("venues/?page=2", handler.Requests[1].AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPagedStopsWhenLimitReachedAsync()
    {
        var (client, handler) = CreateClient();
        handler.Enqueue("""{"events":[{"id":"1"},{"id":"2"}],"pagination":{"has_more_items":true,"continuation":"abc"}}""");

        var items = await client
            .GetPagedAsync("organizations/9/events/", "events", QueryParameters.Empty, TestContext.Current.CancellationToken)
            .Take(2)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, items.Count);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetThrowsApiExceptionWithEventbriteErrorAsync()
    {
        var (client, handler) = CreateClient();
        handler.Enqueue("""{"error":"NOT_FOUND","error_description":"The event you requested does not exist.","status_code":404}""", HttpStatusCode.NotFound);

        var exception = await Assert.ThrowsAsync<EventbriteApiException>(
            () => client.GetAsync("events/1/", QueryParameters.Empty, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("NOT_FOUND", exception.Error);
        Assert.Contains("does not exist", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetThrowsApiExceptionOnInvalidJsonAsync()
    {
        var (client, handler) = CreateClient();
        handler.Enqueue("<html>gateway</html>", HttpStatusCode.BadGateway);

        var exception = await Assert.ThrowsAsync<EventbriteApiException>(
            () => client.GetAsync("events/1/", QueryParameters.Empty, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("INVALID_RESPONSE", exception.Error);
    }
}
