using FluentAssertions;

namespace ClaudeSetup.Tests.Infrastructure.Configuration;

public sealed class ClaudePathResolverTests
{
    [Fact]
    public void GetGlobalClaudePath_ReturnsHomeDirectoryWithClaudeFolder()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetGlobalClaudePath();

        // Assert
        path.Should().NotBeNullOrEmpty();
        path.Should().Contain(".claude");
        Path.IsPathRooted(path).Should().BeTrue();
    }

    [Fact]
    public void GetLocalPath_Skills_ReturnsLocalConfigSkillsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetLocalPath("skills");

        // Assert
        path.Should().EndWith(Path.Combine("local-config", "skills"));
    }

    [Fact]
    public void GetLocalPath_Agents_ReturnsLocalConfigAgentsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetLocalPath("agents");

        // Assert
        path.Should().EndWith(Path.Combine("local-config", "agents"));
    }

    [Fact]
    public void GetGlobalPath_Skills_ReturnsGlobalClaudeSkillsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetGlobalPath("skills");

        // Assert
        path.Should().Contain(".claude");
        path.Should().EndWith("skills");
    }

    [Fact]
    public void GetGlobalPath_Agents_ReturnsGlobalClaudeAgentsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetGlobalPath("agents");

        // Assert
        path.Should().Contain(".claude");
        path.Should().EndWith("agents");
    }

    [Fact]
    public void GetSkillsPath_Local_ReturnsLocalSkillsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetSkillsPath(isGlobal: false);

        // Assert
        path.Should().EndWith(Path.Combine("local-config", "skills"));
    }

    [Fact]
    public void GetSkillsPath_Global_ReturnsGlobalSkillsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetSkillsPath(isGlobal: true);

        // Assert
        path.Should().Contain(".claude");
        path.Should().EndWith("skills");
    }

    [Fact]
    public void GetAgentsPath_Local_ReturnsLocalAgentsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetAgentsPath(isGlobal: false);

        // Assert
        path.Should().EndWith(Path.Combine("local-config", "agents"));
    }

    [Fact]
    public void GetRulesPath_Local_ReturnsLocalRulesPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetRulesPath(isGlobal: false);

        // Assert
        path.Should().EndWith(Path.Combine("local-config", "rules"));
    }

    [Fact]
    public void GetCommandsPath_Local_ReturnsLocalCommandsPath()
    {
        // Arrange
        var resolver = new Claude_Setup.Infrastructure.Configuration.ClaudePathResolver();

        // Act
        var path = resolver.GetCommandsPath(isGlobal: false);

        // Assert
        path.Should().EndWith(Path.Combine("local-config", "commands"));
    }
}
