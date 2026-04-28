using System.Text.RegularExpressions;
using Zuke.Core.Markdown;
using Zuke.Core.Model;
using Zuke.Core.Numbering;

namespace Zuke.Core.Parsing;

public sealed class MarkdownLawParser
{
    private static readonly Regex LabelRegex = new(@"\[(条|項|号|a|p|i):(?<name>[^\]]+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NumberedChapterRegex = new(@"^第[0-9０-９一二三四五六七八九十百千]+章\s*(?<title>.+)$", RegexOptions.Compiled);
    private static readonly Regex NumberedSectionRegex = new(@"^第[0-9０-９一二三四五六七八九十百千]+節\s*(?<title>.+)$", RegexOptions.Compiled);
    private static readonly Regex NumberedArticleRegex = new(@"^第[0-9０-９一二三四五六七八九十百千]+条\s*(?<title>.+)$", RegexOptions.Compiled);

    public LawDocumentModel Parse(string markdown, string? filePath)
    {
        var parsed = FrontMatterParser.ParseDetailed(markdown);
        var meta = parsed.Metadata;
        var body = parsed.Body;

        var chapters = new List<ChapterNode>();
        var direct = new List<ArticleNode>();
        var diagnostics = new List<DiagnosticMessage>();

        var lines = body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        ChapterNode? currentChapter = null;
        SectionNode? currentSection = null;
        var chArticles = new List<ArticleNode>();
        var sections = new List<SectionNode>();
        var secArticles = new List<ArticleNode>();
        var articleNo = 0;

        var i = 0;
        while (i < lines.Length)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) { i++; continue; }

            if (TryParseChapterHeading(line, out var chapterTitle))
            {
                FlushSection();
                FlushChapter();
                currentChapter = new ChapterNode(0, chapterTitle, new(filePath, i + 1, 1), [], []);
                i++;
                continue;
            }

            if (TryParseSectionHeading(line, out var sectionTitle))
            {
                FlushSection();
                currentSection = new SectionNode(0, sectionTitle, new(filePath, i + 1, 1), []);
                i++;
                continue;
            }

            if (currentSection is not null && line.StartsWith("## ", StringComparison.Ordinal) && !line[3..].Trim().StartsWith("節 ", StringComparison.Ordinal) && !NumberedSectionRegex.IsMatch(line[3..].Trim()))
            {
                FlushSection();
            }

            if (TryParseArticleHeading(line, currentSection is not null, out var headingText))
            {
                articleNo++;
                var (caption, articleRefName) = ParseHeading(headingText, "条");
                var articleStart = i + 1;
                i++;

                var blockLines = new List<(string line, int sourceLine)>();
                while (i < lines.Length)
                {
                    var t = lines[i].Trim();
                    if (t.StartsWith("#", StringComparison.Ordinal)) break;
                    blockLines.Add((lines[i], i + 1));
                    i++;
                }

                var paragraphs = ParseParagraphs(blockLines, filePath, diagnostics);
                if (paragraphs.Count == 0)
                {
                    paragraphs.Add(new ParagraphNode(1, null, null, string.Empty, new(filePath, articleStart, 1), []));
                }

                var article = new ArticleNode(articleNo, articleRefName, caption, JapaneseNumberFormatter.ToArticle(articleNo, false), new(filePath, articleStart, 1), paragraphs);
                if (currentSection is not null) secArticles.Add(article);
                else if (currentChapter is not null) chArticles.Add(article);
                else direct.Add(article);

                continue;
            }

            if (line.StartsWith("|", StringComparison.Ordinal) || line.StartsWith("<", StringComparison.Ordinal))
            {
                diagnostics.Add(new(DiagnosticSeverity.Error, "LMD046", "未対応のMarkdown要素です。", new(filePath, i + 1, 1), []));
            }

            i++;
        }

        FlushSection();
        FlushChapter();
        return new LawDocumentModel(meta, chapters, direct, diagnostics);

