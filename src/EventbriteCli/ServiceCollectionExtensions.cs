using EventbriteCli.Api;
using EventbriteCli.Authentication;
using EventbriteCli.Commands;
using EventbriteCli.Output;
using Microsoft.Extensions.DependencyInjection;

namespace EventbriteCli;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventbriteCli(this IServiceCollection services)
    {
        services.AddSingleton<ITokenStore>(new FileTokenStore(FileTokenStore.DefaultFilePath));
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddTransient<AuthenticationHandler>();
        services.AddHttpClient<IEventbriteClient, EventbriteClient>(client =>
            {
                client.BaseAddress = EventbriteClient.BaseAddress;
                client.DefaultRequestHeaders.UserAgent.ParseAdd("eventbrite-cli");
            })
            .AddHttpMessageHandler<AuthenticationHandler>();

        services.AddSingleton<IOutputWriter>(new ConsoleOutputWriter(Console.Out));
        services.AddTransient<ResourceReader>();
        services.AddTransient<OrganizationResolver>();
        services.AddTransient<ResourceCommandFactory>();

        services.AddTransient<ICommandModule, AuthCommandModule>();
        services.AddTransient<ICommandModule, MeCommandModule>();
        services.AddTransient<ICommandModule, OrganizationCommandModule>();
        services.AddTransient<ICommandModule, EventCommandModule>();
        services.AddTransient<ICommandModule, AttendeeCommandModule>();
        services.AddTransient<ICommandModule, OrderCommandModule>();
        services.AddTransient<ICommandModule, VenueCommandModule>();
        services.AddTransient<ICommandModule, DiscountCommandModule>();
        services.AddTransient<ICommandModule, WebhookCommandModule>();
        services.AddTransient<ICommandModule, ReportCommandModule>();
        services.AddTransient<ICommandModule, CatalogCommandModule>();
        services.AddTransient<ICommandModule, ApiCommandModule>();

        return services;
    }
}
