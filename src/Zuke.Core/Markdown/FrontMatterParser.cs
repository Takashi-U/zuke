using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Zuke.Core.Model;

namespace Zuke.Core.Markdown;

public static class FrontMatterParser
{
    public static (LawMetadata metadata, string body) Parse(string markdown)
    {
        if (!markdown.StartsWith("---\n", StringComparison.Ordinal)) return (DefaultMetadata(), markdown);
        var end = markdown.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (end < 0) return (DefaultMetadata(), markdown);
        var yaml = markdown[4..end];
        var body = markdown[(end + 5)..];
        try
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var map = deserializer.Deserialize<Dictionary<string, object>>(yaml) ?? new();
            return (new LawMetadata(
                map.TryGetValue("lawTitle", out var a) ? a.ToString() ?? "無題" : "無題",
                map.TryGetValue("lawNum", out var b) ? b.ToString() ?? "" : "",
                map.TryGetValue("era", out var c) ? c.ToString() ?? "Reiwa" : "Reiwa",
                map.TryGetValue("year", out var d) ? Convert.ToInt32(d) : 1,
                map.TryGetValue("num", out var e) ? Convert.ToInt32(e) : 1,
                map.TryGetValue("lawType", out var f) ? f.ToString() ?? "Misc" : "Misc",
                map.TryGetValue("lang", out var g) ? g.ToString() ?? "ja" : "ja"), body);
        }
        catch
        {
            return (DefaultMetadata(), body);
        }
    }

    private static LawMetadata DefaultMetadata() => new("無題", "", "Reiwa", 1, 1, "Misc", "ja");
}
