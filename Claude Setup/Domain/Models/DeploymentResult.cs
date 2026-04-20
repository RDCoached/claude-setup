namespace Claude_Setup.Domain.Models;

public sealed record DeploymentResult(
    bool Success,
    int FilesDeployed,
    int FilesBackedUp,
    IReadOnlyList<string> Errors,
    string? BackupPath
);

public sealed record ImportResult(
    bool Success,
    int FilesImported,
    int FilesBackedUp,
    IReadOnlyList<string> Errors,
    string? BackupPath
);
