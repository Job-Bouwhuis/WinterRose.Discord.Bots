using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.Discord.Bots.Application;

public sealed class DiscordBotApplication
{
    private readonly IServiceProvider services;

    internal DiscordBotApplication(IServiceCollection serviceCollection)
    {
        services = serviceCollection.BuildServiceProvider();
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public static DiscordBotApplicationBuilder CreateBuilder(string[] args)
    {
        return new DiscordBotApplicationBuilder(args);
    }
}
