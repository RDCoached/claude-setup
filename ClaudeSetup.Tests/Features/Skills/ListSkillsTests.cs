using Claude_Setup.Features.Skills;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;
using ClaudeSetup.Tests.TestFixtures;
using FluentAssertions;

namespace ClaudeSetup.Tests.Features.Skills;

public sealed class ListSkillsTests
{
    [Fact]
    public async Task HandleAsync_WithSkills_ReturnsAllSkills()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        testStructure.CreateSkill("skill1", "name: skill1\ndescription: First skill", "Content 1");
        testStructure.CreateSkill("skill2", "name: skill2\ndescription: Second skill", "Content 2");

        var pathResolver = new TestPathResolver(testStructure.RootPath);
        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);
        var handler = new ListSkills(pathResolver, reader);

        // Act
        var skills = await handler.HandleAsync(isGlobal: false);

        // Assert
        skills.Should().HaveCount(2);
        skills.Should().ContainSingle(s => s.Name == "skill1" && s.Description == "First skill");
        skills.Should().ContainSingle(s => s.Name == "skill2" && s.Description == "Second skill");
    }

    [Fact]
    public async Task HandleAsync_EmptyFolder_ReturnsEmptyList()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var skillsPath = Path.Combine(testStructure.RootPath, "skills");
        Directory.CreateDirectory(skillsPath);

        var pathResolver = new TestPathResolver(testStructure.RootPath);
        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);
        var handler = new ListSkills(pathResolver, reader);

        // Act
        var skills = await handler.HandleAsync(isGlobal: false);

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_MissingFolder_ReturnsEmptyList()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();

        var pathResolver = new TestPathResolver(testStructure.RootPath);
        var parser = new FrontmatterParser();
        var reader = new ClaudeFileReader(parser);
        var handler = new ListSkills(pathResolver, reader);

        // Act
        var skills = await handler.HandleAsync(isGlobal: false);

        // Assert
        skills.Should().BeEmpty();
    }

    private sealed class TestPathResolver(string rootPath) : ClaudePathResolver
    {
        public override string GetSkillsPath(bool isGlobal)
        {
            return Path.Combine(rootPath, "skills");
        }
    }
}
