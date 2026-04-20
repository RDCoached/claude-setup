namespace Claude_Setup.Infrastructure.Configuration;

public sealed class CategoryPathResolver(ClaudePathResolver pathResolver)
{
    public static readonly IReadOnlyList<string> AllCategories = ["skills", "agents", "rules", "commands"];

    public string GetLocalPath(string category) => category switch
    {
        "skills" => pathResolver.GetSkillsPath(isGlobal: false),
        "agents" => pathResolver.GetAgentsPath(isGlobal: false),
        "rules" => pathResolver.GetRulesPath(isGlobal: false),
        "commands" => pathResolver.GetCommandsPath(isGlobal: false),
        _ => throw new ArgumentException($"Unknown category: {category}", nameof(category))
    };

    public string GetGlobalPath(string category) => category switch
    {
        "skills" => pathResolver.GetSkillsPath(isGlobal: true),
        "agents" => pathResolver.GetAgentsPath(isGlobal: true),
        "rules" => pathResolver.GetRulesPath(isGlobal: true),
        "commands" => pathResolver.GetCommandsPath(isGlobal: true),
        _ => throw new ArgumentException($"Unknown category: {category}", nameof(category))
    };
}
