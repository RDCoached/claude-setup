using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Agents;

public sealed class ListAgents(
    ClaudePathResolver pathResolver,
    ClaudeFileReader fileReader,
    EntityListHelper listHelper)
{
    public Task<IReadOnlyList<AgentSummary>> HandleAsync(bool isGlobal = false) =>
        listHelper.ListEntitiesAsync(
            pathResolver.GetAgentsPath(isGlobal),
            path => Directory.GetFiles(path, "*.md"),
            fileReader.ReadAgentAsync,
            (agent, path) => new AgentSummary(agent.Name, agent.Metadata?.Role, path)
        );
}
