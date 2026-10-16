using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>attendees</c>: attendees of an event or of the whole organization.</summary>
internal sealed class AttendeeCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private static readonly IReadOnlyList<Column> Columns =
    [
        new("ID", "id"), new("NAME", "profile.name"), new("EMAIL", "profile.email"), new("TICKET", "ticket_class_name"),
        new("STATUS", "status"), new("CHECKED IN", "checked_in"), new("EVENT", "event_id"), new("ORDER", "order_id"), new("CREATED", "created"),
    ];

    public Command Create() => new("attendees", "Attendees of your events.")
    {
        factory.List(
            "list",
            "List attendees of an event (--event) or of the whole organization.",
            factory.EventOrOrganization("attendees"),
            "attendees",
            Columns,
            QueryOption.Create("status", "attending, not_attending or unpaid."),
            CommonQueryOptions.ChangedSince()),
        factory.Get(
            "get",
            "Show an attendee of an event.",
            PathSource.ByIds(("event-id", "Event id"), ("attendee-id", "Attendee id"), (eventId, id) => $"events/{eventId}/attendees/{id}/"),
            Columns,
            CommonQueryOptions.Expand("event,order,promotional_code")),
    };
}
