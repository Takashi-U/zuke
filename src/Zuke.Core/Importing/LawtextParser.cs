using System.Text.RegularExpressions;
using Zuke.Core.Model;
using Zuke.Core.Numbering;

namespace Zuke.Core.Importing;

public sealed class LawtextParser
{
    private static readonly Regex LawNumRegex = new(@"^（(?<num>.+)）$", RegexOptions.Compiled);
    private static readonly Regex ChapterRegex = new(@"^\s*第(?<n>[0-9０-９一二三四五六七八九十百千]+)章\s+[　 ]*(?<t>.+)$", RegexOptions.Compiled);
    private static readonly Regex SectionRegex = new(@"^\s*第(?<n>[0-9０-９一二三四五六七八九十百千]+)節\s+[　 ]*(?<t>.+)$", RegexOptions.Compiled);
    private static readonly Regex CaptionRegex = new(@"^\s*（(?<t>.+)）\s*$", RegexOptions.Compiled);
    private static readonly Regex ArticleRegex = new(@"^(?<num>第[0-9０-９一二三四五六七八九十百千]+条(の[0-9０-９一二三四五六七八九十百千]+)*)\s*[　 ]*(?<s>.*)$", RegexOptions.Compiled);
    private static readonly Regex ParagraphRegex = new(@"^(?<n>[0-9０-９]+)\s*[　 ]*(?<s>.*)$", RegexOptions.Compiled);
    private static readonly Regex ItemRegex = new(@"^\s*(?<n>[一二三四五六七八九十]+)\s*[　 ](?<s>.+)$", RegexOptions.Compiled);
    private static readonly Regex Subitem1Regex = new(@"^\s*(?<n>[イロハニホヘトチリヌルヲワカヨタレソツネナラムウヰノオクヤマケフコエテ])\s*[　 ](?<s>.+)$", RegexOptions.Compiled);

    public (LawDocumentModel Model, IReadOnlyList<DiagnosticMessage> Diagnostics) Parse(string lawtext, string? filePath)
    {
        var diags = new List<DiagnosticMessage>();
        var lines = lawtext.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var title = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim() ?? "";
        var index = Array.FindIndex(lines, l => !string.IsNullOrWhiteSpace(l));
        var lawNum = "";
        if (index >= 0 && index + 1 < lines.Length)
        {
            var mnum = LawNumRegex.Match(lines[index + 1].Trim());
            if (mnum.Success)
            {
                lawNum = mnum.Groups["num"].Value.Trim();
                index += 2;
            }
            else
            {
                index += 1;
            }
        }

        var metadata = InferMetadata(title, lawNum, filePath, diags);

        var chapters = new List<ChapterNode>();
        var direct = new List<ArticleNode>();
        ChapterNode? currentChapter = null;
        SectionNode? currentSection = null;
        var chArticles = new List<ArticleNode>();
        var sections = new List<SectionNode>();
        var secArticles = new List<ArticleNode>();
        ArticleNode? currentArticle = null;
        var paragraphs = new List<ParagraphNode>();
        ParagraphNode? currentParagraph = null;
        var items = new List<ItemNode>();

        string? pendingCaption = null;

        for (var i = Math.Max(index, 0); i < lines.Length; i++)
        {
            var raw = lines[i];
            var trim = raw.Trim();
            if (string.IsNullOrWhiteSpace(trim)) continue;

            var cap = CaptionRegex.Match(trim);
            if (cap.Success)
            {
                pendingCaption = cap.Groups["t"].Value.Trim();
                continue;
            }

            var chapter = ChapterRegex.Match(raw);
            if (chapter.Success)
            {
                FlushArticle();
                FlushSection();
                FlushChapter();
                currentChapter = new(ParseNumber(chapter.Groups["n"].Value), chapter.Groups["t"].Value.Trim(), new(filePath, i + 1, 1), [], []);
                continue;
            }

            var section = SectionRegex.Match(raw);
            if (section.Success)
            {
                FlushArticle();
                FlushSection();
                currentSection = new(ParseNumber(section.Groups["n"].Value), section.Groups["t"].Value.Trim(), new(filePath, i + 1, 1), []);
                continue;
            }

            var article = ArticleRegex.Match(trim);
            if (article.Success)
            {
                FlushArticle();
                var articleText = article.Groups["num"].Value;
                if (!ArticleNumberFormatter.TryParseArticleNumber(articleText, out var articleNumber))
                {
                    diags.Add(new(DiagnosticSeverity.Warning, "LMD101", "Article枝番号の形式が不正です。", new(filePath, i + 1, 1), []));
                    continue;
                }
                var n = articleNumber.BaseNumber;
                currentArticle = new(n, null, pendingCaption ?? "", ArticleNumberFormatter.ToArticleTitle(articleNumber, false), new(filePath, i + 1, 1), []) { ArticleNumber = articleNumber };
                pendingCaption = null;
                currentParagraph = new(1, null, null, article.Groups["s"].Value.Trim(), new(filePath, i + 1, 1), []);
                items = [];
                continue;
            }

            var para = ParagraphRegex.Match(trim);
            if (para.Success && currentArticle is not null)
            {
                FlushParagraph();
                currentParagraph = new(ParseNumber(para.Groups["n"].Value), null, para.Groups["n"].Value, para.Groups["s"].Value.Trim(), new(filePath, i + 1, 1), []);
                continue;
            }

            var item = ItemRegex.Match(trim);
            if (item.Success && currentParagraph is not null)
            {
                items.Add(new(ParseNumber(item.Groups["n"].Value), null, item.Groups["n"].Value, item.Groups["s"].Value.Trim(), new(filePath, i + 1, 1), []));
                continue;
            }

            var subitem = Subitem1Regex.Match(trim);
            if (subitem.Success && items.Count > 0)
            {
                var parent = items[^1];
                var children = parent.Children.ToList();
                children.Add(new(children.Count + 1, null, subitem.Groups["n"].Value, subitem.Groups["s"].Value.Trim(), new(filePath, i + 1, 1), []));
                items[^1] = parent with { Children = children };
                continue;
            }

            if (currentParagraph is not null)
            {
                currentParagraph = currentParagraph with { SentenceText = string.Concat(currentParagraph.SentenceText, trim) };
                continue;
            }

            diags.Add(new(DiagnosticSeverity.Warning, "LMD096", "Lawtext構造を安全にMarkdownへ変換できません。", new(filePath, i + 1, 1), []));
        }

        FlushArticle();
        FlushSection();
        FlushChapter();

        var model = new LawDocumentModel(metadata, chapters, direct, diags);
        return (model, diags);

        void FlushParagraph()
        {
            if (currentParagraph is null) return;
            currentParagraph = currentParagraph with { Items = [.. items] };
            paragraphs.Add(currentParagraph);
            currentParagraph = null;
            items = [];
        }

        void FlushArticle()
        {
            if (currentArticle is null) return;
            FlushParagraph();
            if (!IsSequential(paragraphs.Select(p => p.Number)))
            {
                diags.Add(new(DiagnosticSeverity.Warning, "LMD097", "Lawtextの条番号または項番号が連番ではありません。", currentArticle.Location, []));
            }

            currentArticle = currentArticle with { Paragraphs = [.. paragraphs] };
            paragraphs = [];
            if (currentSection is not null) secArticles.Add(currentArticle);
            else if (currentChapter is not null) chArticles.Add(currentArticle);
            else direct.Add(currentArticle);
            currentArticle = null;
        }

        void FlushSection()
        {
            if (currentSection is null) return;
            currentSection = currentSection with { Articles = [.. secArticles] };
            secArticles = [];
            sections.Add(currentSection);
            currentSection = null;
        }

        void FlushChapter()
        {
            if (currentChapter is null) return;
            currentChapter = currentChapter with { Sections = [.. sections], Articles = [.. chArticles] };
            sections = [];
            chArticles = [];
            chapters.Add(currentChapter);
            currentChapter = null;
        }
    }

