using System.Net;
using System.Text;

namespace Zuke.Core.Diff;

public sealed class HtmlDiffRenderer
{
    public string Render(string oldName, string newName, DiffResult r)
    {
        var rows = r.Hunks.SelectMany(h => h.Lines).ToList();
        var added = rows.Count(l => l.Kind == '+');
        var removed = rows.Count(l => l.Kind == '-');

        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset='utf-8'><style>");
        sb.Append("body{font-family:-apple-system,BlinkMacSystemFont,Segoe UI,sans-serif;padding:16px;color:#24292f;}table{width:100%;border-collapse:collapse;table-layout:fixed;font-family:ui-monospace,SFMono-Regular,Menlo,monospace;font-size:12px;}th,td{border:1px solid #d0d7de;padding:4px 8px;vertical-align:top;white-space:pre-wrap;word-break:break-word;}thead th{background:#f6f8fa;} .ln{width:56px;color:#57606a;text-align:right;background:#f6f8fa;} .del{background:#ffebe9;} .add{background:#dafbe1;} .ctx{background:#ffffff;} .wrap{display:grid;grid-template-columns:1fr;gap:12px;} .summary{margin-bottom:12px;} h2{margin:0 0 8px;} ");
        sb.Append("</style></head><body>");
        sb.Append($"<h2>Diff: {WebUtility.HtmlEncode(oldName)} → {WebUtility.HtmlEncode(newName)}</h2>");
        sb.Append($"<div class='summary'>追加 <b>{added}</b> / 削除 <b>{removed}</b> / Hunk <b>{r.Hunks.Count}</b></div>");
        sb.Append("<div class='wrap'><table><thead><tr><th class='ln'>-</th><th>変更前</th><th class='ln'>+</th><th>変更後</th></tr></thead><tbody>");

        if (!r.HasChanges)
        {
            sb.Append("<tr class='ctx'><td class='ln'> </td><td colspan='3'>差分なし</td></tr>");
        }
        else
        {
            foreach (var line in rows)
            {
                var cls = line.Kind == '-' ? "del" : line.Kind == '+' ? "add" : "ctx";
                var oldText = line.Kind == '+' ? string.Empty : line.Text;
                var newText = line.Kind == '-' ? string.Empty : line.Text;
                sb.Append($"<tr class='{cls}'><td class='ln'>{(line.OldLineNumber == 0 ? "" : line.OldLineNumber)}</td><td>{WebUtility.HtmlEncode(oldText)}</td><td class='ln'>{(line.NewLineNumber == 0 ? "" : line.NewLineNumber)}</td><td>{WebUtility.HtmlEncode(newText)}</td></tr>");
            }
        }

        sb.Append("</tbody></table></div></body></html>");
        return sb.ToString();
    }
}
