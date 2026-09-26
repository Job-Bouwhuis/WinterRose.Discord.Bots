using Microsoft.Extensions.DependencyInjection;

namespace WinterRose.Discord.Bots.SlashCommands;

public interface IDiscordCommandBuilder
{
    IDiscordCommandBuilder AddCommand<TCommand>()
        where TCommand : DiscordCommandBase;

    IDiscordCommandCollection Build(IServiceProvider services);
}
