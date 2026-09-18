using Microsoft.Extensions.DependencyInjection;
using WinterRose.Discord.Bots.Application;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = DiscordBotApplication.CreateBuilder(args);

        builder.Services.AddSingleton<BotState>();

        var app = builder.Build();

        await app.RunAsync();
    }
}