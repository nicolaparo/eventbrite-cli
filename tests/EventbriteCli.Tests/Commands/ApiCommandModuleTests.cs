using EventbriteCli.Commands;

namespace EventbriteCli.Tests.Commands;

public sealed class ApiCommandModuleTests
{
    [Theory]
    [InlineData("users/me", "users/me/")]
    [InlineData("/users/me/", "users/me/")]
    [InlineData("/v3/events/1", "events/1/")]
    [InlineData("events/1?expand=venue", "events/1/?expand=venue")]
    [InlineData("events/1/attendees?changed_since=2026-01-01T00:00:00Z", "events/1/attendees/?changed_since=2026-01-01T00:00:00Z")]
    public void NormalizePathAddsTrailingSlash(string input, string expected)
    {
        Assert.Equal(expected, ApiCommandModule.NormalizePath(input));
    }

    [Theory]
    [InlineData("https://evil.example/steal")]
    [InlineData("//evil.example/steal")]
    [InlineData("https:/evil.example/steal")]
    [InlineData("mailto:someone")]
    public void NormalizePathRejectsAbsoluteUrls(string input)
    {
        Assert.Throws<InvalidOperationException>(() => ApiCommandModule.NormalizePath(input));
    }

    [Fact]
    public void ParseQueryRejectsMissingSeparator()
    {
        Assert.Throws<InvalidOperationException>(() => ApiCommandModule.ParseQuery(["status"]));
    }

    [Fact]
    public void ParseQueryKeepsEqualsInValue()
    {
        Assert.Equal("x/?filter=a%3Db", ApiCommandModule.ParseQuery(["filter=a=b"]).AppendTo("x/"));
    }
}
