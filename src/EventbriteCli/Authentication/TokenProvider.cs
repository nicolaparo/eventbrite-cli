namespace EventbriteCli.Authentication;

internal interface ITokenProvider
{
    /// <summary>Returns the token to use, or <c>null</c> when none is configured.</summary>
    Task<string?> GetTokenAsync(CancellationToken cancellationToken);
}

/// <summary>Resolves the token from the <c>EVENTBRITE_TOKEN</c> environment variable first, then from the token store.</summary>
internal sealed class TokenProvider(ITokenStore tokenStore) : ITokenProvider
{
    public const string EnvironmentVariableName = "EVENTBRITE_TOKEN";

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        return string.IsNullOrWhiteSpace(fromEnvironment)
            ? await tokenStore.LoadAsync(cancellationToken)
            : fromEnvironment.Trim();
    }
}
