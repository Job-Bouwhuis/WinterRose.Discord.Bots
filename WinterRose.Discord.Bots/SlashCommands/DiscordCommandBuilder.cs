using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;

namespace WinterRose.Discord.Bots.SlashCommands;

internal sealed class DiscordCommandBuilder : IDiscordCommandBuilder
{
    private readonly IServiceCollection services;
    private readonly List<Type> commandTypes = [];

    internal DiscordCommandBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public IDiscordCommandBuilder AddCommand<TCommand>() where TCommand : DiscordCommandBase
    {
        Type commandType = typeof(TCommand);

        if (commandTypes.Contains(commandType))
            throw new InvalidOperationException(
                $"Command type '{commandType.Name}' is already registered.");

        services.AddSingleton<TCommand>();
        commandTypes.Add(commandType);
        return this;
    }

    public IDiscordCommandCollection Build(IServiceProvider provider)
    {
        var commands = commandTypes
            .Select(commandType =>
                (DiscordCommandBase)provider.GetRequiredService(commandType))
            .ToArray();

        return new DiscordCommandCollection(commands);
    }
}
