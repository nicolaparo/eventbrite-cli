using EventbriteCli.Api;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary>Resolves the organization id: explicit option, then EVENTBRITE_ORG, then the user's only organization.</summary>
internal sealed class OrganizationResolver(IEventbriteClient client)
{
    public const string EnvironmentVariableName = "EVENTBRITE_ORG";

    private static readonly Column IdColumn = new("ID", "id");
    private static readonly Column NameColumn = new("NAME", "name");

    public async Task<string> ResolveAsync(string? organizationId, CancellationToken cancellationToken)
    {
        var explicitId = string.IsNullOrWhiteSpace(organizationId)
            ? Environment.GetEnvironmentVariable(EnvironmentVariableName)
            : organizationId;

        if (!string.IsNullOrWhiteSpace(explicitId))
        {
            return explicitId.Trim();
        }

        var organizations = await client
            .GetPagedAsync("users/me/organizations/", "organizations", QueryParameters.Empty, cancellationToken)
            .ToListAsync(cancellationToken);

        return organizations.Count switch
        {
            1 => IdColumn.ValueOf(organizations[0]),
            0 => throw new InvalidOperationException("Your account has no organizations."),
            _ => throw new InvalidOperationException(
                $"You belong to several organizations; pass --org or set {EnvironmentVariableName}. Available: "
                + string.Join(", ", organizations.Select(organization => $"{IdColumn.ValueOf(organization)} ({NameColumn.ValueOf(organization)})"))),
        };
    }
}
