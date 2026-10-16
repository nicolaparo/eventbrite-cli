using System.CommandLine;
using EventbriteCli.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace EventbriteCli;

internal static class RootCommandBuilder
{
    public static RootCommand Build(IServiceProvider services)
    {
        var root = new RootCommand("Read-only command line client for the Eventbrite Platform API v3.") { CommonOptions.Output };
        foreach (var module in services.GetServices<ICommandModule>())
        {
            root.Subcommands.Add(module.Create());
        }

        return root;
    }
}
