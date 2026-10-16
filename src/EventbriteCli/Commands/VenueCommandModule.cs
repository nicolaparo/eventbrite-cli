using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>venues</c>: venues of your organization and the events they host.</summary>
internal sealed class VenueCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private static readonly IReadOnlyList<Column> VenueColumns =
        [new("ID", "id"), new("NAME", "name"), new("ADDRESS", "address.localized_address_display"), new("CAPACITY", "capacity")];

    private static readonly IReadOnlyList<Column> EventColumns =
        [new("ID", "id"), new("NAME", "name.text"), new("START", "start.local"), new("STATUS", "status"), new("URL", "url")];

    public Command Create() => new("venues", "Venues of your organization.")
    {
        factory.List("list", "List the venues of an organization.", factory.Organization(org => $"organizations/{org}/venues/"), "venues", VenueColumns),
        factory.Get("get", "Show a venue.", VenueById(), VenueColumns),
        factory.List(
            "events",
            "List the events hosted at a venue.",
            VenueById("events/"),
            "events",
            EventColumns,
            QueryOption.Create("status", "Comma separated: draft, live, started, ended, completed, canceled, all."),
            QueryOption.Create("order_by", "e.g. start_asc, start_desc."),
            QueryOption.Create("start_date", "Only events starting after this date.")),
    };

    private static PathSource VenueById(string suffix = "") =>
        PathSource.ById("venue-id", "Venue id", id => $"venues/{id}/{suffix}");
}
