using Claude_Setup.Features.Deploy;
using Claude_Setup.Infrastructure.Configuration;
using ClaudeSetup.Tests.TestFixtures;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClaudeSetup.Tests.Features.Deploy;

public sealed class ImportConfigurationTests
{
    [Fact]
    public async Task HandleAsync_ImportsFiles_CopiesFromGlobalToLocal()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        var skillsPath = Path.Combine(globalPath, "skills");
        Directory.CreateDirectory(skillsPath);
        File.WriteAllText(Path.Combine(skillsPath, "global-skill.txt"), "global content");

        var localPath = Path.Combine(testStructure.RootPath, "local");
        Directory.CreateDirectory(localPath);

        var pathResolver = new TestImportPathResolver(localPath, globalPath);
        var backupStrategy = new BackupStrategy();
        var timeProvider = new FakeTimeProvider();
        var import = new ImportConfiguration(pathResolver, backupStrategy, timeProvider);

        var options = new ImportOptions(BackupLocal: false);

        // Act
        var result = await import.HandleAsync(options);

        // Assert
        result.Success.Should().BeTrue();
        result.FilesImported.Should().BeGreaterThan(0);
        File.Exists(Path.Combine(localPath, "skills", "global-skill.txt")).Should().BeTrue();
        File.ReadAllText(Path.Combine(localPath, "skills", "global-skill.txt")).Should().Be("global content");
    }

    [Fact]
    public async Task HandleAsync_WithBackup_CreatesLocalBackup()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        Directory.CreateDirectory(globalPath);

        var localPath = Path.Combine(testStructure.RootPath, "local-config");
        Directory.CreateDirectory(localPath);
        File.WriteAllText(Path.Combine(localPath, "existing.txt"), "old local");

        // Change to the test directory so local-config is found
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(testStructure.RootPath);

            var pathResolver = new TestImportPathResolver(localPath, globalPath);
            var backupStrategy = new BackupStrategy();
            var timeProvider = new FakeTimeProvider();
            var import = new ImportConfiguration(pathResolver, backupStrategy, timeProvider);

            var options = new ImportOptions(BackupLocal: true);

            // Act
            var result = await import.HandleAsync(options);

            // Assert
            result.Success.Should().BeTrue();
            result.BackupPath.Should().NotBeNullOrEmpty();
            Directory.Exists(result.BackupPath).Should().BeTrue();
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public async Task HandleAsync_SpecificCategories_OnlyImportsThoseCategories()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        Directory.CreateDirectory(Path.Combine(globalPath, "skills"));
        Directory.CreateDirectory(Path.Combine(globalPath, "agents"));
        File.WriteAllText(Path.Combine(globalPath, "skills", "skill.txt"), "skill");
        File.WriteAllText(Path.Combine(globalPath, "agents", "agent.txt"), "agent");

        var localPath = Path.Combine(testStructure.RootPath, "local");
        Directory.CreateDirectory(localPath);

        var pathResolver = new TestImportPathResolver(localPath, globalPath);
        var backupStrategy = new BackupStrategy();
        var timeProvider = new FakeTimeProvider();
        var import = new ImportConfiguration(pathResolver, backupStrategy, timeProvider);

        var options = new ImportOptions(BackupLocal: false, IncludeCategories: ["skills"]);

        // Act
        var result = await import.HandleAsync(options);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(Path.Combine(localPath, "skills", "skill.txt")).Should().BeTrue();
        File.Exists(Path.Combine(localPath, "agents", "agent.txt")).Should().BeFalse();
    }

    private sealed class TestImportPathResolver(string localRoot, string globalRoot) : ClaudePathResolver
    {
        public override string GetGlobalClaudePath() => globalRoot;

        public override string GetSkillsPath(bool isGlobal)
        {
            return Path.Combine(isGlobal ? globalRoot : localRoot, "skills");
        }

        public override string GetAgentsPath(bool isGlobal)
        {
            return Path.Combine(isGlobal ? globalRoot : localRoot, "agents");
        }

        public override string GetRulesPath(bool isGlobal)
        {
            return Path.Combine(isGlobal ? globalRoot : localRoot, "rules");
        }

        public override string GetCommandsPath(bool isGlobal)
        {
            return Path.Combine(isGlobal ? globalRoot : localRoot, "commands");
        }
    }
}
