using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Rules;

public sealed class ListRules(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
    : EntityListHandler<Rule, RuleSummary>(pathResolver, fileReader)
{
    protected override string GetPath(bool isGlobal) =>
        PathResolver.GetRulesPath(isGlobal);

    protected override IEnumerable<string> GetItems(string path) =>
        Directory.GetFiles(path, "*.md");

    protected override Task<Rule?> ReadEntityAsync(string path) =>
        FileReader.ReadRuleAsync(path);

    protected override RuleSummary CreateSummary(Rule rule, string path) =>
        new(rule.Name, path);
}
