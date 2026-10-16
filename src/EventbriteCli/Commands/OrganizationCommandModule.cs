using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>orgs</c>: organizations you belong to, their members and roles.</summary>
internal sealed class OrganizationCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private static readonly IReadOnlyList<Column> OrganizationColumns = [new("ID", "id"), new("NAME", "name"), new("VERTICAL", "vertical")];

    private static readonly IReadOnlyList<Column> MemberColumns = [new("ID", "id"), new("NAME", "name"), new("EMAIL", "email")];

    private static readonly IReadOnlyList<Column> RoleColumns = [new("ID", "id"), new("NAME", "name")];

    public Command Create() => new("orgs", "Organizations you belong to.")
    {
        factory.List("list", "List your organizations.", PathSource.Fixed("users/me/organizations/"), "organizations", OrganizationColumns),
        factory.List("members", "List the members of an organization.", factory.Organization(org => $"organizations/{org}/members/"), "members", MemberColumns),
        factory.List("roles", "List the roles of an organization.", factory.Organization(org => $"organizations/{org}/roles/"), "roles", RoleColumns),
    };
}
