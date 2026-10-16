using System.Security.Cryptography;
using System.Text;

namespace EventbriteCli.Authentication;

/// <summary>
/// Stores the token in the user profile. On Windows the token is encrypted with DPAPI (current user scope);
/// elsewhere it is stored as plain text in a file readable only by the owner.
/// </summary>
internal sealed class FileTokenStore(string filePath) : ITokenStore
{
    public static string DefaultFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "eventbrite-cli",
        "token");

    public string FilePath { get; } = filePath;

    public string Location => FilePath;

    public async Task<string?> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        var content = await File.ReadAllBytesAsync(FilePath, cancellationToken);
        return Encoding.UTF8.GetString(Unprotect(content)).Trim();
    }

    public async Task SaveAsync(string token, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        await File.WriteAllBytesAsync(FilePath, Protect(Encoding.UTF8.GetBytes(token)), cancellationToken);

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(FilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    public Task<bool> DeleteAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(FilePath))
        {
            return Task.FromResult(false);
        }

        File.Delete(FilePath);
        return Task.FromResult(true);
    }

    private static byte[] Protect(byte[] data) =>
        OperatingSystem.IsWindows() ? ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser) : data;

    private static byte[] Unprotect(byte[] data) =>
        OperatingSystem.IsWindows() ? ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser) : data;
}
