using System.Text.RegularExpressions;
using Zuke.Core.Model;
using Zuke.Core.Numbering;
using Zuke.Core.References;

namespace Zuke.Core.Importing;

public sealed class LawtextReferenceResolver
{
    private static readonly Regex ParagraphRegex = new(@"^第?(?<p>[0-9０-９一二三四五六七八九十百千]+)項$", RegexOptions.Compiled);
    private static readonly Regex ItemRegex = new(@"^第?(?<i>[0-9０-９一二三四五六七八九十百千]+)号$", RegexOptions.Compiled);

    public string Resolve(string sentence, ArticleNode article, ParagraphNode paragraph, ItemNode? item, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags, SourceLocation? loc, LawtextImportOptions options, HashSet<string> usedRefs)
    {
        if (!options.ShouldConvertReferences) return sentence;
        var detector = new LawtextReferenceDetector();
        foreach (var token in detector.Detect(sentence).OrderByDescending(x => x.Index))
        {
            if (IsProtectedReferenceToken(sentence, token)) continue;
            if (token.Text is "及び" or "又は") { AddDiag("LMD094", "複数条項参照はMVP未対応です。"); continue; }
            if (token.Text == "から") { AddDiag("LMD095", "範囲参照はMVP未対応です。"); continue; }
            if (token.Text is "前条" or "前項" or "前号" or "次条" or "次項" or "次号" or "同条" or "同項" or "同号")
            {
                AddDiag("LMD091", "Lawtextの相対参照を解決できません。");
                continue;
            }

            string? replacement = null;
            if (TryResolveAbsolute(token.Text, out var absolute)) replacement = absolute;

            if (replacement is null) continue;
            var target = replacement.Replace("{{参照:", string.Empty, StringComparison.Ordinal).Replace("}}", string.Empty, StringComparison.Ordinal).Split('|')[0];
            if (!table.ContainsKey(target)) { AddDiag("LMD092", "Lawtextの条項参照を解決できません。"); continue; }
            usedRefs.Add(target);
            sentence = sentence.Remove(token.Index, token.Length).Insert(token.Index, replacement);
        }
        return sentence;

        void AddDiag(string code, string message)
            => diags.Add(new(options.Strict ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, code, message, loc, []));
    }

    private static bool IsProtectedReferenceToken(string sentence, LawtextReferenceToken token)
    {
        var protectedRegex = new Regex(@"(?:本条|同条|前条|次条)第[0-9０-９一二三四五六七八九十百千]+項(?:第[0-9０-９一二三四五六七八九十百千]+号)?(?:及び第[0-9０-９一二三四五六七八九十百千]+号)?(?:又は第[0-9０-９一二三四五六七八九十百千]+項)?(?:から第[0-9０-９一二三四五六七八九十百千]+項)?");
        return protectedRegex.Matches(sentence)
            .Any(m => token.Index >= m.Index && (token.Index + token.Length) <= (m.Index + m.Length));
    }

    private static bool TryResolveAbsolute(string text, out string? replacement)
    {
        replacement = null;
        var itemMatch = Regex.Match(text, @"第?[0-9０-９一二三四五六七八九十百千]+号$");
        var paraMatch = Regex.Match(text, @"第?[0-9０-９一二三四五六七八九十百千]+項");
        var articlePart = text;
        var p = 0;
        var i = 0;
        if (paraMatch.Success)
        {
            articlePart = text[..paraMatch.Index];
            p = LawtextParser.ParseNumber(ParagraphRegex.Match(paraMatch.Value).Groups["p"].Value);
            if (itemMatch.Success) i = LawtextParser.ParseNumber(ItemRegex.Match(itemMatch.Value).Groups["i"].Value);
        }
        if (!ArticleNumberFormatter.TryParseArticleNumber(articlePart, out var an)) return false;

        var aRef = ArticleNumberFormatter.ToReferenceName(an);
        replacement = p == 0 ? $"{{{{参照:{aRef}|完全}}}}" : i == 0 ? $"{{{{参照:{aRef}-p{p}|完全}}}}" : $"{{{{参照:{aRef}-p{p}-i{i}|完全}}}}";
        return true;
    }

}
