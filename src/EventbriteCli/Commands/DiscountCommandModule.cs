using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>discounts</c>: promo codes and discounts of your organization.</summary>
internal sealed class DiscountCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private static readonly IReadOnlyList<Column> Columns =
    [
        new("ID", "id"), new("CODE", "code"), new("TYPE", "type"), new("AMOUNT OFF", "amount_off"), new("PERCENT OFF", "percent_off"),
        new("USED", "quantity_sold"), new("AVAILABLE", "quantity_available"), new("EVENT", "event_id"), new("END", "end_date"),
    ];

    public Command Create() => new("discounts", "Discounts and promo codes.")
    {
        factory.List(
            "list",
            "List the discounts of an organization.",
            factory.Organization(org => $"organizations/{org}/discounts/"),
            "discounts",
            Columns,
            QueryOption.Create("scope", "event, multi_events or user (required by the API).", required: true),
            QueryOption.Create("event_id", "Event id (required when --scope event)."),
            QueryOption.Create("code_filter", "Only codes containing this text."),
            QueryOption.Create("type", "coded, access, public or hold.")),
        factory.Get("get", "Show a discount.", PathSource.ById("discount-id", "Discount id", id => $"discounts/{id}/"), Columns),
    };
}
