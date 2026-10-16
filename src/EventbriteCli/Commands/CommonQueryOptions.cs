namespace EventbriteCli.Commands;

/// <summary>Query options reused across resources.</summary>
internal static class CommonQueryOptions
{
    public static QueryOption Expand(string examples) =>
        QueryOption.Create("expand", $"Comma separated related objects to include, e.g. {examples}.");

    public static QueryOption ChangedSince() =>
        QueryOption.Create("changed_since", "Only items changed on or after this UTC time, e.g. 2026-01-31T00:00:00Z.");
}
