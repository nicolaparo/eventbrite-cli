using System.Net.Http.Headers;
using EventbriteCli.Authentication;

namespace EventbriteCli.Api;

/// <summary>Adds the Eventbrite private token as a bearer token to every request.</summary>
internal sealed class AuthenticationHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken)
            ?? throw new MissingTokenException();

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}

internal sealed class MissingTokenException() : Exception(
    $"No Eventbrite token configured. Run 'eventbrite auth login' or set the {TokenProvider.EnvironmentVariableName} environment variable.");
