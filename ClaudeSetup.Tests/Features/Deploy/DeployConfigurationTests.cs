using Claude_Setup.Features.Deploy;
using Claude_Setup.Infrastructure.Configuration;
using ClaudeSetup.Tests.TestFixtures;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClaudeSetup.Tests.Features.Deploy;

public sealed class DeployConfigurationTests
{
    [Fact]
    public async Task HandleAsync_WithBackup_CreatesBackup()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        Directory.CreateDirectory(globalPath);
        File.WriteAllText(Path.Combine(globalPath, "existing.txt"), "old");

        var localPath = Path.Combine(testStructure.RootPath, "local");
        Directory.CreateDirectory(localPath);

        var pathResolver = new TestDeployPathResolver(localPath, globalPath);
        var backupStrategy = new BackupStrategy();
        var timeProvider = new FakeTimeProvider();
        var deploy = new DeployConfiguration(pathResolver, backupStrategy, timeProvider);

        var options = new DeployOptions(BackupExisting: true);

        // Act
        var result = await deploy.HandleAsync(options);

        // Assert
        result.Success.Should().BeTrue();
        result.BackupPath.Should().NotBeNullOrEmpty();
        Directory.Exists(result.BackupPath).Should().BeTrue();
        result.FilesBackedUp.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HandleAsync_WithoutBackup_SkipsBackup()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        Directory.CreateDirectory(globalPath);

        var localPath = Path.Combine(testStructure.RootPath, "local");
        Directory.CreateDirectory(localPath);

        var pathResolver = new TestDeployPathResolver(localPath, globalPath);
        var backupStrategy = new BackupStrategy();
        var timeProvider = new FakeTimeProvider();
        var deploy = new DeployConfiguration(pathResolver, backupStrategy, timeProvider);

        var options = new DeployOptions(BackupExisting: false);

        // Act
        var result = await deploy.HandleAsync(options);

        // Assert
        result.Success.Should().BeTrue();
        result.BackupPath.Should().BeNull();
        result.FilesBackedUp.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_DryRun_DoesNotCopyFiles()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        Directory.CreateDirectory(globalPath);

        var localPath = Path.Combine(testStructure.RootPath, "local");
        Directory.CreateDirectory(localPath);
        File.WriteAllText(Path.Combine(localPath, "test.txt"), "content");

        var pathResolver = new TestDeployPathResolver(localPath, globalPath);
        var backupStrategy = new BackupStrategy();
        var timeProvider = new FakeTimeProvider();
        var deploy = new DeployConfiguration(pathResolver, backupStrategy, timeProvider);

        var options = new DeployOptions(DryRun: true);

        // Act
        var result = await deploy.HandleAsync(options);

        // Assert
        result.Success.Should().BeTrue($"Errors: {string.Join(", ", result.Errors)}");
        File.Exists(Path.Combine(globalPath, "test.txt")).Should().BeFalse();
        result.FilesDeployed.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_DeploysFiles_CopiesFromLocalToGlobal()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        Directory.CreateDirectory(globalPath);

        var localPath = Path.Combine(testStructure.RootPath, "local");
        Directory.CreateDirectory(localPath);
        var skillsPath = Path.Combine(localPath, "skills");
        Directory.CreateDirectory(skillsPath);
        File.WriteAllText(Path.Combine(skillsPath, "test.txt"), "new content");

        var pathResolver = new TestDeployPathResolver(localPath, globalPath);
        var backupStrategy = new BackupStrategy();
        var timeProvider = new FakeTimeProvider();
        var deploy = new DeployConfiguration(pathResolver, backupStrategy, timeProvider);

        var options = new DeployOptions(BackupExisting: false);

        // Act
        var result = await deploy.HandleAsync(options);

        // Assert
        result.Success.Should().BeTrue();
        result.FilesDeployed.Should().BeGreaterThan(0);
        File.Exists(Path.Combine(globalPath, "skills", "test.txt")).Should().BeTrue();
        File.ReadAllText(Path.Combine(globalPath, "skills", "test.txt")).Should().Be("new content");
    }

    [Fact]
    public async Task HandleAsync_SpecificCategories_OnlyDeploysThoseCategories()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var globalPath = Path.Combine(testStructure.RootPath, "global");
        Directory.CreateDirectory(globalPath);
        Directory.CreateDirectory(Path.Combine(globalPath, "skills"));
        Directory.CreateDirectory(Path.Combine(globalPath, "agents"));

        var localPath = Path.Combine(testStructure.RootPath, "local");
        Directory.CreateDirectory(localPath);
        Directory.CreateDirectory(Path.Combine(localPath, "skills"));
        Directory.CreateDirectory(Path.Combine(localPath, "agents"));
        File.WriteAllText(Path.Combine(localPath, "skills", "skill.txt"), "skill");
        File.WriteAllText(Path.Combine(localPath, "agents", "agent.txt"), "agent");

        var pathResolver = new TestDeployPathResolver(localPath, globalPath);
        var backupStrategy = new BackupStrategy();
        var timeProvider = new FakeTimeProvider();
        var deploy = new DeployConfiguration(pathResolver, backupStrategy, timeProvider);

        var options = new DeployOptions(BackupExisting: false, IncludeCategories: ["skills"]);

        // Act
        var result = await deploy.HandleAsync(options);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(Path.Combine(globalPath, "skills", "skill.txt")).Should().BeTrue();
        File.Exists(Path.Combine(globalPath, "agents", "agent.txt")).Should().BeFalse();
    }

    private sealed class TestDeployPathResolver(string localRoot, string globalRoot) : ClaudePathResolver
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
