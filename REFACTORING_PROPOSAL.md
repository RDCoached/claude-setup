# Code Duplication Refactoring Proposal

## Summary

Identified ~250 lines of duplicated code across 3 major areas. Proposed refactoring would reduce codebase by ~150 lines while improving maintainability.

## 1. List Handlers - Generic Entity Lister

### Current State (120 lines across 4 files)

```csharp
// ListSkills.cs, ListAgents.cs, ListRules.cs, ListCommands.cs
// All have nearly identical structure
```

### Proposed Solution A: Generic Base Class

```csharp
public abstract class EntityLister<TEntity, TSummary>(
    ClaudePathResolver pathResolver,
    ClaudeFileReader fileReader)
{
    protected abstract string GetPath(bool isGlobal);
    protected abstract IEnumerable<string> GetFilesOrFolders(string path);
    protected abstract Task<TEntity?> ReadEntityAsync(string path);
    protected abstract TSummary CreateSummary(TEntity entity, string path);

    public async Task<IReadOnlyList<TSummary>> HandleAsync(bool isGlobal = false)
    {
        var entityPath = GetPath(isGlobal);
        if (!Directory.Exists(entityPath))
            return [];

        var items = GetFilesOrFolders(entityPath);
        var summaries = new List<TSummary>();

        foreach (var item in items)
        {
            var entity = await ReadEntityAsync(item);
            if (entity is not null)
            {
                summaries.Add(CreateSummary(entity, item));
            }
        }

        return summaries;
    }
}

// Concrete implementations become tiny:
public sealed class ListSkills(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
    : EntityLister<Skill, SkillSummary>(pathResolver, fileReader)
{
    protected override string GetPath(bool isGlobal) => pathResolver.GetSkillsPath(isGlobal);
    protected override IEnumerable<string> GetFilesOrFolders(string path) => Directory.GetDirectories(path);
    protected override Task<Skill?> ReadEntityAsync(string path) => fileReader.ReadSkillAsync(path);
    protected override SkillSummary CreateSummary(Skill skill, string path) =>
        new(skill.Name, skill.Metadata.Description, path);
}
```

**Pros:**
- Reduces from 120 lines to ~60 lines
- DRY principle
- Easy to add new entity types

**Cons:**
- Slightly more complex
- Abstract methods may be overkill for simple cases

### Proposed Solution B: Helper Service

```csharp
public sealed class EntityListHelper(ClaudeFileReader fileReader)
{
    public async Task<IReadOnlyList<TSummary>> ListEntitiesAsync<TEntity, TSummary>(
        string path,
        Func<string, IEnumerable<string>> getItems,
        Func<string, Task<TEntity?>> readEntity,
        Func<TEntity, string, TSummary> createSummary)
    {
        if (!Directory.Exists(path))
            return [];

        var items = getItems(path);
        var summaries = new List<TSummary>();

        foreach (var item in items)
        {
            var entity = await readEntity(item);
            if (entity is not null)
            {
                summaries.Add(createSummary(entity, item));
            }
        }

        return summaries;
    }
}

// Usage:
public sealed class ListSkills(ClaudePathResolver pathResolver, EntityListHelper helper, ClaudeFileReader reader)
{
    public Task<IReadOnlyList<SkillSummary>> HandleAsync(bool isGlobal = false) =>
        helper.ListEntitiesAsync(
            pathResolver.GetSkillsPath(isGlobal),
            Directory.GetDirectories,
            reader.ReadSkillAsync,
            (skill, path) => new SkillSummary(skill.Name, skill.Metadata.Description, path)
        );
}
```

**Pros:**
- More flexible
- No inheritance
- Functional approach

**Cons:**
- More parameters to pass

---

## 2. Category Path Resolution

### Current State (46 lines duplicated)

```csharp
// Duplicated in both DeployConfiguration and ImportConfiguration
private string GetLocalCategoryPath(string category) { ... }
private string GetGlobalCategoryPath(string category) { ... }
private static readonly string[] AllCategories = [...];
```

### Proposed Solution: Shared Service

```csharp
// New file: Infrastructure/Configuration/CategoryPathResolver.cs
public sealed class CategoryPathResolver(ClaudePathResolver pathResolver)
{
    public static readonly IReadOnlyList<string> AllCategories = ["skills", "agents", "rules", "commands"];

    public string GetLocalPath(string category) => category switch
    {
        "skills" => pathResolver.GetSkillsPath(false),
        "agents" => pathResolver.GetAgentsPath(false),
        "rules" => pathResolver.GetRulesPath(false),
        "commands" => pathResolver.GetCommandsPath(false),
        _ => throw new ArgumentException($"Unknown category: {category}", nameof(category))
    };

    public string GetGlobalPath(string category) => category switch
    {
        "skills" => pathResolver.GetSkillsPath(true),
        "agents" => pathResolver.GetAgentsPath(true),
        "rules" => pathResolver.GetRulesPath(true),
        "commands" => pathResolver.GetCommandsPath(true),
        _ => throw new ArgumentException($"Unknown category: {category}", nameof(category))
    };
}
```

