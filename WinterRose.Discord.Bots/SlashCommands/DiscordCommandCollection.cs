using System.Collections;
using System.Collections.ObjectModel;

namespace WinterRose.Discord.Bots.SlashCommands;

internal sealed class DiscordCommandCollection : IDiscordCommandCollection
{
    private readonly IReadOnlyDictionary<string, DiscordCommandBase> commands;

    public int Count => commands.Count;

    internal DiscordCommandCollection(
        IEnumerable<DiscordCommandBase> commandInstances)
    {
        var commandDictionary =
            new Dictionary<string, DiscordCommandBase>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var command in commandInstances)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(command.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(command.Description);

            if (command.Name.Length > 32)
                throw new InvalidOperationException(
                    $"Command '{command.Name}' exceeds Discord's name limit.");

            if (!commandDictionary.TryAdd(command.Name, command))
                throw new InvalidOperationException(
                    $"Duplicate command name '{command.Name}'.");
        }

        commands = new System.Collections.ObjectModel
            .ReadOnlyDictionary<string, DiscordCommandBase>(commandDictionary);
    }

    public bool TryGetCommand(
        string name,
        out DiscordCommandBase command)
    {
        return commands.TryGetValue(name, out command!);
    }

    public IEnumerator<DiscordCommandBase> GetEnumerator()
    {
        return commands.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}