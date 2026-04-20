namespace Claude_Setup.Domain.Models;

public sealed record Rule(
    string Name,
    string FilePath,
    string Content
);

public sealed record RuleSummary(
    string Name,
    string FilePath
);
