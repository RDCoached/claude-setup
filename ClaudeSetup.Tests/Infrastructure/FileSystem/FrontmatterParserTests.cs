using FluentAssertions;

namespace ClaudeSetup.Tests.Infrastructure.FileSystem;

public sealed class FrontmatterParserTests
{
    [Fact]
    public void Parse_WithFrontmatter_ExtractsMetadataAndContent()
    {
        // Arrange
        const string markdown = """
            ---
            name: test-skill
            description: A test skill
            context: testing
            ---

            # Test Skill

            This is the content.
            """;
        var parser = new Claude_Setup.Infrastructure.FileSystem.FrontmatterParser();

        // Act
        var (metadata, content) = parser.Parse(markdown);

        // Assert
        metadata.Should().ContainKey("name").WhoseValue.Should().Be("test-skill");
        metadata.Should().ContainKey("description").WhoseValue.Should().Be("A test skill");
        metadata.Should().ContainKey("context").WhoseValue.Should().Be("testing");
        content.Should().Contain("# Test Skill");
        content.Should().Contain("This is the content.");
    }

    [Fact]
    public void Parse_WithoutFrontmatter_ReturnsEmptyMetadataAndFullContent()
    {
        // Arrange
        const string markdown = """
            # Test Skill

            This is the content without frontmatter.
            """;
        var parser = new Claude_Setup.Infrastructure.FileSystem.FrontmatterParser();

        // Act
        var (metadata, content) = parser.Parse(markdown);

        // Assert
        metadata.Should().BeEmpty();
        content.Should().Be(markdown);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyMetadataAndEmptyContent()
    {
        // Arrange
        const string markdown = "";
        var parser = new Claude_Setup.Infrastructure.FileSystem.FrontmatterParser();

        // Act
        var (metadata, content) = parser.Parse(markdown);

        // Assert
        metadata.Should().BeEmpty();
        content.Should().BeEmpty();
    }

    [Fact]
    public void Serialize_MetadataAndContent_ProducesValidFrontmatter()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["name"] = "test-skill",
            ["description"] = "A test skill",
            ["model"] = "sonnet"
        };
        const string content = "# Test Skill\n\nContent here.";
        var parser = new Claude_Setup.Infrastructure.FileSystem.FrontmatterParser();

        // Act
        var markdown = parser.Serialize(metadata, content);

        // Assert
        markdown.Should().StartWith("---\n");
        markdown.Should().Contain("name: test-skill");
        markdown.Should().Contain("description: A test skill");
        markdown.Should().Contain("model: sonnet");
        markdown.Should().Contain("---\n");
        markdown.Should().EndWith("# Test Skill\n\nContent here.");
    }

    [Fact]
    public void RoundTrip_ParseThenSerialize_PreservesData()
    {
        // Arrange
        const string original = """
            ---
            name: round-trip-test
            description: Testing round trip
            ---

            # Content

            Some content here.
            """;
        var parser = new Claude_Setup.Infrastructure.FileSystem.FrontmatterParser();

        // Act
        var (metadata, content) = parser.Parse(original);
        var reserialized = parser.Serialize(metadata, content);
        var (metadata2, content2) = parser.Parse(reserialized);

        // Assert
        metadata2.Should().BeEquivalentTo(metadata);
        content2.Trim().Should().Be(content.Trim());
    }
}
