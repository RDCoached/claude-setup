using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.FileSystem;
using ClaudeSetup.Tests.TestFixtures;
using FluentAssertions;

namespace ClaudeSetup.Tests.Infrastructure.FileSystem;

public sealed class ClaudeFileWriterTests
{
    [Fact]
    public async Task WriteSkillAsync_CreatesSkillFileWithFrontmatter()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var skillsPath = Path.Combine(testStructure.RootPath, "skills");
        Directory.CreateDirectory(skillsPath);

        var metadata = new SkillMetadata("test-skill", "A test skill", "testing");
        var skill = new Skill("test-skill", Path.Combine(skillsPath, "test-skill"), metadata, "# Skill Content\n\nDetails here.");

        var parser = new FrontmatterParser();
        var writer = new ClaudeFileWriter(parser);

        // Act
        await writer.WriteSkillAsync(skill);

        // Assert
        var filePath = Path.Combine(skillsPath, "test-skill", "SKILL.md");
        File.Exists(filePath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("---");
        content.Should().Contain("name: test-skill");
        content.Should().Contain("description: A test skill");
        content.Should().Contain("context: testing");
        content.Should().Contain("# Skill Content");
    }

    [Fact]
    public async Task WriteAgentAsync_CreatesAgentFileWithFrontmatter()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var agentsPath = Path.Combine(testStructure.RootPath, "agents");
        Directory.CreateDirectory(agentsPath);

        var metadata = new AgentMetadata("test-agent", ["skill1", "skill2"]);
        var agent = new Agent("test-agent", Path.Combine(agentsPath, "test-agent.md"), metadata, "# Agent Instructions");

        var parser = new FrontmatterParser();
        var writer = new ClaudeFileWriter(parser);

        // Act
        await writer.WriteAgentAsync(agent);

        // Assert
        var filePath = Path.Combine(agentsPath, "test-agent.md");
        File.Exists(filePath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("---");
        content.Should().Contain("role: test-agent");
        content.Should().Contain("# Agent Instructions");
    }

    [Fact]
    public async Task WriteRuleAsync_CreatesRuleFile()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var rulesPath = Path.Combine(testStructure.RootPath, "rules");
        Directory.CreateDirectory(rulesPath);

        var rule = new Rule("test-rule", Path.Combine(rulesPath, "test-rule.md"), "# Rule Content");

        var parser = new FrontmatterParser();
        var writer = new ClaudeFileWriter(parser);

        // Act
        await writer.WriteRuleAsync(rule);

        // Assert
        var filePath = Path.Combine(rulesPath, "test-rule.md");
        File.Exists(filePath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Be("# Rule Content");
    }

    [Fact]
    public async Task WriteCommandAsync_CreatesCommandFileWithFrontmatter()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var commandsPath = Path.Combine(testStructure.RootPath, "commands");
        Directory.CreateDirectory(commandsPath);

        var metadata = new CommandMetadata("A test command");
        var command = new Command("test-cmd", Path.Combine(commandsPath, "test-cmd.md"), metadata, "# Command Details");

        var parser = new FrontmatterParser();
        var writer = new ClaudeFileWriter(parser);

        // Act
        await writer.WriteCommandAsync(command);

        // Assert
        var filePath = Path.Combine(commandsPath, "test-cmd.md");
        File.Exists(filePath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("---");
        content.Should().Contain("description: A test command");
        content.Should().Contain("# Command Details");
    }

    [Fact]
    public async Task RoundTrip_WriteAndRead_PreservesData()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var skillsPath = Path.Combine(testStructure.RootPath, "skills");
        Directory.CreateDirectory(skillsPath);

        var originalMetadata = new SkillMetadata("round-trip", "Round trip test", "test context", "test-agent", "sonnet");
        var originalSkill = new Skill("round-trip", Path.Combine(skillsPath, "round-trip"), originalMetadata, "# Round Trip\n\nContent.");

        var parser = new FrontmatterParser();
        var writer = new ClaudeFileWriter(parser);
        var reader = new ClaudeFileReader(parser);

        // Act
        await writer.WriteSkillAsync(originalSkill);
        var readSkill = await reader.ReadSkillAsync(originalSkill.FolderPath);

        // Assert
        readSkill.Should().NotBeNull();
        readSkill!.Name.Should().Be(originalSkill.Name);
        readSkill.Metadata.Name.Should().Be(originalMetadata.Name);
        readSkill.Metadata.Description.Should().Be(originalMetadata.Description);
        readSkill.Metadata.Context.Should().Be(originalMetadata.Context);
        readSkill.Metadata.Agent.Should().Be(originalMetadata.Agent);
        readSkill.Metadata.Model.Should().Be(originalMetadata.Model);
        readSkill.Content.Trim().Should().Be(originalSkill.Content.Trim());
    }
}
