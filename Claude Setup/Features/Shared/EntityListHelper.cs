namespace Claude_Setup.Features.Shared;

public sealed class EntityListHelper
{
    public async Task<IReadOnlyList<TSummary>> ListEntitiesAsync<TEntity, TSummary>(
        string path,
        Func<string, IEnumerable<string>> getItems,
        Func<string, Task<TEntity?>> readEntity,
        Func<TEntity, string, TSummary> createSummary)
        where TEntity : class
    {
        if (!Directory.Exists(path))
        {
            return [];
        }

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
