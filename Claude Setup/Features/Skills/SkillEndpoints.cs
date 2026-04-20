using Microsoft.AspNetCore.Http.HttpResults;

namespace Claude_Setup.Features.Skills;

public sealed class SkillEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/skills").WithTags("Skills");

        group.MapGet("/", GetSkills);
    }

    private static async Task<Ok<object>> GetSkills(ListSkills handler, bool isGlobal = false)
    {
        var skills = await handler.HandleAsync(isGlobal);
        return TypedResults.Ok<object>(new { skills, count = skills.Count });
    }
}

public interface IEndpointGroup
{
    void Map(IEndpointRouteBuilder app);
}

public static class EndpointGroupExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointGroupType = typeof(IEndpointGroup);
        var assembly = typeof(Program).Assembly;

        var endpointGroups = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && endpointGroupType.IsAssignableFrom(t));

        foreach (var group in endpointGroups)
        {
            var instance = Activator.CreateInstance(group) as IEndpointGroup;
            instance?.Map(app);
        }

        return app;
    }
}
