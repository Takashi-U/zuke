using System.Text;

namespace Zuke.Core.Importing;

public sealed class ImportReportRenderer
{
    public string Render(string input, string output, LawtextImportOptions options, LawtextImportResult result)
    {
        var generatedRefs = Extract(result.Markdown, "[条:", "[項:", "[号:");
        var convertedRefs = Extract(result.Markdown, "{{参照:");
        var unconvertedRefHints = result.Diagnostics.Where(x => x.Code is "LMD091" or "LMD092" or "LMD094" or "LMD095").Select(x => x.Message).Distinct().ToList();
        var sb = new StringBuilder();
        sb.AppendLine("# Lawtext Import Report");
        sb.AppendLine();
        sb.AppendLine($"- Input: `{input}`");
        sb.AppendLine($"- Output: `{output}`");
        sb.AppendLine($"- Reference Labels: `{options.ReferenceLabels}`");
        sb.AppendLine($"- Reference Mode: `{options.ReferenceMode}`");
        sb.AppendLine();
        sb.AppendLine("## 診断一覧");
        foreach (var d in result.Diagnostics) sb.AppendLine($"- {d.Severity} {d.Code}: {d.Message}");
        sb.AppendLine();
        sb.AppendLine("## 生成参照名一覧");
        foreach (var r in generatedRefs) sb.AppendLine($"- {r}");
        sb.AppendLine();
        sb.AppendLine("## 変換した参照表現一覧");
        foreach (var r in convertedRefs) sb.AppendLine($"- {r}");
        sb.AppendLine();
        sb.AppendLine("## 未変換の参照表現一覧");
        foreach (var r in unconvertedRefHints) sb.AppendLine($"- {r}");
        sb.AppendLine();
        sb.AppendLine("## roundtrip check結果");
        sb.AppendLine(result.Diagnostics.Any(x => x.Code == "LMD098") ? "- 失敗" : "- 成功");
        sb.AppendLine();
        sb.AppendLine("## XSD検証結果");
        sb.AppendLine(result.Diagnostics.Any(x => x.Code == "XSD001" && x.Severity.ToString() == "Error") ? "- 失敗" : "- 成功");
        sb.AppendLine();
        sb.AppendLine("## 再実行コマンド例");
        sb.AppendLine($"`zuke import {input} -o {output} --reference-labels {options.ReferenceLabels} --reference-mode {options.ReferenceMode}`");
        return sb.ToString();

        static IReadOnlyList<string> Extract(string markdown, params string[] prefixes)
            => markdown.Split('\n')
                .SelectMany(line => prefixes.Select(prefix => (prefix, line.IndexOf(prefix, StringComparison.Ordinal))).Where(x => x.Item2 >= 0).Select(x => line[x.Item2..].Split(']').FirstOrDefault() + "]"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToList()!;
    }
}
