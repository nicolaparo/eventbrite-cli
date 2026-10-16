using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary>Options shared by many commands. Factories return new instances so each command owns its symbols.</summary>
internal static class CommonOptions
{
    public static Option<OutputFormat> Output { get; } = new("--output", "-o")
    {
        Description = "Output format: table (summary columns) or json (complete API payload).",
        DefaultValueFactory = _ => OutputFormat.Table,
        Recursive = true,
    };

    public static Option<int> Limit() => new("--limit", "-n")
    {
        Description = "Maximum number of items to return (0 = all, follows pagination).",
        DefaultValueFactory = _ => 50,
    };

    public static Option<string?> Organization() => new("--org")
    {
        Description = "Organization id. Defaults to your only organization (or EVENTBRITE_ORG).",
    };

    public static Argument<string> Id(string name, string description) => new(name) { Description = description };
}
