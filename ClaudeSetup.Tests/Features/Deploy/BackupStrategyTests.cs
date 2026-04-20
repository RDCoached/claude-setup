using Claude_Setup.Features.Deploy;
using FluentAssertions;

namespace ClaudeSetup.Tests.Features.Deploy;

public sealed class BackupStrategyTests
{
    [Fact]
    public async Task CreateBackupAsync_CreatesTimestampedFolder()
    {
        // Arrange
        var sourceDir = Path.Combine(Path.GetTempPath(), $"source-{Guid.NewGuid()}");
        Directory.CreateDirectory(sourceDir);
        File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "test content");

        var backupRoot = Path.Combine(Path.GetTempPath(), $"backups-{Guid.NewGuid()}");
        Directory.CreateDirectory(backupRoot);

        var timestamp = DateTimeOffset.UtcNow;
        var strategy = new BackupStrategy();

        try
        {
            // Act
            var backupPath = await strategy.CreateBackupAsync(sourceDir, backupRoot, timestamp);

            // Assert
            backupPath.Should().NotBeNullOrEmpty();
            Directory.Exists(backupPath).Should().BeTrue();
            backupPath.Should().Contain($"claude-backup-{timestamp:yyyyMMdd-HHmmss}");
        }
        finally
        {
            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (Directory.Exists(backupRoot)) Directory.Delete(backupRoot, true);
        }
    }

    [Fact]
    public async Task CreateBackupAsync_CopiesAllFilesRecursively()
    {
        // Arrange
        var sourceDir = Path.Combine(Path.GetTempPath(), $"source-{Guid.NewGuid()}");
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(Path.Combine(sourceDir, "subdir"));
        File.WriteAllText(Path.Combine(sourceDir, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(sourceDir, "subdir", "file2.txt"), "content2");

        var backupRoot = Path.Combine(Path.GetTempPath(), $"backups-{Guid.NewGuid()}");
        Directory.CreateDirectory(backupRoot);

        var timestamp = DateTimeOffset.UtcNow;
        var strategy = new BackupStrategy();

        try
        {
            // Act
            var backupPath = await strategy.CreateBackupAsync(sourceDir, backupRoot, timestamp);

            // Assert
            File.Exists(Path.Combine(backupPath, "file1.txt")).Should().BeTrue();
            File.Exists(Path.Combine(backupPath, "subdir", "file2.txt")).Should().BeTrue();
            File.ReadAllText(Path.Combine(backupPath, "file1.txt")).Should().Be("content1");
            File.ReadAllText(Path.Combine(backupPath, "subdir", "file2.txt")).Should().Be("content2");
        }
        finally
        {
            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (Directory.Exists(backupRoot)) Directory.Delete(backupRoot, true);
        }
    }

    [Fact]
    public async Task CreateBackupAsync_ReturnsCorrectBackupPath()
    {
        // Arrange
        var sourceDir = Path.Combine(Path.GetTempPath(), $"source-{Guid.NewGuid()}");
        Directory.CreateDirectory(sourceDir);

        var backupRoot = Path.Combine(Path.GetTempPath(), $"backups-{Guid.NewGuid()}");
        Directory.CreateDirectory(backupRoot);

        var timestamp = new DateTimeOffset(2026, 4, 20, 15, 30, 0, TimeSpan.Zero);
        var strategy = new BackupStrategy();

        try
        {
            // Act
            var backupPath = await strategy.CreateBackupAsync(sourceDir, backupRoot, timestamp);

            // Assert
            backupPath.Should().EndWith("claude-backup-20260420-153000");
            Path.GetDirectoryName(backupPath).Should().Be(backupRoot);
        }
        finally
        {
            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (Directory.Exists(backupRoot)) Directory.Delete(backupRoot, true);
        }
    }

    [Fact]
    public async Task CreateBackupAsync_EmptySource_CreatesEmptyBackup()
    {
        // Arrange
        var sourceDir = Path.Combine(Path.GetTempPath(), $"source-{Guid.NewGuid()}");
        Directory.CreateDirectory(sourceDir);

        var backupRoot = Path.Combine(Path.GetTempPath(), $"backups-{Guid.NewGuid()}");
        Directory.CreateDirectory(backupRoot);

        var timestamp = DateTimeOffset.UtcNow;
        var strategy = new BackupStrategy();

        try
        {
            // Act
            var backupPath = await strategy.CreateBackupAsync(sourceDir, backupRoot, timestamp);

            // Assert
            Directory.Exists(backupPath).Should().BeTrue();
            Directory.GetFiles(backupPath, "*", SearchOption.AllDirectories).Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (Directory.Exists(backupRoot)) Directory.Delete(backupRoot, true);
        }
    }
}
