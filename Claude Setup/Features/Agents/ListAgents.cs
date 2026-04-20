using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Agents;

public sealed class ListAgents(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
{
    public async Task<IReadOnlyList<AgentSummary>> HandleAsync(bool isGlobal = false)
    {
        var agentsPath = pathResolver.GetAgentsPath(isGlobal);

        if (!Directory.Exists(agentsPath))
        {
            return [];
        }

        var agentFiles = Directory.GetFiles(agentsPath, "*.md");
        var agents = new List<AgentSummary>();

        foreach (var file in agentFiles)
        {
            var agent = await fileReader.ReadAgentAsync(file);
            if (agent is not null)
            {
                agents.Add(new AgentSummary(agent.Name, agent.Metadata?.Role, file));
            }
        }

        return agents;
    }
}
