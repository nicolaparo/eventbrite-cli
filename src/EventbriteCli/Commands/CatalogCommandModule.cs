using System.CommandLine;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>catalog</c>: Eventbrite reference data (categories, subcategories, formats).</summary>
internal sealed class CatalogCommandModule(ResourceCommandFactory factory) : ICommandModule
{
    private static readonly IReadOnlyList<Column> Columns = [new("ID", "id"), new("NAME", "name"), new("SHORT NAME", "short_name")];

    private static readonly IReadOnlyList<Column> SubcategoryColumns = [new("ID", "id"), new("NAME", "name"), new("PARENT", "parent_category.name")];

    public Command Create() => new("catalog", "Eventbrite reference data: categories, subcategories, formats.")
    {
        CatalogGroup("categories", "category", Columns),
        CatalogGroup("subcategories", "subcategory", SubcategoryColumns),
        CatalogGroup("formats", "format", Columns),
    };

    private Command CatalogGroup(string resource, string singular, IReadOnlyList<Column> columns) => new(resource, $"Eventbrite {resource}.")
    {
        factory.List("list", $"List all {resource}.", PathSource.Fixed($"{resource}/"), resource, columns),
        factory.Get("get", $"Show a {singular}.", PathSource.ById("id", $"The {singular} id", id => $"{resource}/{id}/"), columns),
    };
}
