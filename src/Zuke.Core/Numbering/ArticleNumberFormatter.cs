using System.Text.RegularExpressions;

namespace Zuke.Core.Numbering;

public static class ArticleNumberFormatter
{
    private static readonly Regex ArticleNumberRegex = new(@"^第(?<base>[0-9０-９一二三四五六七八九十百千]+)条(?<branch>(の[0-9０-９一二三四五六七八九十百千]+)*)$", RegexOptions.Compiled);

    public static bool TryParseArticleNumber(string text, out ArticleNumber number)
    {
        number = ArticleNumber.FromBase(0);
        var m = ArticleNumberRegex.Match(text.Trim());
        if (!m.Success) return false;
        var baseNumber = LawtextImportingNumber(m.Groups["base"].Value);
        var rawBranch = m.Groups["branch"].Value;
        var branches = new List<int>();
        if (!string.IsNullOrEmpty(rawBranch))
        {
            foreach (var token in rawBranch.Split('の', StringSplitOptions.RemoveEmptyEntries))
            {
                var b = LawtextImportingNumber(token);
                if (b <= 0) return false;
                branches.Add(b);
            }
        }

        if (baseNumber <= 0) return false;
        number = new ArticleNumber(baseNumber, branches);
        return true;
    }

    public static ArticleNumber ParseArticleNumber(string text)
        => TryParseArticleNumber(text, out var n) ? n : throw new FormatException($"Invalid article number format: {text}");

    public static string ToArticleTitle(ArticleNumber number, bool arabic)
    {
        var baseText = arabic ? number.BaseNumber.ToString() : JapaneseNumberFormatter.ToKanjiNumber(number.BaseNumber);
        var branchText = string.Concat(number.BranchNumbers.Select(b => $"の{(arabic ? b.ToString() : JapaneseNumberFormatter.ToKanjiNumber(b))}"));
        return $"第{baseText}条{branchText}";
    }

    public static string ToXmlNum(ArticleNumber number) => string.Join("_", new[] { number.BaseNumber }.Concat(number.BranchNumbers));
    public static string ToReferenceName(ArticleNumber number) => "article-" + string.Join("-", new[] { number.BaseNumber }.Concat(number.BranchNumbers));

    private static int LawtextImportingNumber(string text)
    {
        return Importing.LawtextParser.ParseNumber(text);
    }
}
