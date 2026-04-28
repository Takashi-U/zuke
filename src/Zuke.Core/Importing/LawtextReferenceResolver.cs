using System.Text.RegularExpressions;
using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed class LawtextReferenceResolver
{
    private static readonly Regex FullRegex = new(@"^第?(?<a>[0-9０-９一二三四五六七八九十百千]+)条(?:第?(?<p>[0-9０-９一二三四五六七八九十百千]+)項(?:第?(?<i>[0-9０-９一二三四五六七八九十百千]+)号)?)?$", RegexOptions.Compiled);
    private static readonly Regex ParagraphRegex = new(@"^第?(?<p>[0-9０-９一二三四五六七八九十百千]+)項$", RegexOptions.Compiled);
    private static readonly Regex ItemRegex = new(@"^第?(?<i>[0-9０-９一二三四五六七八九十百千]+)号$", RegexOptions.Compiled);

    public string Resolve(string sentence, ArticleNode article, ParagraphNode paragraph, ItemNode? item, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags, SourceLocation? loc, LawtextImportOptions options, HashSet<string> usedRefs)
    {
        if (!options.ShouldConvertReferences) return sentence;

        var detector = new LawtextReferenceDetector();
        foreach (var token in detector.Detect(sentence).OrderByDescending(x => x.Index))
        {
            if (token.Text is "及び" or "又は")
            {
                AddDiag("LMD094", "複数条項参照はMVP未対応です。");
                continue;
            }

            if (token.Text == "から")
            {
                AddDiag("LMD095", "範囲参照はMVP未対応です。");
                continue;
            }

            if (token.Text is "次条" or "次項" or "次号" or "同条" or "同項" or "同号")
            {
                AddDiag("LMD092", "Lawtextの条項参照を解決できません。");
                continue;
            }

            string? replacement = null;
            if (token.Text == "前条")
            {
                replacement = article.Number > 1 ? $"{{{{参照:article-{article.Number - 1}|相対}}}}" : null;
                if (replacement is null) AddDiag("LMD091", "Lawtextの相対参照を解決できません。");
            }
            else if (token.Text == "前項")
            {
                replacement = paragraph.Number > 1 ? $"{{{{参照:article-{article.Number}-p{paragraph.Number - 1}|相対}}}}" : null;
                if (replacement is null) AddDiag("LMD091", "Lawtextの相対参照を解決できません。");
            }
            else if (token.Text == "前号")
            {
                replacement = item is not null && item.Number > 1 ? $"{{{{参照:article-{article.Number}-p{paragraph.Number}-i{item.Number - 1}|相対}}}}" : null;
                if (replacement is null) AddDiag("LMD091", "Lawtextの相対参照を解決できません。");
            }
            else
            {
                var m = FullRegex.Match(token.Text);
                if (m.Success)
                {
                    var a = LawtextParser.ParseNumber(m.Groups["a"].Value);
                    var p = m.Groups["p"].Success ? LawtextParser.ParseNumber(m.Groups["p"].Value) : 0;
                    var i = m.Groups["i"].Success ? LawtextParser.ParseNumber(m.Groups["i"].Value) : 0;
                    replacement = p == 0 ? $"{{{{参照:article-{a}}}}}" : i == 0 ? $"{{{{参照:article-{a}-p{p}|完全}}}}" : $"{{{{参照:article-{a}-p{p}-i{i}|完全}}}}";
                }
                else if ((m = ParagraphRegex.Match(token.Text)).Success)
                {
                    var p = LawtextParser.ParseNumber(m.Groups["p"].Value);
                    replacement = $"{{{{参照:article-{article.Number}-p{p}}}}}";
                }
                else if ((m = ItemRegex.Match(token.Text)).Success)
                {
                    var i = LawtextParser.ParseNumber(m.Groups["i"].Value);
                    replacement = $"{{{{参照:article-{article.Number}-p{paragraph.Number}-i{i}}}}}";
                }
            }

            if (replacement is null) continue;
            var target = replacement.Replace("{{参照:", string.Empty, StringComparison.Ordinal).Replace("}}", string.Empty, StringComparison.Ordinal).Split('|')[0];
            if (!table.ContainsKey(target))
            {
                AddDiag("LMD092", "Lawtextの条項参照を解決できません。");
                continue;
            }

            usedRefs.Add(target);
            sentence = sentence.Remove(token.Index, token.Length).Insert(token.Index, replacement);
        }

        return sentence;

        void AddDiag(string code, string message)
            => diags.Add(new(options.Strict ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, code, message, loc, []));
    }
}
