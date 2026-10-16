using System.CommandLine;

namespace EventbriteCli.Commands;

/// <summary>A top-level command group. Register new modules in DI to extend the CLI without touching existing ones.</summary>
internal interface ICommandModule
{
    Command Create();
}
