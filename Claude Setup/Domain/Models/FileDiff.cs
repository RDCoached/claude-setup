namespace Claude_Setup.Domain.Models;

public sealed record FileDiff(
    string RelativePath,
    DiffStatus Status,
    string? LocalPath,
    string? GlobalPath
);

public enum DiffStatus
{
    New,
    Modified,
    Deleted,
    Unchanged
}
