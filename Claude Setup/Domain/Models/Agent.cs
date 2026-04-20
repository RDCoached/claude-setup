namespace Claude_Setup.Domain.Models;

public sealed record Agent(
    string Name,
    string FilePath,
    AgentMetadata? Metadata,
    string Content
);

public sealed record AgentMetadata(
    string? Role = null,
    IReadOnlyList<string>? Skills = null
);

public sealed record AgentSummary(
    string Name,
    string? Role,
    string FilePath
);
