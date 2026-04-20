namespace ClaudeSetup.Tests.TestFixtures;

public sealed class TestClaudeStructure : IDisposable
{
    public string RootPath { get; }

    public TestClaudeStructure()
    {
        RootPath = Path.Combine(Path.GetTempPath(), $"claude-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(RootPath);
    }

    public string CreateSkill(string name, string? frontmatter = null, string? content = null)
    {
        var skillPath = Path.Combine(RootPath, "skills", name);
        Directory.CreateDirectory(skillPath);

        var markdown = frontmatter is not null
            ? $"""
              ---
              {frontmatter}
              ---

              {content ?? "# Test Content"}
              """
            : content ?? "# Test Content";

        var filePath = Path.Combine(skillPath, "SKILL.md");
        File.WriteAllText(filePath, markdown);
        return filePath;
    }

    public string CreateAgent(string name, string? frontmatter = null, string? content = null)
    {
        var agentsPath = Path.Combine(RootPath, "agents");
        Directory.CreateDirectory(agentsPath);

        var markdown = frontmatter is not null
            ? $"""
              ---
              {frontmatter}
              ---

              {content ?? "# Agent Content"}
              """
            : content ?? "# Agent Content";

        var filePath = Path.Combine(agentsPath, $"{name}.md");
        File.WriteAllText(filePath, markdown);
        return filePath;
    }

    public string CreateRule(string name, string content)
    {
        var rulesPath = Path.Combine(RootPath, "rules");
        Directory.CreateDirectory(rulesPath);

        var filePath = Path.Combine(rulesPath, $"{name}.md");
        File.WriteAllText(filePath, content);
        return filePath;
    }

    public string CreateCommand(string name, string? frontmatter = null, string? content = null)
    {
        var commandsPath = Path.Combine(RootPath, "commands");
        Directory.CreateDirectory(commandsPath);

        var markdown = frontmatter is not null
            ? $"""
              ---
              {frontmatter}
              ---

              {content ?? "# Command Content"}
              """
            : content ?? "# Command Content";

        var filePath = Path.Combine(commandsPath, $"{name}.md");
        File.WriteAllText(filePath, markdown);
        return filePath;
    }

    public void Dispose()
    {
        if (Directory.Exists(RootPath))
        {
            Directory.Delete(RootPath, recursive: true);
        }
    }
}
