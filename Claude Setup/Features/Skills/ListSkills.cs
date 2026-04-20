using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Skills;

public sealed class ListSkills(
    ClaudePathResolver pathResolver,
    ClaudeFileReader fileReader,
    EntityListHelper listHelper)
{
    public Task<IReadOnlyList<SkillSummary>> HandleAsync(bool isGlobal = false) =>
        listHelper.ListEntitiesAsync(
            pathResolver.GetSkillsPath(isGlobal),
            Directory.GetDirectories,
            fileReader.ReadSkillAsync,
            (skill, path) => new SkillSummary(skill.Name, skill.Metadata.Description, path)
        );
}
