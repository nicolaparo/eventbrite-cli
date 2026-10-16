using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>orders</c>: orders of an event or of the whole organization.</summary>
internal sealed class OrderCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private const string OrderExpansions = "event,attendees,refund_requests";

    private static readonly IReadOnlyList<Column> Columns =
    [
        new("ID", "id"), new("NAME", "name"), new("EMAIL", "email"), new("TOTAL", "costs.gross.display"),
        new("STATUS", "status"), new("EVENT", "event_id"), new("CREATED", "created"),
    ];

    public Command Create() => new("orders", "Orders placed for your events.")
    {
        factory.List(
            "list",
            "List orders of an event (--event) or of the whole organization.",
            factory.EventOrOrganization("orders"),
            "orders",
            Columns,
            QueryOption.Create("status", "active, inactive, both or all_not_deleted."),
            CommonQueryOptions.ChangedSince(),
            QueryOption.Create("only_emails", "Comma separated buyer emails to include."),
            QueryOption.Create("exclude_emails", "Comma separated buyer emails to exclude."),
            CommonQueryOptions.Expand(OrderExpansions)),
        factory.Get("get", "Show an order.", PathSource.ById("order-id", "Order id", id => $"orders/{id}/"), Columns, CommonQueryOptions.Expand(OrderExpansions)),
    };
}
