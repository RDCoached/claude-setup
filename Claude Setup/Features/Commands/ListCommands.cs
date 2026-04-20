using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Commands;

public sealed class ListCommands(
    ClaudePathResolver pathResolver,
    ClaudeFileReader fileReader,
    EntityListHelper listHelper)
{
    public Task<IReadOnlyList<CommandSummary>> HandleAsync(bool isGlobal = false) =>
        listHelper.ListEntitiesAsync(
            pathResolver.GetCommandsPath(isGlobal),
            path => Directory.GetFiles(path, "*.md"),
            fileReader.ReadCommandAsync,
            (command, path) => new CommandSummary(command.Name, command.Metadata.Description, path)
        );
}
