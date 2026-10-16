using System.CommandLine;
using System.Text;
using System.Text.Json;
using EventbriteCli.Api;
using EventbriteCli.Authentication;
using EventbriteCli.Output;

namespace EventbriteCli.Commands;

/// <summary><c>auth login | logout | status</c>: manages the stored private token.</summary>
internal sealed class AuthCommandModule(ITokenStore tokenStore, IEventbriteClient client) : ICommandModule
{
    private static readonly IReadOnlyList<Column> UserColumns = [new("ID", "id"), new("NAME", "name"), new("EMAIL", "emails.0.email")];

    public Command Create()
    {
        var login = new Command("login", "Store your Eventbrite private token (read from a hidden prompt or from piped stdin).");
        login.SetHandler(LoginAsync);

        var logout = new Command("logout", "Delete the stored token.");
        logout.SetHandler(LogoutAsync);

        var status = new Command("status", "Show where the token comes from and who it authenticates.");
        status.SetHandler(StatusAsync);

        return new Command("auth", "Manage the Eventbrite private token.") { login, logout, status };
    }

    private async Task LoginAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var console = parseResult.InvocationConfiguration.Output;
        var token = Console.IsInputRedirected
            ? (await Console.In.ReadToEndAsync(cancellationToken)).Trim()
            : await ReadHiddenAsync(console, cancellationToken);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("No token provided.");
        }

        var previous = await tokenStore.LoadAsync(cancellationToken);
        await tokenStore.SaveAsync(token, cancellationToken);

        if (IsEnvironmentTokenSet())
        {
            await console.WriteLineAsync($"Token stored, but {TokenProvider.EnvironmentVariableName} is set and takes precedence.".AsMemory(), cancellationToken);
            return;
        }

        try
        {
            var user = await client.GetAsync("users/me/", QueryParameters.Empty, cancellationToken);
            await console.WriteLineAsync($"Logged in as {Describe(user)}.".AsMemory(), cancellationToken);
        }
        catch (EventbriteApiException)
        {
            await RestoreAsync(previous, cancellationToken);
            throw;
        }
    }

    private async Task LogoutAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var deleted = await tokenStore.DeleteAsync(cancellationToken);
        await parseResult.InvocationConfiguration.Output.WriteLineAsync(
            (deleted ? "Stored token deleted." : "No stored token.").AsMemory(), cancellationToken);
    }

    private async Task StatusAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var console = parseResult.InvocationConfiguration.Output;
        var source = IsEnvironmentTokenSet()
            ? $"environment variable {TokenProvider.EnvironmentVariableName}"
            : $"stored token ({tokenStore.Location})";

        var user = await client.GetAsync("users/me/", QueryParameters.Empty, cancellationToken);
        await console.WriteLineAsync($"Token source: {source}".AsMemory(), cancellationToken);
        await console.WriteLineAsync($"Authenticated as {Describe(user)}.".AsMemory(), cancellationToken);
    }

    private static bool IsEnvironmentTokenSet() =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(TokenProvider.EnvironmentVariableName));

    private static string Describe(JsonElement user) =>
        $"{UserColumns[1].ValueOf(user)} <{UserColumns[2].ValueOf(user)}> (user id {UserColumns[0].ValueOf(user)})";

    private async Task RestoreAsync(string? previous, CancellationToken cancellationToken)
    {
        if (previous is null)
        {
            await tokenStore.DeleteAsync(cancellationToken);
        }
        else
        {
            await tokenStore.SaveAsync(previous, cancellationToken);
        }
    }

    private static async Task<string> ReadHiddenAsync(TextWriter console, CancellationToken cancellationToken)
    {
        await console.WriteAsync("Eventbrite private token: ".AsMemory(), cancellationToken);
        var buffer = new StringBuilder();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                buffer.Length = Math.Max(0, buffer.Length - 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
            }
        }

        await console.WriteLineAsync();
        return buffer.ToString().Trim();
    }
}
