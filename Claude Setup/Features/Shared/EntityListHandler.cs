using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Features.Shared;

public abstract class EntityListHandler<TEntity, TSummary>
    where TEntity : class
{
    protected ClaudePathResolver PathResolver { get; }
    protected ClaudeFileReader FileReader { get; }

    protected EntityListHandler(ClaudePathResolver pathResolver, ClaudeFileReader fileReader)
    {
        PathResolver = pathResolver;
        FileReader = fileReader;
    }

    public async Task<IReadOnlyList<TSummary>> HandleAsync(bool isGlobal = false)
    {
        var path = GetPath(isGlobal);
        if (!Directory.Exists(path))
            return [];

        var items = GetItems(path);
        var summaries = new List<TSummary>();

        foreach (var item in items)
        {
            var entity = await ReadEntityAsync(item);
            if (entity is not null)
                summaries.Add(CreateSummary(entity, item));
        }

        return summaries;
    }

    protected abstract string GetPath(bool isGlobal);
    protected abstract IEnumerable<string> GetItems(string path);
    protected abstract Task<TEntity?> ReadEntityAsync(string path);
    protected abstract TSummary CreateSummary(TEntity entity, string path);
}
