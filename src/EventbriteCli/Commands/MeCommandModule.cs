using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>me</c>: the authenticated user.</summary>
internal sealed class MeCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private static readonly IReadOnlyList<Column> Columns =
        [new("ID", "id"), new("NAME", "name"), new("FIRST NAME", "first_name"), new("LAST NAME", "last_name"), new("EMAIL", "emails.0.email")];

    public Command Create() => factory.Get("me", "Show the authenticated user.", PathSource.Fixed("users/me/"), Columns);
}
