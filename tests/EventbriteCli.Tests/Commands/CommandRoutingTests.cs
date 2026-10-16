using EventbriteCli.Api;
using EventbriteCli.Authentication;
using EventbriteCli.Output;
using EventbriteCli.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace EventbriteCli.Tests.Commands;

/// <summary>Runs real command lines through the full DI graph against a stub HTTP handler.</summary>
public sealed class CommandRoutingTests
{
    private const string Api = "https://www.eventbriteapi.com/v3/";

    public static TheoryData<string, string, string?> Routes => new()
    {
        { "me", "users/me/", null },
        { "orgs list", "users/me/organizations/", "organizations" },
        { "orgs members --org 9", "organizations/9/members/", "members" },
        { "events list --org 9 --status live --time-filter current_future -o json", "organizations/9/events/?status=live&time_filter=current_future", "events" },
        { "events get 42 --expand venue,ticket_availability", "events/42/?expand=venue%2Cticket_availability", null },
        { "events description 42", "events/42/description/", null },
        { "events structured-content 42 --purpose listing", "events/42/structured_content/?purpose=listing", null },
        { "events ticket-classes 42", "events/42/ticket_classes/", "ticket_classes" },
        { "events ticket-class 42 7", "events/42/ticket_classes/7/", null },
        { "events questions 42", "events/42/questions/", "questions" },
        { "events capacity 42", "events/42/capacity_tier/", null },
        { "attendees list --event 42 --status attending", "events/42/attendees/?status=attending", "attendees" },
        { "attendees list --org 9 --changed-since 2026-01-01T00:00:00Z", "organizations/9/attendees/?changed_since=2026-01-01T00%3A00%3A00Z", "attendees" },
        { "attendees get 42 5", "events/42/attendees/5/", null },
        { "orders list --org 9", "organizations/9/orders/", "orders" },
        { "orders get 77 --expand attendees", "orders/77/?expand=attendees", null },
        { "venues list --org 9", "organizations/9/venues/", "venues" },
        { "venues get 3", "venues/3/", null },
        { "venues events 3", "venues/3/events/", "events" },
        { "discounts list --org 9 --scope event --event-id 42", "organizations/9/discounts/?scope=event&event_id=42", "discounts" },
        { "webhooks list --org 9", "organizations/9/webhooks/", "webhooks" },
        { "reports sales --event-ids 42", "reports/sales/?event_ids=42", null },
        { "catalog categories list", "categories/", "categories" },
        { "catalog formats get 2", "formats/2/", null },
        { "api get events/42 -q expand=venue", "events/42/?expand=venue", null },
        { "api get events/42/attendees --items attendees", "events/42/attendees/", "attendees" },
        { "events get ../users/me", "events/..%2Fusers%2Fme/", null },
    };

    [Theory]
    [MemberData(nameof(Routes))]
    public async Task CommandRequestsExpectedUrlAsync(string commandLine, string expectedPath, string? itemsProperty)
    {
        var handler = new StubHttpMessageHandler().Enqueue(itemsProperty is null ? "{}" : $$"""{"{{itemsProperty}}":[]}""");
        var (exitCode, error) = await RunAsync(handler, commandLine);

        Assert.Equal(string.Empty, error);
        Assert.Equal(0, exitCode);
        Assert.Equal(Api + expectedPath, Assert.Single(handler.Requests).AbsoluteUri);
    }

    [Fact]
    public async Task OrganizationDefaultsToOnlyOrganizationAsync()
    {
        var handler = new StubHttpMessageHandler()
            .Enqueue("""{"organizations":[{"id":"9","name":"Mine"}]}""")
            .Enqueue("""{"events":[]}""");

        var (exitCode, _) = await RunAsync(handler, "events list");

        Assert.Equal(0, exitCode);
        Assert.Equal(Api + "organizations/9/events/", handler.Requests[1].AbsoluteUri);
    }

    [Fact]
    public async Task AmbiguousOrganizationFailsWithChoicesAsync()
    {
        var handler = new StubHttpMessageHandler().Enqueue("""{"organizations":[{"id":"1","name":"A"},{"id":"2","name":"B"}]}""");

        var (exitCode, error) = await RunAsync(handler, "events list");

        Assert.Equal(1, exitCode);
        Assert.Contains("1 (A), 2 (B)", error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MissingListPropertyFailsWithAvailablePropertiesAsync()
    {
        var handler = new StubHttpMessageHandler().Enqueue("""{"items":[],"pagination":{}}""");

        var (exitCode, error) = await RunAsync(handler, "venues list --org 9");

        Assert.Equal(1, exitCode);
        Assert.Contains("no 'venues' list. Available properties: items.", error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApiGetRejectsForeignHostAsync()
    {
        var handler = new StubHttpMessageHandler();

        var (exitCode, error) = await RunAsync(handler, "api get https:/evil.example/x");

        Assert.Equal(1, exitCode);
        Assert.NotEqual(string.Empty, error);
        Assert.Empty(handler.Requests);
    }

    private static async Task<(int ExitCode, string Error)> RunAsync(StubHttpMessageHandler handler, string commandLine)
    {
        var services = new ServiceCollection().AddEventbriteCli();
        services.AddSingleton<ITokenProvider>(new FixedTokenProvider());
        services.AddSingleton<IOutputWriter>(new ConsoleOutputWriter(TextWriter.Null));
        services.AddHttpClient<IEventbriteClient, EventbriteClient>().ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var provider = services.BuildServiceProvider();

        using var error = new StringWriter();
        var parseResult = RootCommandBuilder.Build(provider).Parse(commandLine);
        parseResult.InvocationConfiguration.Error = error;
        parseResult.InvocationConfiguration.Output = TextWriter.Null;

        var exitCode = await parseResult.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (exitCode, error.ToString().Trim());
    }

    private sealed class FixedTokenProvider : ITokenProvider
    {
        public Task<string?> GetTokenAsync(CancellationToken cancellationToken) => Task.FromResult<string?>("test-token");
    }
}
