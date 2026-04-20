using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Agents;

public sealed class ListAgents(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
    : EntityListHandler<Agent, AgentSummary>(pathResolver, fileReader)
{
    protected override string GetPath(bool isGlobal) =>
        PathResolver.GetAgentsPath(isGlobal);

    protected override IEnumerable<string> GetItems(string path) =>
        Directory.GetFiles(path, "*.md");

    protected override Task<Agent?> ReadEntityAsync(string path) =>
        FileReader.ReadAgentAsync(path);

    protected override AgentSummary CreateSummary(Agent agent, string path) =>
        new(agent.Name, agent.Metadata?.Role, path);
}
