namespace EventbriteCli.Authentication;

/// <summary>Persists the Eventbrite private token for the current user.</summary>
internal interface ITokenStore
{
    /// <summary>Human readable description of where the token is kept.</summary>
    string Location { get; }

    Task<string?> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(string token, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(CancellationToken cancellationToken);
}
