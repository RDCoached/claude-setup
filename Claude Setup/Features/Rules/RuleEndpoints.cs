using Claude_Setup.Features.Skills;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Claude_Setup.Features.Rules;

public sealed class RuleEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rules").WithTags("Rules");

        group.MapGet("/", GetRules);
    }

    private static async Task<Ok<object>> GetRules(ListRules handler, bool isGlobal = false)
    {
        var rules = await handler.HandleAsync(isGlobal);
        return TypedResults.Ok<object>(new { rules, count = rules.Count });
    }
}
