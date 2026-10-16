using System.Text;
using EventbriteCli;
using Microsoft.Extensions.DependencyInjection;

Console.OutputEncoding = Encoding.UTF8;

await using var services = new ServiceCollection().AddEventbriteCli().BuildServiceProvider();

return await RootCommandBuilder.Build(services).Parse(args).InvokeAsync();
