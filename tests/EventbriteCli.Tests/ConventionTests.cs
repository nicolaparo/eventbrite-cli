using System.Reflection;
using EventbriteCli.Commands;

namespace EventbriteCli.Tests;

/// <summary>Enforces the repository conventions that the analyzers cannot express (see CLAUDE.md).</summary>
public sealed class ConventionTests
{
    private const BindingFlags DeclaredMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static TheoryData<string> Assemblies => [typeof(ICommandModule).Assembly.FullName!, typeof(ConventionTests).Assembly.FullName!];

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void NoMemberNameContainsUnderscore(string assemblyName)
    {
        var offenders = UserTypes(assemblyName)
            .SelectMany(type => type.GetMembers(DeclaredMembers).Select(member => (type, member)))
            .Where(pair => !IsCompilerGenerated(pair.member.Name) && pair.member.Name.Contains('_', StringComparison.Ordinal))
            .Where(pair => pair.member is not (MethodInfo { IsSpecialName: true } or FieldInfo { IsSpecialName: true }))
            .Select(pair => $"{pair.type.FullName}.{pair.member.Name}");

        Assert.Empty(offenders);
    }

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void AsynchronousMethodsEndWithAsync(string assemblyName)
    {
        var offenders = UserTypes(assemblyName)
            .Where(type => !typeof(Delegate).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(DeclaredMembers).Select(method => (type, method)))
            .Where(pair => !IsCompilerGenerated(pair.method.Name) && !pair.method.IsSpecialName && IsAsynchronous(pair.method.ReturnType))
            .Where(pair => !pair.method.Name.EndsWith("Async", StringComparison.Ordinal))
            .Select(pair => $"{pair.type.FullName}.{pair.method.Name}");

        Assert.Empty(offenders);
    }

    private static IEnumerable<Type> UserTypes(string assemblyName) =>
        Assembly.Load(assemblyName).GetTypes().Where(type => !IsCompilerGenerated(type.Name) && !IsCompilerGenerated(type.FullName ?? string.Empty));

    private static bool IsCompilerGenerated(string name) => name.Contains('<', StringComparison.Ordinal);

    private static bool IsAsynchronous(Type returnType) =>
        returnType == typeof(Task)
        || returnType == typeof(ValueTask)
        || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() is var definition
            && (definition == typeof(Task<>) || definition == typeof(ValueTask<>) || definition == typeof(IAsyncEnumerable<>)));
}
