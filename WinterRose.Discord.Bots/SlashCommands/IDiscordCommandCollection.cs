namespace WinterRose.Discord.Bots.SlashCommands;

public interface IDiscordCommandCollection :
    IReadOnlyCollection<DiscordCommandBase>
{
    bool TryGetCommand(
        string name,
        out DiscordCommandBase command);
}