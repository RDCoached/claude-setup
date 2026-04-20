using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;

namespace Claude_Setup.Features.Deploy;

public sealed class ImportConfiguration(
    ClaudePathResolver pathResolver,
    BackupStrategy backupStrategy,
    TimeProvider timeProvider)
{
    private static readonly string[] AllCategories = ["skills", "agents", "rules", "commands"];

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
            var categories = options.IncludeCategories ?? AllCategories;

            // Import each category
            foreach (var category in categories)
            {
                var globalPath = GetGlobalCategoryPath(category);
                var localPath = GetLocalCategoryPath(category);

                if (!Directory.Exists(globalPath))
                {
                    continue;
                }

                filesImported += await ImportCategoryAsync(globalPath, localPath);
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

    private async Task<int> ImportCategoryAsync(string sourcePath, string targetPath)
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
