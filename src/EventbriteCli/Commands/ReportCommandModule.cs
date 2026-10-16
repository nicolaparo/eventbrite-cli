using System.CommandLine;

namespace EventbriteCli.Commands;

/// <summary><c>reports</c>: sales and attendee reports (JSON).</summary>
internal sealed class ReportCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    public Command Create() => new("reports", "Sales and attendee reports.")
    {
        Report("sales", "Sales report for one or more events."),
        Report("attendees", "Attendee report for one or more events."),
    };

    private Command Report(string name, string description) => factory.Get(
        name,
        description,
        PathSource.Fixed($"reports/{name}/"),
        [],
        QueryOption.Create("event_ids", "Comma separated event ids."),
        QueryOption.Create("event_status", "live, ended or all (alternative to --event-ids)."),
        QueryOption.Create("start_date", "Start of the reporting period (YYYY-MM-DD)."),
        QueryOption.Create("end_date", "End of the reporting period (YYYY-MM-DD)."),
        QueryOption.Create("period", "Number of date_facet units to report on."),
        QueryOption.Create("date_facet", "fifteen, hour, day, event_day, week, month, year or none."),
        QueryOption.Create("filter_by", "JSON filter, e.g. {\"ticket_ids\":[\"123\"]}."),
        QueryOption.Create("group_by", "e.g. payment_method, ticket, currency, event, attendee_status."),
        QueryOption.Create("timezone", "IANA timezone, e.g. Europe/Rome."));
}
