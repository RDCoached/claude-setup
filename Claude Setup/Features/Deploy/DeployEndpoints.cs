using Claude_Setup.Domain.Services;
using Claude_Setup.Features.Skills;
using Claude_Setup.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Claude_Setup.Features.Deploy;

public sealed class DeployEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/deploy").WithTags("Deploy");

        group.MapPost("/", Deploy);
        group.MapPost("/import", Import);
        group.MapGet("/diff", GetDiff);
        group.MapGet("/validate", Validate);
    }

    private static async Task<Ok<object>> Deploy(
        DeployConfiguration handler,
        bool backup = true,
        bool dryRun = false)
    {
        var options = new DeployOptions(BackupExisting: backup, DryRun: dryRun);
        var result = await handler.HandleAsync(options);
        return TypedResults.Ok<object>(result);
    }

    private static async Task<Ok<object>> Import(
        ImportConfiguration handler,
        bool backup = true)
    {
        var options = new ImportOptions(BackupLocal: backup);
        var result = await handler.HandleAsync(options);
        return TypedResults.Ok<object>(result);
    }

    private static async Task<Ok<object>> GetDiff(
        DiffCalculator calculator,
        ClaudePathResolver pathResolver)
    {
        var localRoot = Path.Combine(Directory.GetCurrentDirectory(), "local-config");
        var globalRoot = pathResolver.GetGlobalClaudePath();

        var diffs = await calculator.CalculateDiffAsync(localRoot, globalRoot);

        var summary = new
        {
            totalFiles = diffs.Count,
            newFiles = diffs.Count(d => d.Status == Domain.Models.DiffStatus.New),
            modifiedFiles = diffs.Count(d => d.Status == Domain.Models.DiffStatus.Modified),
            deletedFiles = diffs.Count(d => d.Status == Domain.Models.DiffStatus.Deleted),
            unchangedFiles = diffs.Count(d => d.Status == Domain.Models.DiffStatus.Unchanged),
            files = diffs
        };

        return TypedResults.Ok<object>(summary);
    }

    private static async Task<Ok<object>> Validate(
        ConfigurationValidator validator)
    {
        var localRoot = Path.Combine(Directory.GetCurrentDirectory(), "local-config");
        var result = await validator.ValidateAsync(localRoot);
        return TypedResults.Ok<object>(result);
    }
}
