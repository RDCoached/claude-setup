using Claude_Setup.Domain.Models;
using Claude_Setup.Domain.Services;
using FluentAssertions;

namespace ClaudeSetup.Tests.Domain.Services;

public sealed class DiffCalculatorTests
{
    [Fact]
    public async Task CalculateDiffAsync_NewFileInLocal_ReturnsNewStatus()
    {
        // Arrange
        var localDir = Path.Combine(Path.GetTempPath(), $"local-{Guid.NewGuid()}");
        var globalDir = Path.Combine(Path.GetTempPath(), $"global-{Guid.NewGuid()}");
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(globalDir);

        File.WriteAllText(Path.Combine(localDir, "new.txt"), "new content");

        var calculator = new DiffCalculator();

        try
        {
            // Act
            var diffs = await calculator.CalculateDiffAsync(localDir, globalDir);

            // Assert
            diffs.Should().ContainSingle();
            diffs[0].Status.Should().Be(DiffStatus.New);
            diffs[0].RelativePath.Should().Be("new.txt");
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public async Task CalculateDiffAsync_ModifiedFile_ReturnsModifiedStatus()
    {
        // Arrange
        var localDir = Path.Combine(Path.GetTempPath(), $"local-{Guid.NewGuid()}");
        var globalDir = Path.Combine(Path.GetTempPath(), $"global-{Guid.NewGuid()}");
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(globalDir);

        File.WriteAllText(Path.Combine(localDir, "file.txt"), "new content");
        File.WriteAllText(Path.Combine(globalDir, "file.txt"), "old content");

        var calculator = new DiffCalculator();

        try
        {
            // Act
            var diffs = await calculator.CalculateDiffAsync(localDir, globalDir);

            // Assert
            diffs.Should().ContainSingle();
            diffs[0].Status.Should().Be(DiffStatus.Modified);
            diffs[0].RelativePath.Should().Be("file.txt");
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public async Task CalculateDiffAsync_DeletedFile_ReturnsDeletedStatus()
    {
        // Arrange
        var localDir = Path.Combine(Path.GetTempPath(), $"local-{Guid.NewGuid()}");
        var globalDir = Path.Combine(Path.GetTempPath(), $"global-{Guid.NewGuid()}");
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(globalDir);

        File.WriteAllText(Path.Combine(globalDir, "deleted.txt"), "old content");

        var calculator = new DiffCalculator();

        try
        {
            // Act
            var diffs = await calculator.CalculateDiffAsync(localDir, globalDir);

            // Assert
            diffs.Should().ContainSingle();
            diffs[0].Status.Should().Be(DiffStatus.Deleted);
            diffs[0].RelativePath.Should().Be("deleted.txt");
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public async Task CalculateDiffAsync_UnchangedFile_ReturnsUnchangedStatus()
    {
        // Arrange
        var localDir = Path.Combine(Path.GetTempPath(), $"local-{Guid.NewGuid()}");
        var globalDir = Path.Combine(Path.GetTempPath(), $"global-{Guid.NewGuid()}");
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(globalDir);

        File.WriteAllText(Path.Combine(localDir, "same.txt"), "same content");
        File.WriteAllText(Path.Combine(globalDir, "same.txt"), "same content");

        var calculator = new DiffCalculator();

        try
        {
            // Act
            var diffs = await calculator.CalculateDiffAsync(localDir, globalDir);

            // Assert
            diffs.Should().ContainSingle();
            diffs[0].Status.Should().Be(DiffStatus.Unchanged);
            diffs[0].RelativePath.Should().Be("same.txt");
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public async Task CalculateDiffAsync_MixedChanges_ReturnsAllStatuses()
    {
        // Arrange
        var localDir = Path.Combine(Path.GetTempPath(), $"local-{Guid.NewGuid()}");
        var globalDir = Path.Combine(Path.GetTempPath(), $"global-{Guid.NewGuid()}");
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(globalDir);

        File.WriteAllText(Path.Combine(localDir, "new.txt"), "new");
        File.WriteAllText(Path.Combine(localDir, "modified.txt"), "new version");
        File.WriteAllText(Path.Combine(globalDir, "modified.txt"), "old version");
        File.WriteAllText(Path.Combine(globalDir, "deleted.txt"), "to delete");
        File.WriteAllText(Path.Combine(localDir, "unchanged.txt"), "same");
        File.WriteAllText(Path.Combine(globalDir, "unchanged.txt"), "same");

        var calculator = new DiffCalculator();

        try
        {
            // Act
            var diffs = await calculator.CalculateDiffAsync(localDir, globalDir);

            // Assert
            diffs.Should().HaveCount(4);
            diffs.Should().ContainSingle(d => d.Status == DiffStatus.New);
            diffs.Should().ContainSingle(d => d.Status == DiffStatus.Modified);
            diffs.Should().ContainSingle(d => d.Status == DiffStatus.Deleted);
            diffs.Should().ContainSingle(d => d.Status == DiffStatus.Unchanged);
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(globalDir, true);
        }
    }
}
