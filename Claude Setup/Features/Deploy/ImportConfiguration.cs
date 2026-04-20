using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Deploy;

public sealed class ImportConfiguration(
    CategoryPathResolver categoryPathResolver,
    BackupStrategy backupStrategy,
    FileCopier fileCopier,
    TimeProvider timeProvider)
{
    public async Task<ImportResult> HandleAsync(ImportOptions options)
    {
        var errors = new List<string>();
        var filesImported = 0;
        var filesBackedUp = 0;
        string? backupPath = null;

        try
        {
            // Create backup of local-config if requested
            if (options.BackupLocal)
            {
                var localRoot = Directory.GetCurrentDirectory();
                var localConfigPath = Path.Combine(localRoot, "local-config");

                if (Directory.Exists(localConfigPath))
                {
                    var backupRoot = Path.Combine(Path.GetTempPath(), "claude-backups");
                    Directory.CreateDirectory(backupRoot);
                    backupPath = await backupStrategy.CreateBackupAsync(localConfigPath, backupRoot, timeProvider.GetUtcNow());
                    filesBackedUp = Directory.GetFiles(backupPath, "*", SearchOption.AllDirectories).Length;
                }
            }

            // Determine categories to import
            var categories = options.IncludeCategories ?? CategoryPathResolver.AllCategories;

            // Import each category
            foreach (var category in categories)
            {
                var globalPath = categoryPathResolver.GetGlobalPath(category);
                var localPath = categoryPathResolver.GetLocalPath(category);

                if (!Directory.Exists(globalPath))
                {
                    continue;
                }

                filesImported += await fileCopier.CopyDirectoryAsync(globalPath, localPath);
            }

            return new ImportResult(
                Success: true,
                FilesImported: filesImported,
                FilesBackedUp: filesBackedUp,
                Errors: errors,
                BackupPath: backupPath
            );
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
            return new ImportResult(
                Success: false,
                FilesImported: filesImported,
                FilesBackedUp: filesBackedUp,
                Errors: errors,
                BackupPath: backupPath
            );
        }
    }
}
