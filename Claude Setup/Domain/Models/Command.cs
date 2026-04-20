namespace Claude_Setup.Domain.Models;

public sealed record Command(
    string Name,
    string FilePath,
    CommandMetadata Metadata,
    string Content
);

public sealed record CommandMetadata(
    string Description
);

public sealed record CommandSummary(
    string Name,
    string Description,
    string FilePath
);
