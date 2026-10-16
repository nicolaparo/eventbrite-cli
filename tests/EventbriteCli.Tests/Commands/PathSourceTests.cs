using EventbriteCli.Commands;

namespace EventbriteCli.Tests.Commands;

public sealed class PathSourceTests
{
    [Theory]
    [InlineData("123", "123")]
    [InlineData(" 123 ", "123")]
    [InlineData("../users/me", "..%2Fusers%2Fme")]
    [InlineData(null, "")]
    public void EscapePreventsPathTraversal(string? id, string expected)
    {
        Assert.Equal(expected, PathSource.Escape(id));
    }
}
