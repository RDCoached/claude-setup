using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;
using ClaudeSetup.Tests.TestFixtures;
using FluentAssertions;

namespace ClaudeSetup.Tests.Infrastructure.FileSystem;

public sealed class ClaudeFileReaderTests
{
    [Fact]
    public async Task ReadSkillAsync_WithFrontmatter_CreatesCorrectSkillModel()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var frontmatter = """
            name: test-skill
            description: A test skill for testing
            context: testing context
            """;
        var content = "# Test Skill\n\nThis is the skill content.";
        var skillPath = testStructure.CreateSkill("test-skill", frontmatter, content);

        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);

        // Act
        var skill = await reader.ReadSkillAsync(Path.GetDirectoryName(skillPath)!);

        // Assert
        skill.Should().NotBeNull();
        skill!.Name.Should().Be("test-skill");
        skill.Metadata.Name.Should().Be("test-skill");
        skill.Metadata.Description.Should().Be("A test skill for testing");
        skill.Metadata.Context.Should().Be("testing context");
        skill.Content.Should().Contain("# Test Skill");
        skill.Content.Should().Contain("This is the skill content.");
    }

    [Fact]
    public async Task ReadSkillAsync_WithoutFrontmatter_ReturnsNull()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var content = "# Test Skill\n\nNo frontmatter here.";
        var skillPath = testStructure.CreateSkill("no-frontmatter", frontmatter: null, content: content);

        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);

        // Act
        var skill = await reader.ReadSkillAsync(Path.GetDirectoryName(skillPath)!);

        // Assert
        skill.Should().BeNull();
    }

    [Fact]
    public async Task ReadSkillAsync_MissingSkillFile_ReturnsNull()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var nonExistentPath = Path.Combine(testStructure.RootPath, "skills", "missing");
        Directory.CreateDirectory(nonExistentPath);

        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);

        // Act
        var skill = await reader.ReadSkillAsync(nonExistentPath);

        // Assert
        skill.Should().BeNull();
    }

    [Fact]
    public async Task ReadAgentAsync_WithFrontmatter_CreatesCorrectAgentModel()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var frontmatter = """
            role: test-agent
            skills:
              - skill1
              - skill2
            """;
        var content = "# Agent Instructions\n\nAgent content here.";
        var agentPath = testStructure.CreateAgent("test-agent", frontmatter, content);

        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);

        // Act
        var agent = await reader.ReadAgentAsync(agentPath);

        // Assert
        agent.Should().NotBeNull();
        agent!.Name.Should().Be("test-agent");
        agent.Metadata.Should().NotBeNull();
        agent.Metadata!.Role.Should().Be("test-agent");
        agent.Content.Should().Contain("# Agent Instructions");
        agent.Content.Should().Contain("Agent content here.");
    }

    [Fact]
    public async Task ReadAgentAsync_WithoutFrontmatter_HandlesGracefully()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var content = "# Agent\n\nNo metadata.";
        var agentPath = testStructure.CreateAgent("simple-agent", frontmatter: null, content: content);

        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);

        // Act
        var agent = await reader.ReadAgentAsync(agentPath);

        // Assert
        agent.Should().NotBeNull();
        agent!.Name.Should().Be("simple-agent");
        agent.Metadata.Should().BeNull();
        agent.Content.Should().Contain("# Agent");
    }

    [Fact]
    public async Task ReadRuleAsync_ReturnsContentCorrectly()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var content = "# Rule\n\nThis is a rule.";
        var rulePath = testStructure.CreateRule("test-rule", content);

        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);

        // Act
        var rule = await reader.ReadRuleAsync(rulePath);

        // Assert
        rule.Should().NotBeNull();
        rule!.Name.Should().Be("test-rule");
        rule.Content.Should().Be(content);
    }

    [Fact]
    public async Task ReadCommandAsync_WithFrontmatter_CreatesCorrectCommandModel()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var frontmatter = """
            description: A test command
            """;
        var content = "# Command\n\nCommand details.";
        var commandPath = testStructure.CreateCommand("test-command", frontmatter, content);

        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);

        // Act
        var command = await reader.ReadCommandAsync(commandPath);

        // Assert
        command.Should().NotBeNull();
        command!.Name.Should().Be("test-command");
        command.Metadata.Description.Should().Be("A test command");
        command.Content.Should().Contain("# Command");
        command.Content.Should().Contain("Command details.");
    }
}
