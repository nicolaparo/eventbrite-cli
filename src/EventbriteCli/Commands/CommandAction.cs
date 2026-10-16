using System.CommandLine;
using EventbriteCli.Api;

namespace EventbriteCli.Commands;

/// <summary>Wraps command actions so expected failures go to stderr with a non-zero exit code instead of a stack trace.</summary>
internal static class CommandAction
{
    public static void SetHandler(this Command command, Func<ParseResult, CancellationToken, Task> action) =>
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            try
            {
                await action(parseResult, cancellationToken);
                return 0;
            }
            catch (Exception exception) when (exception is MissingTokenException or EventbriteApiException or HttpRequestException or InvalidOperationException)
            {
                await parseResult.InvocationConfiguration.Error.WriteLineAsync(exception.Message.AsMemory(), cancellationToken);
                return 1;
            }
        });
}
