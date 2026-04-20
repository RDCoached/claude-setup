using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Rules;

public sealed class ListRules(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
{
    public async Task<IReadOnlyList<RuleSummary>> HandleAsync(bool isGlobal = false)
    {
        var rulesPath = pathResolver.GetRulesPath(isGlobal);

        if (!Directory.Exists(rulesPath))
        {
            return [];
        }

        var ruleFiles = Directory.GetFiles(rulesPath, "*.md");
        var rules = new List<RuleSummary>();

        foreach (var file in ruleFiles)
        {
            var rule = await fileReader.ReadRuleAsync(file);
            if (rule is not null)
            {
                rules.Add(new RuleSummary(rule.Name, file));
            }
        }

        return rules;
    }
}
