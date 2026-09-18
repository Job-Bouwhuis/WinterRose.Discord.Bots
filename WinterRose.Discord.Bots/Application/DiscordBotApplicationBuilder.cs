using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.Discord.Bots.Application;

public sealed class DiscordBotApplicationBuilder
{
    public IServiceCollection Services { get; }

    internal DiscordBotApplicationBuilder(string[] args)
    {
        Services = new ServiceCollection();
    }

    public DiscordBotApplication Build()
    {
        return new DiscordBotApplication(Services);
    }
}
