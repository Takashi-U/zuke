using System.Text;

namespace Zuke.Core.Importing;

public sealed class ImportReportRenderer
{
    public string Render(string input, string output, LawtextImportOptions options, LawtextImportResult result)
    {
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
        sb.AppendLine("## roundtrip check結果");
        sb.AppendLine(result.Diagnostics.Any(x => x.Code == "LMD098") ? "- 失敗" : "- 成功");
        return sb.ToString();
    }
}
