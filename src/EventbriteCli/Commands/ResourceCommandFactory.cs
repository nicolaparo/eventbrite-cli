using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary>Builds the read-only <c>list</c> and <c>get</c> commands shared by every resource.</summary>
internal sealed class ResourceCommandFactory(ResourceReader reader, OrganizationResolver organizations)
{
    public Command List(
        string name,
        string description,
        PathSource source,
        string itemsProperty,
        IReadOnlyList<Column> columns,
        params QueryOption[] filters)
    {
        var limit = CommonOptions.Limit();
        var command = new Command(name, description) { limit };
        AddSymbols(command, source.Symbols);
        AddSymbols(command, filters.Select(filter => filter.Option));

        command.SetHandler(async (parseResult, cancellationToken) =>
            await reader.ShowListAsync(
                await source.Resolve(parseResult, cancellationToken),
                itemsProperty,
                QueryOption.Apply(parseResult, filters),
                columns,
                parseResult.GetValue(CommonOptions.Output),
                parseResult.GetValue(limit),
                cancellationToken));

        return command;
    }

    public Command Get(
        string name,
        string description,
        PathSource source,
        IReadOnlyList<Column> columns,
        params QueryOption[] parameters)
    {
        var command = new Command(name, description);
        AddSymbols(command, source.Symbols);
        AddSymbols(command, parameters.Select(parameter => parameter.Option));

        command.SetHandler(async (parseResult, cancellationToken) =>
            await reader.ShowItemAsync(
                await source.Resolve(parseResult, cancellationToken),
                QueryOption.Apply(parseResult, parameters),
                columns,
                parseResult.GetValue(CommonOptions.Output),
                cancellationToken));

        return command;
    }

    /// <summary>A path under the organization given by <c>--org</c>, falling back to the default organization.</summary>
    public PathSource Organization(Func<string, string> path)
    {
        var organization = CommonOptions.Organization();
        return new(
            [organization],
            async (parseResult, cancellationToken) =>
                path(PathSource.Escape(await organizations.ResolveAsync(parseResult.GetValue(organization), cancellationToken))));
    }

    /// <summary>A path under <c>--event</c> when given, otherwise under the organization.</summary>
    public PathSource EventOrOrganization(string resource)
    {
        var eventId = new Option<string?>("--event") { Description = "Event id. When omitted, lists across the whole organization." };
        var organization = Organization(org => $"organizations/{org}/{resource}/");
        return new(
            [eventId, .. organization.Symbols],
            async (parseResult, cancellationToken) =>
                parseResult.GetValue(eventId) is { Length: > 0 } id
                    ? $"events/{PathSource.Escape(id)}/{resource}/"
                    : await organization.Resolve(parseResult, cancellationToken));
    }

    private static void AddSymbols(Command command, IEnumerable<Symbol> symbols)
    {
        foreach (var symbol in symbols)
        {
            switch (symbol)
            {
                case Argument argument:
                    command.Arguments.Add(argument);
                    break;
                case Option option:
                    command.Options.Add(option);
                    break;
                default:
                    throw new ArgumentException($"Unsupported symbol {symbol.GetType().Name}.", nameof(symbols));
            }
        }
    }
}
