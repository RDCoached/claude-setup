using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Deploy;

public sealed class DeployConfiguration(
    CategoryPathResolver categoryPathResolver,
    BackupStrategy backupStrategy,
    FileCopier fileCopier,
    ClaudePathResolver pathResolver,
    TimeProvider timeProvider)
{
    public async Task<DeploymentResult> HandleAsync(DeployOptions options)
    {
        var errors = new List<string>();
        var filesDeployed = 0;
        var filesBackedUp = 0;
        string? backupPath = null;

        try
        {
            // Create backup if requested
            if (options.BackupExisting)
            {
                var globalRoot = pathResolver.GetGlobalClaudePath();
                if (Directory.Exists(globalRoot))
                {
                    var backupRoot = Path.Combine(Path.GetTempPath(), "claude-backups");
                    Directory.CreateDirectory(backupRoot);
                    backupPath = await backupStrategy.CreateBackupAsync(globalRoot, backupRoot, timeProvider.GetUtcNow());
                    filesBackedUp = Directory.GetFiles(backupPath, "*", SearchOption.AllDirectories).Length;
                }
            }

            // Determine categories to deploy
            var categories = options.IncludeCategories ?? CategoryPathResolver.AllCategories;

            // Deploy each category
            foreach (var category in categories)
            {
                var localPath = categoryPathResolver.GetLocalPath(category);
                var globalPath = categoryPathResolver.GetGlobalPath(category);

                if (!Directory.Exists(localPath))
                {
                    continue;
                }

                if (!options.DryRun)
                {
                    filesDeployed += await fileCopier.CopyDirectoryAsync(localPath, globalPath);
                }
            }

            return new DeploymentResult(
                Success: true,
                FilesDeployed: filesDeployed,
                FilesBackedUp: filesBackedUp,
                Errors: errors,
                BackupPath: backupPath
            );
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
            return new DeploymentResult(
                Success: false,
                FilesDeployed: filesDeployed,
                FilesBackedUp: filesBackedUp,
                Errors: errors,
                BackupPath: backupPath
            );
        }
    }
}
