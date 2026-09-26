using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using WinterRose.Configuration;
using WinterRose.Discord.Bots.SlashCommands;
using WinterRose.Recordium;

namespace WinterRose.Discord.Bots.Application;

public sealed class DiscordBotApplication : IAsyncDisposable
{
    private readonly IServiceProvider services;
    private readonly DiscordSocketClient client;
    public IDiscordCommandCollection Commands { get; }
    public Log log = new("Discord");

    public readonly Config Configuration;

    public static DiscordBotApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = new DiscordBotApplicationBuilder(args);
        builder.Services.AddSingleton(_ => new DiscordSocketClient(
                                                new DiscordSocketConfig
                                                {
                                                    GatewayIntents = GatewayIntents.Guilds
                                                }));
        return builder;
    }

    internal DiscordBotApplication(
        IServiceProvider serviceProvider,
        Config configuration,
        IDiscordCommandCollection commands)
    {
        services = serviceProvider;
        client = services.GetRequiredService<DiscordSocketClient>();
        Configuration = configuration;
        Commands = commands;
    }

    internal async Task RegisterCommandsAsync(IDiscordCommandCollection commands, SocketGuild guild)
    {
        var existingCommands = await guild.GetApplicationCommandsAsync();

        foreach (var command in commands)
        {
            var builder = command.CreateSlashCommand();

            var desired = builder.Build();

            var existing = existingCommands.FirstOrDefault(
                item => string.Equals(
                    item.Name,
                    desired.Name.Value,
                    StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                if (CommandMatches(existing, desired))
                    continue;
                
                await existing.DeleteAsync();
            }

            log.Info($"Updating command {existing.Name}");
            await guild.CreateApplicationCommandAsync(desired);
        }
    }

    private static bool CommandMatches(SocketApplicationCommand existing, SlashCommandProperties desired)
    {
        if (existing.Name != desired.Name.Value ||
            existing.Description != desired.Description.Value)
            return false;

        var desiredOptions = desired.Options.IsSpecified ? desired.Options.Value : [];
        var existingOptions = existing.Options ?? [];

        return OptionsMatch(existingOptions, desiredOptions);
    }

    private static bool OptionsMatch(
        IReadOnlyCollection<SocketApplicationCommandOption> existing,
        IReadOnlyCollection<ApplicationCommandOptionProperties> desired)
    {
        if (existing.Count != desired.Count)
            return false;

        foreach (var desiredOption in desired)
        {
            var existingOption = existing.FirstOrDefault(
                option => string.Equals(
                    option.Name,
                    desiredOption.Name,
                    StringComparison.OrdinalIgnoreCase));

            if (existingOption is null ||
                existingOption.Description != desiredOption.Description ||
                existingOption.Type != desiredOption.Type ||
                existingOption.IsRequired != desiredOption.IsRequired)
                return false;

            if (!OptionsMatch(
                    existingOption.Options,
                    desiredOption.Options ?? []))
                return false;
        }

        return true;
    }

    CancellationTokenSource cancelSource = new();
    CancellationTokenSource BotCancelSource;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        BotCancelSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, cancelSource.Token);

        string token = (string)Configuration["BotToken"];
        ulong serverId = (ulong)Configuration["ServerId"];

        var ready = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        client.Ready += () =>
        {
            ready.TrySetResult();
            return Task.CompletedTask;
        };

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        try
        {
            await ready.Task.WaitAsync(BotCancelSource.Token);

            log.Info("Initializing...");

            var guild = client.GetGuild(serverId)
                ?? throw new InvalidOperationException(
                    $"Bot is not connected to guild {serverId}.");

            await RegisterCommandsAsync(Commands, guild);

            client.SlashCommandExecuted += HandleSlashCommandAsync;

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                log.Info("Stopping.");
                cancelSource.Cancel();
            };

            log.Info("Bot is running. Press Ctrl+C to stop.");

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, BotCancelSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
        }
        finally
        {
            await client.StopAsync();
        }
        
    }


    private async Task HandleSlashCommandAsync(SocketSlashCommand interaction)
    {
        if (!Commands.TryGetCommand(interaction.CommandName, out var registeredCommand))
        {
            await interaction.RespondAsync(
                "Sorry, I don't recognize that command.",
                ephemeral: true);
            return;
        }

        log.Debug($"Executing command '{interaction.CommandName}' for user {interaction.User.GlobalName ?? interaction.User.Username} ({interaction.User.Id})");

        await using var scope = services.CreateAsyncScope();

        CancellationTokenSource ccs = new();
        CancellationTokenSource commandCancelSource = CancellationTokenSource.CreateLinkedTokenSource(BotCancelSource.Token, ccs.Token);

        Task commandHandle = Task.Run(async () =>
        {
            try
            {
                var command = (DiscordCommandBase)ActivatorUtilities.CreateInstance(
                scope.ServiceProvider,
                registeredCommand.GetType());

                await command.ExecuteAsync(interaction, commandCancelSource.Token);
            }
            catch (Exception exception)
            {
                if (commandCancelSource.Token.IsCancellationRequested)
                    return;

                Console.Error.WriteLine($"Command '{interaction.CommandName}' failed: {exception?.ToString() ?? "unknown error"}");

                if (!interaction.HasResponded)
                {
                    await interaction.RespondAsync(
                        "An error occurred while executing this command.",
                        ephemeral: true);
                }
                else
                {
                    await interaction.FollowupAsync(
                        "An error occurred while executing this command.",
                        ephemeral: true);
                }
            }
        });

        try
        {
            await commandHandle.WaitAsync(TimeSpan.FromMilliseconds(2500));
        }
        catch (TimeoutException)
        {
            ccs.Cancel();
            if (!interaction.HasResponded)
            {
                await interaction.RespondAsync(
                          "Sorry. it seems this command takes too long to complete.",
                          ephemeral: true);
            }
            else
            {
                await interaction.FollowupAsync(
                          "Sorry. it seems this command takes too long to complete.",
                          ephemeral: true);
            }

        }
    }


    public async ValueTask DisposeAsync()
    {
        BotCancelSource?.Cancel();
    }
}