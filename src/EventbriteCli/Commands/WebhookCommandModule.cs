using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>webhooks</c>: webhooks registered by your organization.</summary>
internal sealed class WebhookCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private static readonly IReadOnlyList<Column> Columns =
        [new("ID", "id"), new("ENDPOINT", "endpoint_url"), new("ACTIONS", "actions"), new("EVENT", "event_id"), new("CREATED", "created")];

    public Command Create() => new("webhooks", "Webhooks of your organization.")
    {
        factory.List("list", "List the webhooks of an organization.", factory.Organization(org => $"organizations/{org}/webhooks/"), "webhooks", Columns),
    };
}
