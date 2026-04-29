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
        var normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).TrimStart('\uFEFF');
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
                map.TryGetValue("year", out var d) ? Convert.ToInt32(d) : 1,
                map.TryGetValue("num", out var e) ? Convert.ToInt32(e) : 1,
                map.TryGetValue("lawType", out var f) ? f.ToString() ?? "" : "",
                map.TryGetValue("lang", out var g) ? g.ToString() ?? "" : "")
            {
                NumberStyle = map.TryGetValue("numberStyle", out var h) ? h?.ToString() ?? "kanji" : "kanji"
                ,ParagraphNumberStyle = map.TryGetValue("paragraphNumberStyle", out var pns) ? pns?.ToString() ?? "fullwidth" : "fullwidth"
            };

            return new FrontMatterParseResult(metadata, bodyNormalized, true, missing);
        }
        catch
        {
            var metadata = ParseBestEffortMetadata(yaml);
            var missing = RequiredKeys.Where(k =>
                k == "lawTitle" && string.IsNullOrWhiteSpace(metadata.LawTitle)).ToList();
            return new FrontMatterParseResult(metadata, bodyNormalized, true, missing);
        }
    }

    private static LawMetadata ParseBestEffortMetadata(string yaml)
    {
        var fallback = DefaultMetadata();
        var title = TryExtractScalar(yaml, "lawTitle") ?? fallback.LawTitle;
        var lawNum = TryExtractScalar(yaml, "lawNum") ?? fallback.LawNum;
        var era = TryExtractScalar(yaml, "era") ?? fallback.Era;
        var lawType = TryExtractScalar(yaml, "lawType") ?? fallback.LawType;
        var lang = TryExtractScalar(yaml, "lang") ?? fallback.Lang;
        var year = int.TryParse(TryExtractScalar(yaml, "year"), out var parsedYear) ? parsedYear : fallback.Year;
        var num = int.TryParse(TryExtractScalar(yaml, "num"), out var parsedNum) ? parsedNum : fallback.Num;

        return new LawMetadata(title, lawNum, era, year, num, lawType, lang)
        {
            NumberStyle = TryExtractScalar(yaml, "numberStyle") ?? fallback.NumberStyle,
            ParagraphNumberStyle = TryExtractScalar(yaml, "paragraphNumberStyle") ?? fallback.ParagraphNumberStyle
        };
    }

    private static string? TryExtractScalar(string yaml, string key)
    {
        foreach (var rawLine in yaml.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0) continue;

            var left = line[..separatorIndex].Trim();
            if (!left.Equals(key, StringComparison.Ordinal)) continue;

            var right = line[(separatorIndex + 1)..].Trim();
            if (right.Length >= 2)
            {
                if ((right[0] == '"' && right[^1] == '"') || (right[0] == '\'' && right[^1] == '\''))
                {
                    right = right[1..^1];
                }
            }

            return right;
        }

        return null;
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

    private static readonly string[] RequiredKeys = ["lawTitle"];

    public static IReadOnlyList<DiagnosticMessage> ValidateForXml(LawMetadata metadata, string? filePath)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(metadata.LawTitle)) missing.Add("lawTitle");
        if (string.IsNullOrWhiteSpace(metadata.LawNum)) missing.Add("lawNum");
        if (string.IsNullOrWhiteSpace(metadata.Era)) missing.Add("era");
        if (metadata.Year <= 0) missing.Add("year");
        if (metadata.Num <= 0) missing.Add("num");
        if (string.IsNullOrWhiteSpace(metadata.LawType)) missing.Add("lawType");
        if (string.IsNullOrWhiteSpace(metadata.Lang)) missing.Add("lang");
        if (missing.Count == 0) return [];
        return [new(DiagnosticSeverity.Error, "LMD045", $"必須メタデータが不足しています: {string.Join(", ", missing)}", new SourceLocation(filePath, 1, 1), [])];
    }

    private static LawMetadata DefaultMetadata() => new("無題", "", "Reiwa", 1, 1, "Misc", "ja");
}

public sealed record FrontMatterParseResult(LawMetadata Metadata, string Body, bool HasFrontMatter, IReadOnlyList<string> MissingRequiredKeys);
