using System.Net;
using System.Text;

namespace Zuke.Core.Diff;

public sealed class HtmlDiffRenderer
{
    public string Render(string oldName, string newName, DiffResult r)
    {
        var sb = new StringBuilder();
        var added = r.Hunks.SelectMany(h => h.Lines).Count(l => l.Kind == '+');
        var removed = r.Hunks.SelectMany(h => h.Lines).Count(l => l.Kind == '-');

        sb.Append("<html><head><meta charset='utf-8'><style>");
        sb.Append("body{font-family:-apple-system,BlinkMacSystemFont,Segoe UI,sans-serif;padding:16px;}table{width:100%;border-collapse:collapse;font-family:ui-monospace,monospace;font-size:13px;}th,td{border:1px solid #d0d7de;padding:4px 8px;vertical-align:top;}thead th{background:#f6f8fa;} .add{background:#dafbe1;} .del{background:#ffebe9;} .ctx{background:#fff;} .lineno{color:#57606a;width:56px;text-align:right;} .wrap{display:grid;grid-template-columns:1fr 1fr;gap:12px;}</style></head><body>");
        sb.Append($"<h2>Diff: {WebUtility.HtmlEncode(oldName)} → {WebUtility.HtmlEncode(newName)}</h2>");
        sb.Append($"<p>追加: {added} / 削除: {removed}</p>");
        sb.Append("<div class='wrap'><table><thead><tr><th colspan='2'>変更前</th><th>内容</th></tr></thead><tbody>");
        foreach (var l in r.Hunks.SelectMany(h => h.Lines))
        {
            var cls = l.Kind == '-' ? "del" : l.Kind == '+' ? "add" : "ctx";
            sb.Append($"<tr class='{cls}'><td class='lineno'>{(l.OldLineNumber == 0 ? "" : l.OldLineNumber)}</td><td>{WebUtility.HtmlEncode(l.Kind.ToString())}</td><td>{WebUtility.HtmlEncode(l.Text)}</td></tr>");
        }

        sb.Append("</tbody></table><table><thead><tr><th colspan='2'>変更後</th><th>内容</th></tr></thead><tbody>");
        foreach (var l in r.Hunks.SelectMany(h => h.Lines))
        {
            var cls = l.Kind == '+' ? "add" : l.Kind == '-' ? "del" : "ctx";
            sb.Append($"<tr class='{cls}'><td class='lineno'>{(l.NewLineNumber == 0 ? "" : l.NewLineNumber)}</td><td>{WebUtility.HtmlEncode(l.Kind.ToString())}</td><td>{WebUtility.HtmlEncode(l.Text)}</td></tr>");
        }

        sb.Append("</tbody></table></div></body></html>");
        return sb.ToString();
    }
}
