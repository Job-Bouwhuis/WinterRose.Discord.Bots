using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using WinterRose.Discord.Bots.Application;
using WinterRose.Discord.Bots.SlashCommands;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = DiscordBotApplication.CreateBuilder(args);
        builder.Commands
            .AddCommand<HelloWorldCommand>()
            .AddCommand<OtherCommand>();

        builder.SetConfiguration("AppSettings.wf");
        var app = builder.Build();

        await app.RunAsync();
    }
}
public sealed class HelloWorldCommand : DiscordCommandBase
{
    public override string Name => "hello";
    public override string Description => "Sends a friendly greeting.";

    public override async Task ExecuteAsync(SocketSlashCommand command, CancellationToken cancellationToken)
    {
        string name = command.Data.Options.FirstOrDefault(o => o.Name == "name")?.Value?.ToString() ?? "World";
        await RespondAsync(command, $"Hello, {name}!");
    }

    public override SlashCommandBuilder CreateSlashCommand()
    {
        var builder = base.CreateSlashCommand();
        builder.AddOption(name: "name", description: "The name to greet", type: ApplicationCommandOptionType.String);
        return builder;
    }
}

public sealed class OtherCommand : DiscordCommandBase
{
    public override string Name => "other";
    public override string Description => "idk what this does";

    public override async Task ExecuteAsync(SocketSlashCommand command, CancellationToken cancellationToken)
    {
        await RespondAsync(command, $"why must you wake me?");
        await FollowupAsync(command, $"Brother you are the most vile creature in existence");

        var embed = new EmbedBuilder()
            .WithTitle("Complaint")
            .WithColor(Color.Purple)
            .WithFields([
                new EmbedFieldBuilder().WithName("test field").WithValue("the value"),
                new EmbedFieldBuilder().WithName("inline test").WithIsInline(true).WithValue(12345)
            ])
            .Build();
        await FollowupAsync(command, embed);

        await FollowupWithFileAsync(command, "AppSettings - Copy.wf");
    }
}
