using EventbriteCli.Api;
using EventbriteCli.Authentication;
using EventbriteCli.Tests.Fakes;

namespace EventbriteCli.Tests.Api;

public sealed class AuthenticationHandlerTests
{
    [Fact]
    public async Task AddsBearerTokenForEventbriteHostAsync()
    {
        var recorder = new RecordingHandler(new StubHttpMessageHandler().Enqueue("{}"));
        using var client = new HttpClient(new AuthenticationHandler(new TokenProviderStub("abc")) { InnerHandler = recorder });

        using var response = await client.GetAsync(new Uri(EventbriteClient.BaseAddress, "users/me/"), TestContext.Current.CancellationToken);

        Assert.Equal("Bearer abc", recorder.LastAuthorization);
    }

    [Fact]
    public async Task RefusesToSendTokenToOtherHostsAsync()
    {
        var stub = new StubHttpMessageHandler();
        using var client = new HttpClient(new AuthenticationHandler(new TokenProviderStub("abc")) { InnerHandler = stub });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetAsync(new Uri("https://evil.example/"), TestContext.Current.CancellationToken));
        Assert.Empty(stub.Requests);
    }

    [Fact]
    public async Task ThrowsMissingTokenWhenNoneConfiguredAsync()
    {
        using var client = new HttpClient(new AuthenticationHandler(new TokenProviderStub(null)) { InnerHandler = new StubHttpMessageHandler() });

        await Assert.ThrowsAsync<MissingTokenException>(
            () => client.GetAsync(new Uri(EventbriteClient.BaseAddress, "users/me/"), TestContext.Current.CancellationToken));
    }

    private sealed class TokenProviderStub(string? token) : ITokenProvider
    {
        public Task<string?> GetTokenAsync(CancellationToken cancellationToken) => Task.FromResult(token);
    }

    private sealed class RecordingHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
    {
        public string? LastAuthorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastAuthorization = request.Headers.Authorization?.ToString();
            return base.SendAsync(request, cancellationToken);
        }
    }
}
