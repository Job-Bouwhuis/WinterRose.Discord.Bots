using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Configuration;
using WinterRose.Discord.Bots.SlashCommands;
using WinterRose.Recordium;

namespace WinterRose.Discord.Bots.Application;

public sealed class DiscordBotApplicationBuilder
{
    public IServiceCollection Services { get; }
    public Config Configuration { get; private set; }
    public IDiscordCommandBuilder Commands { get; }

    internal DiscordBotApplicationBuilder(string[] args)
    {
        LogDestinations.AddDestination(new ConsoleLogDestination());
        LogDestinations.AddDestination(new FileLogDestination("logs"));

        Services = new ServiceCollection();
        Commands = new DiscordCommandBuilder(Services);
        Configuration = Config.Empty;
    }

    public void SetConfiguration(string configPath)
    {
        Configuration = new Config(configPath);
        Services.AddSingleton(Configuration);
    }

    public DiscordBotApplication Build()
    {
        var provider = Services.BuildServiceProvider();

        try
        {
            var commands = ((DiscordCommandBuilder)Commands).Build(provider);
            return new DiscordBotApplication(provider, Configuration, commands);
        }
        catch
        {
            provider.Dispose();
            throw;
        }
    }
}
