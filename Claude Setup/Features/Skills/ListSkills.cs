using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Skills;

public sealed class ListSkills(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
{
    public async Task<IReadOnlyList<SkillSummary>> HandleAsync(bool isGlobal = false)
    {
        var skillsPath = pathResolver.GetSkillsPath(isGlobal);

        if (!Directory.Exists(skillsPath))
        {
            return [];
        }

        var skillFolders = Directory.GetDirectories(skillsPath);
        var skills = new List<SkillSummary>();

        foreach (var folder in skillFolders)
        {
            var skill = await fileReader.ReadSkillAsync(folder);
            if (skill is not null)
            {
                skills.Add(new SkillSummary(skill.Name, skill.Metadata.Description, folder));
            }
        }

        return skills;
    }
}
