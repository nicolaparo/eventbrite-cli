using System.CommandLine;
using System.Security.Cryptography;
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
            catch (Exception exception) when (Describe(exception, cancellationToken) is { } message)
            {
                await parseResult.InvocationConfiguration.Error.WriteLineAsync(message.AsMemory(), CancellationToken.None);
                return 1;
            }
        });

    private static string? Describe(Exception exception, CancellationToken cancellationToken) => exception switch
    {
        MissingTokenException or EventbriteApiException or HttpRequestException or InvalidOperationException => exception.Message,
        TaskCanceledException when !cancellationToken.IsCancellationRequested => "The Eventbrite API did not respond in time.",
        OperationCanceledException => "Cancelled.",
        CryptographicException => "The stored token cannot be decrypted. Run 'eventbrite auth login' again.",
        _ => null,
    };
}
