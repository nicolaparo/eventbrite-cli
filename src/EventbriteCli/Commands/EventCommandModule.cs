using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>events</c>: events, their description, ticket classes, questions and inventory.</summary>
internal sealed class EventCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private const string EventExpansions = "venue,organizer,ticket_availability,category,format,logo";

    private static readonly IReadOnlyList<Column> EventColumns =
    [
        new("ID", "id"), new("NAME", "name.text"), new("START", "start.local"), new("END", "end.local"), new("STATUS", "status"),
        new("CAPACITY", "capacity"), new("ONLINE", "online_event"), new("URL", "url"),
    ];

    private static readonly IReadOnlyList<Column> TicketClassColumns =
    [
        new("ID", "id"), new("NAME", "name"), new("COST", "cost.display"), new("FREE", "free"), new("CAPACITY", "capacity"),
        new("SOLD", "quantity_sold"), new("ON SALE", "on_sale_status"), new("SALES END", "sales_end"),
    ];

    private static readonly IReadOnlyList<Column> QuestionColumns = [new("ID", "id"), new("QUESTION", "question.text"), new("TYPE", "type"), new("REQUIRED", "required")];

    private static readonly IReadOnlyList<Column> InventoryTierColumns = [new("ID", "id"), new("NAME", "name"), new("CAPACITY", "capacity"), new("SOLD", "quantity_sold")];

    private static readonly IReadOnlyList<Column> TeamColumns = [new("ID", "id"), new("NAME", "name"), new("ATTENDEES", "attendee_count")];

    public Command Create() => new("events", "Events of your organization.")
    {
        factory.List(
            "list",
            "List the events of an organization.",
            factory.Organization(org => $"organizations/{org}/events/"),
            "events",
            EventColumns,
            QueryOption.Create("status", "Comma separated: draft, live, started, ended, completed, canceled, all."),
            QueryOption.Create("time_filter", "past, current_future or all."),
            QueryOption.Create("order_by", "e.g. start_asc, start_desc, created_asc, created_desc."),
            QueryOption.Create("name_filter", "Only events whose name contains this text."),
            CommonQueryOptions.Expand(EventExpansions)),
        factory.Get("get", "Show an event.", EventById(), EventColumns, CommonQueryOptions.Expand(EventExpansions)),
        factory.Get("description", "Show the full HTML description of an event.", EventById("description/"), [new("DESCRIPTION", "description")]),
        factory.Get(
            "structured-content",
            "Show the structured content (modules) of an event page.",
            EventById("structured_content/"),
            [],
            QueryOption.Create("purpose", "listing or digital_content.", required: true)),
        factory.List("ticket-classes", "List the ticket classes of an event.", EventById("ticket_classes/"), "ticket_classes", TicketClassColumns, QueryOption.Create("pos", "online or at_the_door.")),
        factory.Get("ticket-class", "Show a ticket class.", PathSource.ByIds(("event-id", "Event id"), ("ticket-class-id", "Ticket class id"), (eventId, id) => $"events/{eventId}/ticket_classes/{id}/"), TicketClassColumns),
        factory.List("questions", "List the custom checkout questions of an event.", EventById("questions/"), "questions", QuestionColumns),
        factory.List("canned-questions", "List the default (canned) checkout questions of an event.", EventById("canned_questions/"), "questions", QuestionColumns),
        factory.List("inventory-tiers", "List the inventory tiers of an event.", EventById("inventory_tiers/"), "inventory_tiers", InventoryTierColumns),
        factory.Get("capacity", "Show the capacity tier of an event.", EventById("capacity_tier/"), [new("CAPACITY", "capacity_total"), new("SOLD", "capacity_sold"), new("PENDING", "capacity_pending"), new("REMAINING", "capacity_remaining")]),
        factory.Get("display-settings", "Show the display settings of an event.", EventById("display_settings/"), []),
        factory.List("teams", "List the teams of an event.", EventById("teams/"), "teams", TeamColumns),
    };

    private static PathSource EventById(string suffix = "") =>
        PathSource.ById("event-id", "Event id", id => $"events/{id}/{suffix}");
}
