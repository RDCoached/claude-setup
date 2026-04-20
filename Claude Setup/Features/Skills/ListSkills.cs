using Claude_Setup.Domain.Models;
using Claude_Setup.Features.Shared;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Skills;

public sealed class ListSkills(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
    : EntityListHandler<Skill, SkillSummary>(pathResolver, fileReader)
{
    protected override string GetPath(bool isGlobal) =>
        PathResolver.GetSkillsPath(isGlobal);

    protected override IEnumerable<string> GetItems(string path) =>
        Directory.GetDirectories(path);

    protected override Task<Skill?> ReadEntityAsync(string path) =>
        FileReader.ReadSkillAsync(path);

    protected override SkillSummary CreateSummary(Skill skill, string path) =>
        new(skill.Name, skill.Metadata.Description, path);
}
