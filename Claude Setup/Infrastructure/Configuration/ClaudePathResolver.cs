namespace Claude_Setup.Infrastructure.Configuration;

public sealed class ClaudePathResolver
{
    private readonly string _homeDirectory;
    private readonly string _workingDirectory;

    public ClaudePathResolver()
    {
        _homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _workingDirectory = Directory.GetCurrentDirectory();
    }

    public string GetGlobalClaudePath()
    {
        return Path.Combine(_homeDirectory, ".claude");
    }

    public string GetLocalPath(string category)
    {
        return Path.Combine(_workingDirectory, "local-config", category);
    }

    public string GetGlobalPath(string category)
    {
        return Path.Combine(GetGlobalClaudePath(), category);
    }

    public string GetSkillsPath(bool isGlobal)
    {
        return isGlobal ? GetGlobalPath("skills") : GetLocalPath("skills");
    }

    public string GetAgentsPath(bool isGlobal)
    {
        return isGlobal ? GetGlobalPath("agents") : GetLocalPath("agents");
    }

    public string GetRulesPath(bool isGlobal)
    {
        return isGlobal ? GetGlobalPath("rules") : GetLocalPath("rules");
    }

    public string GetCommandsPath(bool isGlobal)
    {
        return isGlobal ? GetGlobalPath("commands") : GetLocalPath("commands");
    }

    public string GetClaudeMdPath(bool isGlobal)
    {
        return isGlobal
            ? Path.Combine(GetGlobalClaudePath(), "CLAUDE.md")
            : Path.Combine(_workingDirectory, "local-config", "CLAUDE.md");
    }

    public string GetSettingsPath(bool isGlobal)
    {
        return isGlobal
            ? Path.Combine(GetGlobalClaudePath(), "settings.json")
            : Path.Combine(_workingDirectory, "local-config", "settings.json");
    }
}