    private static LawMetadata InferMetadata(string lawTitle, string lawNum, string? filePath, List<DiagnosticMessage> diags)
    {
        var era = "Reiwa";
        var year = 1;
        var num = 1;
        var lawType = "Misc";
        var m = Regex.Match(lawNum, @"^(?<era>令和|平成|昭和)(?<y>[一二三四五六七八九十百千0-9０-９]+)年(?<type>法律|規則)第(?<n>[一二三四五六七八九十百千0-9０-９]+)号$");
        if (m.Success)
        {
            era = m.Groups["era"].Value switch { "平成" => "Heisei", "昭和" => "Showa", _ => "Reiwa" };
            year = ParseNumber(m.Groups["y"].Value);
            num = ParseNumber(m.Groups["n"].Value);
            lawType = m.Groups["type"].Value == "法律" ? "Act" : "Misc";
        }
        else if (string.IsNullOrWhiteSpace(lawNum))
        {
            diags.Add(new(DiagnosticSeverity.Warning, "LMD090", "LawNum がありません。社内規程として扱います。", new(filePath, 1, 1), []));
        }
        else
        {
            diags.Add(new(DiagnosticSeverity.Warning, "LMD090", "Lawtextの法令番号からメタデータを完全には推定できません。", new(filePath, 1, 1), []));
        }

        return new LawMetadata(lawTitle, lawNum, era, year, num, lawType, "ja");
    }

    private static bool IsSequential(IEnumerable<int> nums)
    {
        var list = nums.ToList();
        for (var i = 0; i < list.Count; i++) if (list[i] != i + 1) return false;
        return true;
    }

    public static int ParseNumber(string text)
    {
        var normalized = text.Trim()
            .Replace("０", "0", StringComparison.Ordinal).Replace("１", "1", StringComparison.Ordinal)
            .Replace("２", "2", StringComparison.Ordinal).Replace("３", "3", StringComparison.Ordinal)
            .Replace("４", "4", StringComparison.Ordinal).Replace("５", "5", StringComparison.Ordinal)
            .Replace("６", "6", StringComparison.Ordinal).Replace("７", "7", StringComparison.Ordinal)
            .Replace("８", "8", StringComparison.Ordinal).Replace("９", "9", StringComparison.Ordinal);

        if (int.TryParse(normalized, out var arabic)) return arabic;

        var map = new Dictionary<char, int>{{'一',1},{'二',2},{'三',3},{'四',4},{'五',5},{'六',6},{'七',7},{'八',8},{'九',9}};
        var total = 0;
        var current = 0;
        foreach (var c in normalized)
        {
            if (map.TryGetValue(c, out var v)) { current += v; continue; }
            if (c == '十') { total += (current == 0 ? 1 : current) * 10; current = 0; continue; }
            if (c == '百') { total += (current == 0 ? 1 : current) * 100; current = 0; continue; }
            if (c == '千') { total += (current == 0 ? 1 : current) * 1000; current = 0; continue; }
        }

        return total + current;
    }
}
