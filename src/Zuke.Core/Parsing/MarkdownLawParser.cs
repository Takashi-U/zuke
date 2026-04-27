using System.Text.RegularExpressions;
using Zuke.Core.Markdown;
using Zuke.Core.Model;

namespace Zuke.Core.Parsing;

public sealed class MarkdownLawParser
{
    private static readonly Regex LabelRegex = new(@"\[(条|項|号):(?<name>[^\]]+)\]", RegexOptions.Compiled);

    public LawDocumentModel Parse(string markdown, string? filePath)
    {
        var (meta, body) = FrontMatterParser.Parse(markdown);
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
            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                FlushSection();
                FlushChapter();
                currentChapter = new ChapterNode(0, line[2..].Replace("章 ", "", StringComparison.Ordinal), new(filePath, i + 1, 1), [], []);
                i++;
                continue;
            }

            if (line.StartsWith("## 節 ", StringComparison.Ordinal))
            {
                FlushSection();
                currentSection = new SectionNode(0, line[5..], new(filePath, i + 1, 1), []);
                i++;
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal) || line.StartsWith("### ", StringComparison.Ordinal))
            {
                articleNo++;
                var headingText = line[3..].Trim();
                if (line.StartsWith("### ", StringComparison.Ordinal))
                {
                    headingText = line[4..].Trim();
                }

                var (caption, articleRefName) = ParseHeading(headingText);
                var articleStart = i + 1;
                i++;

                var blockLines = new List<(string line, int sourceLine)>();
                while (i < lines.Length)
                {
                    var t = lines[i].Trim();
                    if (t.StartsWith("#", StringComparison.Ordinal))
                    {
                        break;
                    }

                    blockLines.Add((lines[i], i + 1));
                    i++;
                }

                var paragraphs = ParseParagraphs(blockLines, filePath);
                if (paragraphs.Count == 0)
                {
                    paragraphs.Add(new ParagraphNode(1, null, null, string.Empty, new(filePath, articleStart, 1), []));
                }

                var article = new ArticleNode(articleNo, articleRefName, caption, $"第{articleNo}条", new(filePath, articleStart, 1), paragraphs);
                if (currentSection is not null)
                {
                    secArticles.Add(article);
                }
                else if (currentChapter is not null)
                {
                    chArticles.Add(article);
                }
                else
                {
                    direct.Add(article);
                }

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

    private static (string caption, string? referenceName) ParseHeading(string heading)
    {
        var match = LabelRegex.Match(heading);
        if (!match.Success || match.Groups[1].Value != "条")
        {
            return (heading.Trim(), null);
        }

        var name = match.Groups["name"].Value.Trim();
        var caption = LabelRegex.Replace(heading, string.Empty).Trim();
        return (caption, name);
    }

    private static List<ParagraphNode> ParseParagraphs(List<(string line, int sourceLine)> blockLines, string? filePath)
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
            if (paragraphLabelMatch.Success && paragraphLabelMatch.Groups[1].Value == "項")
            {
                FlushParagraph(lineNo);
                pendingParagraphRef = paragraphLabelMatch.Groups["name"].Value.Trim();
                paraStartLine = lineNo;
                continue;
            }

            if (TryParseItem(trim, out var itemTitle, out var itemText, out var isSubitem1))
            {
                if (isSubitem1)
                {
                    if (currentItems.Count == 0)
                    {
                        currentItems.Add(new ItemNode(++itemNo, null, "一", string.Empty, new(filePath, lineNo, 1), []));
                    }

                    var parent = currentItems[^1];
                    var children = parent.Children.ToList();
                    children.Add(new ItemNode(children.Count + 1, null, itemTitle, itemText, new(filePath, lineNo, 1), []));
                    currentItems[^1] = parent with { Children = children };
                }
                else
                {
                    currentItems.Add(new ItemNode(++itemNo, null, itemTitle, itemText, new(filePath, lineNo, 1), []));
                }

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

    private static bool TryParseItem(string text, out string title, out string sentence, out bool isSubitem1)
    {
        title = string.Empty;
        sentence = string.Empty;
        isSubitem1 = false;

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
            title = item.Groups["title"].Value;
            sentence = item.Groups["text"].Value.Trim();
            return true;
        }

        return false;
    }
}
