using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Commands;

public sealed class ListCommands(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
    : EntityListHandler<Command, CommandSummary>(pathResolver, fileReader)
{
    protected override string GetPath(bool isGlobal) =>
        PathResolver.GetCommandsPath(isGlobal);

    protected override IEnumerable<string> GetItems(string path) =>
        Directory.GetFiles(path, "*.md");

    protected override Task<Command?> ReadEntityAsync(string path) =>
        FileReader.ReadCommandAsync(path);

    protected override CommandSummary CreateSummary(Command command, string path) =>
        new(command.Name, command.Metadata.Description, path);
}
