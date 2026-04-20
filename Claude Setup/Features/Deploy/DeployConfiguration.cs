using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;

namespace Claude_Setup.Features.Deploy;

public sealed class DeployConfiguration(
    ClaudePathResolver pathResolver,
    BackupStrategy backupStrategy,
    TimeProvider timeProvider)
{
    private static readonly string[] AllCategories = ["skills", "agents", "rules", "commands"];

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
            var categories = options.IncludeCategories ?? AllCategories;

            // Deploy each category
            foreach (var category in categories)
            {
                var localPath = GetLocalCategoryPath(category);
                var globalPath = GetGlobalCategoryPath(category);

                if (!Directory.Exists(localPath))
                {
                    continue;
                }

                if (!options.DryRun)
                {
                    filesDeployed += await DeployCategoryAsync(localPath, globalPath);
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

    private async Task<int> DeployCategoryAsync(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(targetPath);

        var filesCopied = 0;

        foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourcePath, file);
            var targetFile = Path.Combine(targetPath, relativePath);

            var targetDir = Path.GetDirectoryName(targetFile);
            if (targetDir is not null)
            {
                Directory.CreateDirectory(targetDir);
            }

            await Task.Run(() => File.Copy(file, targetFile, overwrite: true));
            filesCopied++;
        }

        return filesCopied;
    }

    private string GetLocalCategoryPath(string category)
    {
        return category switch
        {
            "skills" => pathResolver.GetSkillsPath(isGlobal: false),
            "agents" => pathResolver.GetAgentsPath(isGlobal: false),
            "rules" => pathResolver.GetRulesPath(isGlobal: false),
            "commands" => pathResolver.GetCommandsPath(isGlobal: false),
            _ => throw new ArgumentException($"Unknown category: {category}", nameof(category))
        };
    }

    private string GetGlobalCategoryPath(string category)
    {
        return category switch
        {
            "skills" => pathResolver.GetSkillsPath(isGlobal: true),
            "agents" => pathResolver.GetAgentsPath(isGlobal: true),
            "rules" => pathResolver.GetRulesPath(isGlobal: true),
            "commands" => pathResolver.GetCommandsPath(isGlobal: true),
            _ => throw new ArgumentException($"Unknown category: {category}", nameof(category))
        };
    }
}