        void FlushSection()
        {
            if (currentSection is null) return;
            sections.Add(currentSection with { Articles = [.. secArticles] });
            secArticles.Clear();
            currentSection = null;
        }

        void FlushChapter()
        {
            if (currentChapter is null) return;
            chapters.Add(currentChapter with { Sections = [.. sections], Articles = [.. chArticles] });
            sections.Clear();
            chArticles.Clear();
            currentChapter = null;
        }
    }

    private static bool TryParseChapterHeading(string line, out string title)
    {
        title = string.Empty;
        if (!line.StartsWith("# ", StringComparison.Ordinal)) return false;
        var text = line[2..].Trim();
        if (text.StartsWith("章 ", StringComparison.Ordinal)) text = text[2..].Trim();
        var m = NumberedChapterRegex.Match(text);
        if (m.Success) text = m.Groups["title"].Value.Trim();
        title = text;
        return true;
    }

    private static bool TryParseSectionHeading(string line, out string title)
    {
        title = string.Empty;
        if (!line.StartsWith("## ", StringComparison.Ordinal)) return false;
        var text = line[3..].Trim();
        if (text.StartsWith("節 ", StringComparison.Ordinal)) { title = text[2..].Trim(); return true; }
        var m = NumberedSectionRegex.Match(text);
        if (m.Success) { title = m.Groups["title"].Value.Trim(); return true; }
        return false;
    }

    private static bool TryParseArticleHeading(string line, bool insideSection, out string headingText)
    {
        headingText = string.Empty;
        if (line.StartsWith("### ", StringComparison.Ordinal))
        {
            headingText = line[4..].Trim();
            var m = NumberedArticleRegex.Match(headingText);
            if (m.Success) headingText = m.Groups["title"].Value.Trim();
            return true;
        }

        if (line.StartsWith("## ", StringComparison.Ordinal))
        {
            var text = line[3..].Trim();
            headingText = text;
            var m = NumberedArticleRegex.Match(headingText);
            if (m.Success) headingText = m.Groups["title"].Value.Trim();
            return true;
        }

        return false;
    }

    private static (string caption, string? referenceName) ParseHeading(string heading, string expectedKind)
    {
        var match = LabelRegex.Match(heading);
        if (!match.Success) return (heading.Trim(), null);

        var kind = match.Groups[1].Value;
        if (!kind.Equals(expectedKind, StringComparison.Ordinal) && !kind.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            return (heading.Trim(), null);
        }

        var name = match.Groups["name"].Value.Trim();
        var caption = LabelRegex.Replace(heading, string.Empty).Trim();
        return (caption, name);
    }

    private static List<ParagraphNode> ParseParagraphs(List<(string line, int sourceLine)> blockLines, string? filePath, List<DiagnosticMessage> diagnostics)
    {
        var paragraphs = new List<ParagraphNode>();
        string? pendingParagraphRef = null;
        var currentSentence = new List<string>();
        var currentItems = new List<ItemNode>();
        var itemNo = 0;
        var paraStartLine = blockLines.Count > 0 ? blockLines[0].sourceLine : 1;

        foreach (var (rawLine, lineNo) in blockLines)
        {
            var line = rawLine.TrimEnd();
            var trim = line.Trim();
            if (string.IsNullOrWhiteSpace(trim))
            {
                FlushParagraph(lineNo);
                continue;
            }

            var paragraphLabelMatch = LabelRegex.Match(trim);
            if (paragraphLabelMatch.Success && (paragraphLabelMatch.Groups[1].Value == "項" || paragraphLabelMatch.Groups[1].Value.Equals("p", StringComparison.OrdinalIgnoreCase)))
            {
                FlushParagraph(lineNo);
                pendingParagraphRef = paragraphLabelMatch.Groups["name"].Value.Trim();
                paraStartLine = lineNo;
                continue;
            }

            if (TryParseItem(trim, out var itemTitle, out var itemText, out var isSubitem1, out var itemRefName))
            {
                if (isSubitem1)
                {
                    if (currentItems.Count == 0)
                    {
                        currentItems.Add(new ItemNode(++itemNo, null, JapaneseNumberFormatter.ToItemTitle(itemNo, false), string.Empty, new(filePath, lineNo, 1), []));
                    }

                    var parent = currentItems[^1];
                    var children = parent.Children.ToList();
                    children.Add(new ItemNode(children.Count + 1, itemRefName, itemTitle, itemText, new(filePath, lineNo, 1), []));
                    currentItems[^1] = parent with { Children = children };
                }
                else
                {
                    var nextNo = ++itemNo;
                    currentItems.Add(new ItemNode(nextNo, itemRefName, string.IsNullOrWhiteSpace(itemTitle) ? JapaneseNumberFormatter.ToItemTitle(nextNo, false) : itemTitle, itemText, new(filePath, lineNo, 1), []));
                }

                continue;
            }

            if (trim.StartsWith("|", StringComparison.Ordinal) || trim.StartsWith("<", StringComparison.Ordinal))
            {
                diagnostics.Add(new(DiagnosticSeverity.Error, "LMD046", "未対応のMarkdown要素があります。", new(filePath, lineNo, 1), []));
                continue;
            }

            currentSentence.Add(trim);
        }

        FlushParagraph(blockLines.Count > 0 ? blockLines[^1].sourceLine : 1);
        return paragraphs;

        void FlushParagraph(int lineNo)
        {
            if (currentSentence.Count == 0 && currentItems.Count == 0 && pendingParagraphRef is null)
            {
                paraStartLine = lineNo;
                return;
            }

            var sentence = string.Join("", currentSentence);
            paragraphs.Add(new ParagraphNode(paragraphs.Count + 1, pendingParagraphRef, null, sentence, new(filePath, paraStartLine, 1), [.. currentItems]));
            currentSentence.Clear();
            currentItems.Clear();
            pendingParagraphRef = null;
            itemNo = 0;
            paraStartLine = lineNo;
        }
    }

    private static bool TryParseItem(string text, out string title, out string sentence, out bool isSubitem1, out string? referenceName)
    {
        title = string.Empty;
        sentence = string.Empty;
        isSubitem1 = false;
        referenceName = null;

        var labeledBullet = Regex.Match(text, @"^(?:[-*]|・)\s*\[(号|i):(?<name>[^\]]+)\]\s*(?<text>.+)$", RegexOptions.IgnoreCase);
        if (labeledBullet.Success)
        {
            referenceName = labeledBullet.Groups["name"].Value.Trim();
            sentence = labeledBullet.Groups["text"].Value.Trim();
            return true;
        }

        var bullet = Regex.Match(text, @"^(?:[-*]|・)\s*(?<text>.+)$");
        if (bullet.Success)
        {
            sentence = bullet.Groups["text"].Value.Trim();
            return true;
        }

        var numbered = Regex.Match(text, @"^\d+\.\s*(?<text>.+)$");
        if (numbered.Success)
        {
            sentence = numbered.Groups["text"].Value.Trim();
            return true;
        }

        var labeled = Regex.Match(text, @"^\[(号|i):(?<name>[^\]]+)\]\s*(?<text>.+)$", RegexOptions.IgnoreCase);
        if (labeled.Success)
        {
            referenceName = labeled.Groups["name"].Value.Trim();
            sentence = labeled.Groups["text"].Value.Trim();
            return true;
        }

        var subitem = Regex.Match(text, @"^(?<title>[イロハニホヘトチリヌルヲワカヨタレソツネナラムウヰノオクヤマケフコエテアサキユメミシヱヒモセス])\s*[　 ](?<text>.+)$");
        if (subitem.Success)
        {
            title = subitem.Groups["title"].Value;
            sentence = subitem.Groups["text"].Value.Trim();
            isSubitem1 = true;
            return true;
        }

        var item = Regex.Match(text, @"^(?<title>[一二三四五六七八九十]+)\s*[　 ](?<text>.+)$");
        if (item.Success)
        {
            sentence = item.Groups["text"].Value.Trim();
            return true;
        }

        return false;
    }
}
