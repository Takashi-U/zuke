using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Zuke.Core.Model;

namespace Zuke.Core.Markdown;

public static class FrontMatterParser
{
    public static (LawMetadata metadata, string body) Parse(string markdown)
    {
        var parsed = ParseDetailed(markdown);
        return (parsed.Metadata, parsed.Body);
    }

    public static FrontMatterParseResult ParseDetailed(string markdown)
    {
        var normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return new FrontMatterParseResult(DefaultMetadata(), markdown, false, []);
        }

        var end = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (end < 0)
        {
            return new FrontMatterParseResult(DefaultMetadata(), markdown, false, []);
        }

        var yaml = normalized[4..end];
        var bodyNormalized = normalized[(end + 5)..];

        try
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var map = deserializer.Deserialize<Dictionary<string, object>>(yaml) ?? new();
            var missing = RequiredKeys.Where(k => !map.ContainsKey(k) || string.IsNullOrWhiteSpace(map[k]?.ToString())).ToList();

            var metadata = new LawMetadata(
                map.TryGetValue("lawTitle", out var a) ? a.ToString() ?? "" : "",
                map.TryGetValue("lawNum", out var b) ? b.ToString() ?? "" : "",
                map.TryGetValue("era", out var c) ? c.ToString() ?? "" : "",
                map.TryGetValue("year", out var d) ? Convert.ToInt32(d) : 0,
                map.TryGetValue("num", out var e) ? Convert.ToInt32(e) : 0,
                map.TryGetValue("lawType", out var f) ? f.ToString() ?? "" : "",
                map.TryGetValue("lang", out var g) ? g.ToString() ?? "" : "");

            return new FrontMatterParseResult(metadata, bodyNormalized, true, missing);
        }
        catch
        {
            return new FrontMatterParseResult(DefaultMetadata(), bodyNormalized, true, [.. RequiredKeys]);
        }
    }

    public static IReadOnlyList<DiagnosticMessage> ValidateRequired(FrontMatterParseResult result, string? filePath)
    {
        if (!result.HasFrontMatter)
        {
            return [new(DiagnosticSeverity.Error, "LMD045", "Front Matterがありません。必須メタデータを指定してください。", new SourceLocation(filePath, 1, 1), [])];
        }

        if (result.MissingRequiredKeys.Count == 0)
        {
            return [];
        }

        var msg = $"必須メタデータが不足しています: {string.Join(", ", result.MissingRequiredKeys)}";
        return [new(DiagnosticSeverity.Error, "LMD045", msg, new SourceLocation(filePath, 1, 1), [])];
    }

    private static readonly string[] RequiredKeys = ["lawTitle", "lawNum", "era", "year", "num", "lawType", "lang"];

    private static LawMetadata DefaultMetadata() => new("無題", "", "Reiwa", 1, 1, "Misc", "ja");
}

public sealed record FrontMatterParseResult(LawMetadata Metadata, string Body, bool HasFrontMatter, IReadOnlyList<string> MissingRequiredKeys);
