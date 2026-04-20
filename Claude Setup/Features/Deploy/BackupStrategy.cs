namespace Claude_Setup.Features.Deploy;

public sealed class BackupStrategy
{
    public async Task<string> CreateBackupAsync(string sourcePath, string backupRoot, DateTimeOffset timestamp)
    {
        var backupFolderName = $"claude-backup-{timestamp:yyyyMMdd-HHmmss}";
        var backupPath = Path.Combine(backupRoot, backupFolderName);

        Directory.CreateDirectory(backupPath);

        if (Directory.Exists(sourcePath))
        {
            await CopyDirectoryAsync(sourcePath, backupPath);
        }

        return backupPath;
    }

    private static async Task CopyDirectoryAsync(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(targetDir, fileName);
            await Task.Run(() => File.Copy(file, targetFile, overwrite: true));
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);
            var targetSubDir = Path.Combine(targetDir, dirName);
            await CopyDirectoryAsync(subDir, targetSubDir);
        }
    }
}
