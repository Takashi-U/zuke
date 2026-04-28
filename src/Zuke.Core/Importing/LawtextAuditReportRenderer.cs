using System.Text;

namespace Zuke.Core.Importing;

public sealed class LawtextAuditReportRenderer
{
    public string RenderMarkdown(LawtextAuditResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Lawtext Audit Report");
        sb.AppendLine();
        foreach (var d in result.Diagnostics)
        {
            sb.AppendLine($"- {d.Severity} {d.Code}: {d.Message} ({d.Location?.Line ?? 1}:{d.Location?.Column ?? 1})");
        }

        return sb.ToString();
    }
}
