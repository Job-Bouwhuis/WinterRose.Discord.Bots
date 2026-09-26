namespace WinterRose.Discord.Bots.SlashCommands;

public sealed record DiscordCommandDescriptor(
    string Name,
    string Description,
    Type CommandType);
