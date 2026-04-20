using Claude_Setup.Domain.Models;

namespace Claude_Setup.Domain.Services;

public sealed class DiffCalculator
{
    public async Task<IReadOnlyList<FileDiff>> CalculateDiffAsync(string localPath, string globalPath)
    {
        var diffs = new List<FileDiff>();

        // Get all files from both directories
        var localFiles = Directory.Exists(localPath)
            ? Directory.GetFiles(localPath, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(localPath, f))
                .ToHashSet()
            : [];

        var globalFiles = Directory.Exists(globalPath)
            ? Directory.GetFiles(globalPath, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(globalPath, f))
                .ToHashSet()
            : [];

        var allFiles = localFiles.Union(globalFiles).OrderBy(f => f);

        foreach (var relativePath in allFiles)
        {
            var localFile = Path.Combine(localPath, relativePath);
            var globalFile = Path.Combine(globalPath, relativePath);

            var localExists = File.Exists(localFile);
            var globalExists = File.Exists(globalFile);

            DiffStatus status;

            if (localExists && !globalExists)
            {
                status = DiffStatus.New;
            }
            else if (!localExists && globalExists)
            {
                status = DiffStatus.Deleted;
            }
            else if (localExists && globalExists)
            {
                var localContent = await File.ReadAllBytesAsync(localFile);
                var globalContent = await File.ReadAllBytesAsync(globalFile);
                status = localContent.SequenceEqual(globalContent) ? DiffStatus.Unchanged : DiffStatus.Modified;
            }
            else
            {
                continue; // Should never happen
            }

            diffs.Add(new FileDiff(
                RelativePath: relativePath,
                Status: status,
                LocalPath: localExists ? localFile : null,
                GlobalPath: globalExists ? globalFile : null
            ));
        }

        return diffs;
    }
}
