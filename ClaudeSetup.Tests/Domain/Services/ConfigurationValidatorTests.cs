using Claude_Setup.Domain.Services;
using Claude_Setup.Infrastructure.FileSystem;
using ClaudeSetup.Tests.TestFixtures;
using FluentAssertions;

namespace ClaudeSetup.Tests.Domain.Services;

public sealed class ConfigurationValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ValidSkill_ReturnsValid()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var frontmatter = "name: test-skill\ndescription: Test skill";
        testStructure.CreateSkill("valid-skill", frontmatter, "Content");

        var parser = new FrontmatterParser();
        var validator = new ConfigurationValidator(parser);

        // Act
        var result = await validator.ValidateAsync(testStructure.RootPath);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_SkillWithoutFrontmatter_ReturnsError()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        testStructure.CreateSkill("invalid-skill", frontmatter: null, "No frontmatter");

        var parser = new FrontmatterParser();
        var validator = new ConfigurationValidator(parser);

        // Act
        var result = await validator.ValidateAsync(testStructure.RootPath);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Message.Should().Contain("frontmatter");
    }

    [Fact]
    public async Task ValidateAsync_SkillMissingName_ReturnsError()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();
        var frontmatter = "description: Missing name field";
        testStructure.CreateSkill("missing-name", frontmatter, "Content");

        var parser = new FrontmatterParser();
        var validator = new ConfigurationValidator(parser);

        // Act
        var result = await validator.ValidateAsync(testStructure.RootPath);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Message.Should().Contain("name");
    }

    [Fact]
    public async Task ValidateAsync_EmptyDirectory_ReturnsValid()
    {
        // Arrange
        using var testStructure = new TestClaudeStructure();

        var parser = new FrontmatterParser();
        var validator = new ConfigurationValidator(parser);

        // Act
        var result = await validator.ValidateAsync(testStructure.RootPath);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
