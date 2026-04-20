using Claude_Setup.Features.Skills;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Claude_Setup.Features.Agents;

public sealed class AgentEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agents").WithTags("Agents");

        group.MapGet("/", GetAgents);
    }

    private static async Task<Ok<object>> GetAgents(ListAgents handler, bool isGlobal = false)
    {
        var agents = await handler.HandleAsync(isGlobal);
        return TypedResults.Ok<object>(new { agents, count = agents.Count });
    }
}
