namespace Claude_Setup.Domain.Models;

public sealed record Skill(
    string Name,
    string FolderPath,
    SkillMetadata Metadata,
    string Content
);

public sealed record SkillMetadata(
    string Name,
    string Description,
    string? Context = null,
    string? Agent = null,
    string? Model = null
);

public sealed record SkillSummary(
    string Name,
    string Description,
    string FolderPath
);
