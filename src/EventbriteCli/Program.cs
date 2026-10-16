using System.CommandLine;
using EventbriteCli;
using EventbriteCli.Commands;
using Microsoft.Extensions.DependencyInjection;

await using var services = new ServiceCollection().AddEventbriteCli().BuildServiceProvider();

var root = new RootCommand("Read-only command line client for the Eventbrite Platform API v3.") { CommonOptions.Output };
foreach (var module in services.GetServices<ICommandModule>())
{
    root.Subcommands.Add(module.Create());
}

return await root.Parse(args).InvokeAsync();
