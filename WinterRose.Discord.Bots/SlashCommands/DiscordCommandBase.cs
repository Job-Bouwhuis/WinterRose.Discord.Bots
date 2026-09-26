using Discord;
using Discord.WebSocket;
using WinterRose.Recordium;

namespace WinterRose.Discord.Bots.SlashCommands;

public abstract class DiscordCommandBase
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    protected Log log { get; }

    protected DiscordCommandBase() => log = new Log(Name);

    public virtual SlashCommandBuilder CreateSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description)
            .WithContextTypes(
                InteractionContextType.Guild | InteractionContextType.BotDm | InteractionContextType.PrivateChannel);
    }

    public abstract Task ExecuteAsync(SocketSlashCommand command, CancellationToken cancellationToken);

    protected Task RespondAsync(SocketSlashCommand command, string message, bool ephemeral = false)
        => command.RespondAsync(text: message, ephemeral: ephemeral);

    protected Task RespondAsync(SocketSlashCommand command, Embed embed, bool ephemeral = false)
        => command.RespondAsync(embed: embed, ephemeral: ephemeral);

    protected Task RespondWithFileAsync(SocketSlashCommand command, string filePath, Embed? embed = null, string? nameOverride = null)
    {
        using FileStream fs = File.Open("AppSettings - Copy.wf", FileMode.Open, FileAccess.Read, FileShare.Read);
        nameOverride ??= fs.Name;
        FileAttachment attachment = new(fs, nameOverride);

        return command.RespondWithFileAsync(attachment, embed: embed);
    }

    protected Task DeferAsync(SocketSlashCommand command, bool ephemeral = false)
        => command.DeferAsync(ephemeral: ephemeral);

    protected Task FollowupAsync(SocketSlashCommand command, string message, bool ephemeral = false)
        => command.FollowupAsync(text: message, ephemeral: ephemeral);

    protected Task FollowupAsync(SocketSlashCommand command, Embed embed, bool ephemeral = false) 
        => command.FollowupAsync(embed: embed, ephemeral: ephemeral);

    protected Task FollowupWithFileAsync(SocketSlashCommand command, string filePath, Embed? embed = null, string? nameOverride = null)
    {
        using FileStream fs = File.Open("AppSettings - Copy.wf", FileMode.Open, FileAccess.Read, FileShare.Read);
        nameOverride ??= fs.Name;
        FileAttachment attachment = new(fs, nameOverride);

        return command.FollowupWithFileAsync(attachment, embed: embed);
    }
}
