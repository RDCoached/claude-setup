using Claude_Setup.Domain.Models;

namespace Claude_Setup.Infrastructure.FileSystem;

public sealed class ClaudeFileWriter(FrontmatterParser parser)
{
    public async Task WriteSkillAsync(Skill skill)
    {
        Directory.CreateDirectory(skill.FolderPath);

        var metadata = new Dictionary<string, object>
        {
            ["name"] = skill.Metadata.Name,
            ["description"] = skill.Metadata.Description
        };

        if (skill.Metadata.Context is not null)
        {
            metadata["context"] = skill.Metadata.Context;
        }

        if (skill.Metadata.Agent is not null)
        {
            metadata["agent"] = skill.Metadata.Agent;
        }

        if (skill.Metadata.Model is not null)
        {
            metadata["model"] = skill.Metadata.Model;
        }

        var markdown = parser.Serialize(metadata, skill.Content);
        var filePath = Path.Combine(skill.FolderPath, "SKILL.md");
        await File.WriteAllTextAsync(filePath, markdown);
    }

    public async Task WriteAgentAsync(Agent agent)
    {
        var directory = Path.GetDirectoryName(agent.FilePath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        string markdown;

        if (agent.Metadata is not null)
        {
            var metadata = new Dictionary<string, object>();

            if (agent.Metadata.Role is not null)
            {
                metadata["role"] = agent.Metadata.Role;
            }

            if (agent.Metadata.Skills is not null && agent.Metadata.Skills.Count > 0)
            {
                metadata["skills"] = agent.Metadata.Skills.ToList();
            }

            markdown = parser.Serialize(metadata, agent.Content);
        }
        else
        {
            markdown = agent.Content;
        }

        await File.WriteAllTextAsync(agent.FilePath, markdown);
    }

    public async Task WriteRuleAsync(Rule rule)
    {
        var directory = Path.GetDirectoryName(rule.FilePath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(rule.FilePath, rule.Content);
    }

    public async Task WriteCommandAsync(Command command)
    {
        var directory = Path.GetDirectoryName(command.FilePath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        var metadata = new Dictionary<string, object>
        {
            ["description"] = command.Metadata.Description
        };

        var markdown = parser.Serialize(metadata, command.Content);
        await File.WriteAllTextAsync(command.FilePath, markdown);
    }
}
