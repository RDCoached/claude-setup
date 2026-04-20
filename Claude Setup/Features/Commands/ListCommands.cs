using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Commands;

public sealed class ListCommands(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
{
    public async Task<IReadOnlyList<CommandSummary>> HandleAsync(bool isGlobal = false)
    {
        var commandsPath = pathResolver.GetCommandsPath(isGlobal);

        if (!Directory.Exists(commandsPath))
        {
            return [];
        }

        var commandFiles = Directory.GetFiles(commandsPath, "*.md");
        var commands = new List<CommandSummary>();

        foreach (var file in commandFiles)
        {
            var command = await fileReader.ReadCommandAsync(file);
            if (command is not null)
            {
                commands.Add(new CommandSummary(command.Name, command.Metadata.Description, file));
            }
        }

        return commands;
    }
}
