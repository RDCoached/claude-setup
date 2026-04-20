using Claude_Setup.Domain.Models;

namespace Claude_Setup.Infrastructure.FileSystem;

public sealed class ClaudeFileReader(FrontmatterParser parser)
{
    public async Task<Skill?> ReadSkillAsync(string folderPath)
    {
        var skillFilePath = Path.Combine(folderPath, "SKILL.md");

        if (!File.Exists(skillFilePath))
        {
            return null;
        }

        var markdown = await File.ReadAllTextAsync(skillFilePath);
        var (metadata, content) = parser.Parse(markdown);

        if (metadata.Count == 0)
        {
            return null;
        }

        var name = Path.GetFileName(folderPath);
        var skillMetadata = new SkillMetadata(
            Name: GetString(metadata, "name") ?? name,
            Description: GetString(metadata, "description") ?? string.Empty,
            Context: GetString(metadata, "context"),
            Agent: GetString(metadata, "agent"),
            Model: GetString(metadata, "model")
        );

        return new Skill(name, folderPath, skillMetadata, content);
    }

    public async Task<Agent?> ReadAgentAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var markdown = await File.ReadAllTextAsync(filePath);
        var (metadata, content) = parser.Parse(markdown);

        var name = Path.GetFileNameWithoutExtension(filePath);

        AgentMetadata? agentMetadata = null;
        if (metadata.Count > 0)
        {
            agentMetadata = new AgentMetadata(
                Role: GetString(metadata, "role"),
                Skills: GetStringList(metadata, "skills")
            );
        }

        return new Agent(name, filePath, agentMetadata, content);
    }

    public async Task<Rule?> ReadRuleAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var content = await File.ReadAllTextAsync(filePath);
        var name = Path.GetFileNameWithoutExtension(filePath);

        return new Rule(name, filePath, content);
    }

    public async Task<Command?> ReadCommandAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var markdown = await File.ReadAllTextAsync(filePath);
        var (metadata, content) = parser.Parse(markdown);

        var name = Path.GetFileNameWithoutExtension(filePath);

        var commandMetadata = new CommandMetadata(
            Description: GetString(metadata, "description") ?? string.Empty
        );

        return new Command(name, filePath, commandMetadata, content);
    }

    private static string? GetString(IReadOnlyDictionary<string, object> metadata, string key)
    {
        return metadata.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    private static IReadOnlyList<string>? GetStringList(IReadOnlyDictionary<string, object> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            List<object> list => list.Select(x => x.ToString()!).ToList(),
            IEnumerable<object> enumerable => enumerable.Select(x => x.ToString()!).ToList(),
            _ => null
        };
    }
}
