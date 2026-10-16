using System.CommandLine;

namespace EventbriteCli.Commands;

internal delegate Task<string> PathResolver(ParseResult parseResult, CancellationToken cancellationToken);

/// <summary>Describes how a command builds its API path: the symbols it adds to the command and how they map to the path.</summary>
internal sealed record PathSource(IReadOnlyList<Symbol> Symbols, PathResolver Resolve)
{
    public static PathSource Fixed(string path) => new([], (_, _) => Task.FromResult(path));

    public static PathSource ById(string argumentName, string description, Func<string, string> path)
    {
        var id = CommonOptions.Id(argumentName, description);
        return new([id], (parseResult, _) => Task.FromResult(path(Escape(parseResult.GetValue(id)))));
    }

    public static PathSource ByIds(
        (string Name, string Description) first,
        (string Name, string Description) second,
        Func<string, string, string> path)
    {
        var firstId = CommonOptions.Id(first.Name, first.Description);
        var secondId = CommonOptions.Id(second.Name, second.Description);
        return new(
            [firstId, secondId],
            (parseResult, _) => Task.FromResult(path(Escape(parseResult.GetValue(firstId)), Escape(parseResult.GetValue(secondId)))));
    }

    /// <summary>Escapes an id so it cannot alter the path (e.g. <c>../</c>).</summary>
    public static string Escape(string? id) => Uri.EscapeDataString(id?.Trim() ?? string.Empty);
}
