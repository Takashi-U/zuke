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
        sb.Append("body{font-family:-apple-system,BlinkMacSystemFont,Segoe UI,sans-serif;padding:16px;color:#24292f;}table{width:100%;border-collapse:collapse;table-layout:fixed;font-family:ui-monospace,SFMono-Regular,Menlo,monospace;font-size:12px;}th,td{border:1px solid #d0d7de;padding:4px 8px;vertical-align:top;white-space:pre-wrap;word-break:break-word;}thead th{background:#f6f8fa;} .ln{width:56px;color:#57606a;text-align:right;background:#f6f8fa;} .del{background:#ffebe9;} .add{background:#dafbe1;} .ctx{background:#ffffff;} .del-inline{background:#ff818266;font-weight:600;} .add-inline{background:#2da44e66;font-weight:600;} .wrap{display:grid;grid-template-columns:1fr;gap:12px;} .summary{margin-bottom:12px;} h2{margin:0 0 8px;} ");
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
            for (var i = 0; i < rows.Count; i++)
            {
                var line = rows[i];
                if (line.Kind == ' ')
                {
                    AppendRow(sb, "ctx", line.OldLineNumber, WebUtility.HtmlEncode(line.Text), "ctx", line.NewLineNumber, WebUtility.HtmlEncode(line.Text));
                    continue;
                }

                if (line.Kind is '+' or '-')
                {
                    var deletes = new List<DiffLine>();
                    var adds = new List<DiffLine>();

                    while (i < rows.Count && rows[i].Kind is '+' or '-')
                    {
                        if (rows[i].Kind == '-')
                        {
                            deletes.Add(rows[i]);
                        }
                        else
                        {
                            adds.Add(rows[i]);
                        }

                        i++;
                    }

                    i--;

                    var pairCount = Math.Max(deletes.Count, adds.Count);
                    for (var pairIndex = 0; pairIndex < pairCount; pairIndex++)
                    {
                        var oldLine = pairIndex < deletes.Count ? deletes[pairIndex] : null;
                        var newLine = pairIndex < adds.Count ? adds[pairIndex] : null;

                        if (oldLine is not null && newLine is not null)
                        {
                            var (oldText, newText) = HighlightInlineDiff(oldLine.Text, newLine.Text);
                            AppendRow(sb, "del", oldLine.OldLineNumber, oldText, "add", newLine.NewLineNumber, newText);
                            continue;
                        }

                        if (oldLine is not null)
                        {
                            AppendRow(sb, "del", oldLine.OldLineNumber, WebUtility.HtmlEncode(oldLine.Text), "ctx", 0, string.Empty);
                            continue;
                        }

                        if (newLine is not null)
                        {
                            AppendRow(sb, "ctx", 0, string.Empty, "add", newLine.NewLineNumber, WebUtility.HtmlEncode(newLine.Text));
                        }
                    }
                }
            }
        }

        sb.Append("</tbody></table></div></body></html>");
        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, string oldClass, int oldLineNumber, string oldText, string newClass, int newLineNumber, string newText)
    {
        sb.Append("<tr>");
        sb.Append($"<td class='ln'>{(oldLineNumber == 0 ? "" : oldLineNumber)}</td>");
        sb.Append($"<td class='{oldClass}'>{oldText}</td>");
        sb.Append($"<td class='ln'>{(newLineNumber == 0 ? "" : newLineNumber)}</td>");
        sb.Append($"<td class='{newClass}'>{newText}</td>");
        sb.Append("</tr>");
    }

    private static (string oldHtml, string newHtml) HighlightInlineDiff(string oldText, string newText)
    {
        var oldKeep = BuildLcsKeepMap(oldText, newText, forOld: true);
        var newKeep = BuildLcsKeepMap(oldText, newText, forOld: false);
        return (
            BuildHighlightedText(oldText, oldKeep, "del-inline"),
            BuildHighlightedText(newText, newKeep, "add-inline"));
    }

    private static bool[] BuildLcsKeepMap(string oldText, string newText, bool forOld)
    {
        var n = oldText.Length;
        var m = newText.Length;
        var dp = new int[n + 1, m + 1];

        for (var i = n - 1; i >= 0; i--)
        {
            for (var j = m - 1; j >= 0; j--)
            {
                if (oldText[i] == newText[j])
                {
                    dp[i, j] = dp[i + 1, j + 1] + 1;
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i + 1, j], dp[i, j + 1]);
                }
            }
        }

        var oldKeep = new bool[n];
        var newKeep = new bool[m];
        var oi = 0;
        var nj = 0;
        while (oi < n && nj < m)
        {
            if (oldText[oi] == newText[nj])
            {
                oldKeep[oi] = true;
                newKeep[nj] = true;
                oi++;
                nj++;
                continue;
            }

            if (dp[oi + 1, nj] >= dp[oi, nj + 1])
            {
                oi++;
            }
            else
            {
                nj++;
            }
        }

        return forOld ? oldKeep : newKeep;
    }

    private static string BuildHighlightedText(string text, bool[] keep, string cssClass)
    {
        var sb = new StringBuilder();
        var inSpan = false;
        for (var i = 0; i < text.Length; i++)
        {
            if (i < keep.Length && !keep[i] && !inSpan)
            {
                sb.Append($"<span class='{cssClass}'>");
                inSpan = true;
            }
            else if (i < keep.Length && keep[i] && inSpan)
            {
                sb.Append("</span>");
                inSpan = false;
            }

            sb.Append(WebUtility.HtmlEncode(text[i].ToString()));
        }

        if (inSpan)
        {
            sb.Append("</span>");
        }

        return sb.ToString();
    }
}