**Impact:**
- Eliminates 46 lines of duplication
- Single source of truth for categories
- Easy to add new categories

---

## 3. File Copying Logic

### Current State (22 lines duplicated)

```csharp
// Identical in DeployConfiguration and ImportConfiguration
private async Task<int> DeployCategoryAsync(string sourcePath, string targetPath) { ... }
private async Task<int> ImportCategoryAsync(string sourcePath, string targetPath) { ... }
```

### Proposed Solution: Infrastructure Service

```csharp
// New file: Infrastructure/FileSystem/FileCopier.cs
public sealed class FileCopier
{
    public async Task<int> CopyDirectoryAsync(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(targetPath);
        var filesCopied = 0;

        foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourcePath, file);
            var targetFile = Path.Combine(targetPath, relativePath);

            var targetDir = Path.GetDirectoryName(targetFile);
            if (targetDir is not null)
            {
                Directory.CreateDirectory(targetDir);
            }

            await Task.Run(() => File.Copy(file, targetFile, overwrite: true));
            filesCopied++;
        }

        return filesCopied;
    }
}
```

**Impact:**
- Eliminates 22 lines of duplication
- Reusable for other copy operations
- Easier to test independently

---

## 4. Deploy/Import Common Structure

### Proposed Solution: Base Class or Helper

```csharp
// New file: Features/Deploy/ConfigurationSyncBase.cs
public abstract class ConfigurationSyncBase(
    CategoryPathResolver categoryPathResolver,
    BackupStrategy backupStrategy,
    FileCopier fileCopier,
    TimeProvider timeProvider)
{
    protected async Task<TSyncResult> ExecuteSyncAsync<TSyncResult>(
        Func<string> getBackupPath,
        IReadOnlyList<string> categories,
        bool createBackup,
        bool dryRun,
        Func<string, string> getSourcePath,
        Func<string, string> getTargetPath,
        Func<bool, int, int, List<string>, string?, TSyncResult> createResult)
    {
        var errors = new List<string>();
        var filesSynced = 0;
        var filesBackedUp = 0;
        string? backupPath = null;

        try
        {
            // Backup logic
            if (createBackup)
            {
                var pathToBackup = getBackupPath();
                if (Directory.Exists(pathToBackup))
                {
                    var backupRoot = Path.Combine(Path.GetTempPath(), "claude-backups");
                    Directory.CreateDirectory(backupRoot);
                    backupPath = await backupStrategy.CreateBackupAsync(pathToBackup, backupRoot, timeProvider.GetUtcNow());
                    filesBackedUp = Directory.GetFiles(backupPath, "*", SearchOption.AllDirectories).Length;
                }
            }

            // Sync each category
            foreach (var category in categories)
            {
                var sourcePath = getSourcePath(category);
                var targetPath = getTargetPath(category);

                if (!Directory.Exists(sourcePath))
                    continue;

                if (!dryRun)
                {
                    filesSynced += await fileCopier.CopyDirectoryAsync(sourcePath, targetPath);
                }
            }

            return createResult(true, filesSynced, filesBackedUp, errors, backupPath);
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
            return createResult(false, filesSynced, filesBackedUp, errors, backupPath);
        }
    }
}
```

---

## Summary of Benefits

| Refactoring | Lines Saved | Maintainability | Testability |
|-------------|-------------|-----------------|-------------|
| Generic Entity Lister | ~60 lines | ⬆️ High | ⬆️ Better |
| Category Path Resolver | ~46 lines | ⬆️ High | ⬆️ Better |
| File Copier | ~22 lines | ⬆️ High | ⬆️ Better |
| Sync Base Class | ~40 lines | ⬆️ Medium | ➡️ Same |
| **Total** | **~168 lines** | **⬆️ Significant improvement** | **⬆️ Better** |

## Recommendation

**Priority 1 (High Impact, Low Risk):**
1. Extract `CategoryPathResolver` - immediate 46 line reduction, zero risk
2. Extract `FileCopier` - immediate 22 line reduction, zero risk

**Priority 2 (Medium Impact, Low Risk):**
3. Create `EntityListHelper` - 60 line reduction, low risk

**Priority 3 (Lower Priority):**
4. Consider sync base class only if more sync operations are added

## Next Steps

Would you like me to:
1. ✅ Implement Priority 1 refactorings (CategoryPathResolver + FileCopier)?
2. ✅ Implement all refactorings?
3. ❌ Keep as-is and document for future consideration?
