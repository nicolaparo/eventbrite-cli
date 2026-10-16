using EventbriteCli.Authentication;

namespace EventbriteCli.Tests.Authentication;

public sealed class FileTokenStoreTests : IDisposable
{
    private readonly string directory = Directory.CreateTempSubdirectory("eventbrite-cli-tests").FullName;

    public void Dispose() => Directory.Delete(directory, recursive: true);

    [Fact]
    public async Task SaveThenLoadRoundTripsTokenAsync()
    {
        var store = new FileTokenStore(Path.Combine(directory, "nested", "token"));

        await store.SaveAsync("SECRET123", TestContext.Current.CancellationToken);

        Assert.Equal("SECRET123", await store.LoadAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SavedFileDoesNotContainPlainTokenOnWindowsAsync()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "DPAPI encryption is Windows only.");
        var path = Path.Combine(directory, "token");

        await new FileTokenStore(path).SaveAsync("SECRET123", TestContext.Current.CancellationToken);

        Assert.DoesNotContain("SECRET123", await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadReturnsNullAndDeleteReturnsFalseWhenMissingAsync()
    {
        var store = new FileTokenStore(Path.Combine(directory, "token"));

        Assert.Null(await store.LoadAsync(TestContext.Current.CancellationToken));
        Assert.False(await store.DeleteAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteRemovesStoredTokenAsync()
    {
        var store = new FileTokenStore(Path.Combine(directory, "token"));
        await store.SaveAsync("SECRET123", TestContext.Current.CancellationToken);

        Assert.True(await store.DeleteAsync(TestContext.Current.CancellationToken));
        Assert.Null(await store.LoadAsync(TestContext.Current.CancellationToken));
    }
}
