using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Claude_Setup.Infrastructure.FileSystem;

public sealed partial class FrontmatterParser
{
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build();

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build();

    [GeneratedRegex(@"^---\s*\n(.*?)\n---\s*\n(.*)$", RegexOptions.Singleline)]
    private static partial Regex FrontmatterRegex();

    public (IReadOnlyDictionary<string, object> Metadata, string Content) Parse(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return (new Dictionary<string, object>(), string.Empty);
        }

        var match = FrontmatterRegex().Match(markdown);

        if (!match.Success)
        {
            return (new Dictionary<string, object>(), markdown);
        }

        var yamlContent = match.Groups[1].Value;
        var bodyContent = match.Groups[2].Value;

        var metadata = YamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent)
                       ?? new Dictionary<string, object>();

        return (metadata, bodyContent);
    }

    public string Serialize(IReadOnlyDictionary<string, object> metadata, string content)
    {
        var sb = new StringBuilder();

        sb.AppendLine("---");
        var yaml = YamlSerializer.Serialize(metadata);
        sb.Append(yaml);
        sb.AppendLine("---");
        sb.Append(content);

        return sb.ToString();
    }
}
