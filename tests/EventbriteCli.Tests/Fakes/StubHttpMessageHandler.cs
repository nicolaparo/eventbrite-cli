using System.Net;
using System.Text;

namespace EventbriteCli.Tests.Fakes;

/// <summary>Returns queued responses and records the requested URIs.</summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode Status, string Body)> responses = new();

    public List<Uri> Requests { get; } = [];

    public StubHttpMessageHandler Enqueue(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        responses.Enqueue((status, body));
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request.RequestUri!);
        var (status, body) = responses.Dequeue();
        return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
}
