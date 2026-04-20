namespace Claude_Setup.Infrastructure.FileSystem;

public sealed class FileCopier
{
    public async Task<int> CopyDirectoryAsync(string sourcePath, string targetPath)
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
}
