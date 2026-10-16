using System.Net;

namespace EventbriteCli.Api;

/// <summary>Raised when the Eventbrite API returns a non-success status code.</summary>
internal sealed class EventbriteApiException(HttpStatusCode statusCode, string? error, string? description)
    : Exception($"Eventbrite API error {(int)statusCode} {error}: {description}".TrimEnd(' ', ':'))
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string? Error { get; } = error;
}
