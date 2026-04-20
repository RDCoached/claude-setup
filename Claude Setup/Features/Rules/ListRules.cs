using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Rules;

public sealed class ListRules(
    ClaudePathResolver pathResolver,
    ClaudeFileReader fileReader,
    EntityListHelper listHelper)
{
    public Task<IReadOnlyList<RuleSummary>> HandleAsync(bool isGlobal = false) =>
        listHelper.ListEntitiesAsync(
            pathResolver.GetRulesPath(isGlobal),
            path => Directory.GetFiles(path, "*.md"),
            fileReader.ReadRuleAsync,
            (rule, path) => new RuleSummary(rule.Name, path)
        );
}
