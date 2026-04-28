using System.Text.RegularExpressions;
using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed class LawtextAuditService
{
    public LawtextAuditResult Audit(string lawtext, string? path, bool strict)
    {
        var parser = new LawtextParser();
        var (model, diags) = parser.Parse(lawtext, path);
        var all = new List<DiagnosticMessage>(diags);

        if (string.IsNullOrWhiteSpace(model.Metadata.LawTitle)) Add("LMD090", "LawTitle がありません。", 1);
        if (string.IsNullOrWhiteSpace(model.Metadata.LawNum)) Add("LMD090", "LawNum がありません。", 1);

        foreach (var article in model.DirectArticles.Concat(model.Chapters.SelectMany(c => c.Articles)).Concat(model.Chapters.SelectMany(c => c.Sections).SelectMany(s => s.Articles)))
        {
            if (!article.Paragraphs.Any() || article.Paragraphs.All(p => string.IsNullOrWhiteSpace(p.SentenceText) && !p.Items.Any()))
                Add("LMD099", "Article内に本文がありません。", article.Location?.Line ?? 1);
        }

        var lines = lawtext.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (Regex.IsMatch(line, @"^\d+\s*/\s*\d+$") || Regex.IsMatch(line, @"^-\s*\d+\s*-$") || Regex.IsMatch(line, @"^ページ\s*\d+$") || line.Contains("Page", StringComparison.OrdinalIgnoreCase))
                Add("LMD099", "Word由来のヘッダー/フッター/ページ番号らしき行です。", i + 1);
            if (line.Contains("第一条", StringComparison.Ordinal) && !line.StartsWith("第一条", StringComparison.Ordinal))
                Add("LMD099", "本文途中に条番号らしき表現があります。", i + 1);
            if (Regex.IsMatch(line, @"^第.+条の.+"))
                Add("LMD092", "枝番付き条番号参照/条文はMVP未対応です。", i + 1);
            if (Regex.IsMatch(line, @"^（.+）$") && (i + 1 >= lines.Length || !Regex.IsMatch(lines[i + 1].Trim(), @"^第.+条")))
                Add("LMD099", "ArticleCaptionらしき行の直後に条文がありません。", i + 1);
        }

        return new LawtextAuditResult(all);

        void Add(string code, string msg, int line) => all.Add(new(strict ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, code, msg, new SourceLocation(path, line, 1), []));
    }
}
