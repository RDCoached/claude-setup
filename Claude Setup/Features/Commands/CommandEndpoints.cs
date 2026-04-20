using Claude_Setup.Features.Skills;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Claude_Setup.Features.Commands;

public sealed class CommandEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commands").WithTags("Commands");

        group.MapGet("/", GetCommands);
    }

    private static async Task<Ok<object>> GetCommands(ListCommands handler, bool isGlobal = false)
    {
        var commands = await handler.HandleAsync(isGlobal);
        return TypedResults.Ok<object>(new { commands, count = commands.Count });
    }
}
