using Zuke.Core.Markdown;
using Zuke.Core.Model;

namespace Zuke.Core.Parsing;

public sealed class MarkdownLawParser
{
    public LawDocumentModel Parse(string markdown, string? filePath)
    {
        var (meta, body) = FrontMatterParser.Parse(markdown);
        var chapters = new List<ChapterNode>();
        var direct = new List<ArticleNode>();
        var lines = body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        ChapterNode? currentChapter = null;
        SectionNode? currentSection = null;
        var chArticles = new List<ArticleNode>();
        var sections = new List<SectionNode>();
        var secArticles = new List<ArticleNode>();
        var articleNo = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                FlushSection();
                FlushChapter();
                currentChapter = new ChapterNode(0, line[2..].Replace("章 ", "", StringComparison.Ordinal), new(filePath, i + 1, 1), new List<SectionNode>(), new List<ArticleNode>());
                continue;
            }

            if (line.StartsWith("## 節 ", StringComparison.Ordinal))
            {
                FlushSection();
                currentSection = new SectionNode(0, line[5..], new(filePath, i + 1, 1), new List<ArticleNode>());
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal) || line.StartsWith("### ", StringComparison.Ordinal))
            {
                articleNo++;
                var t = line.Split(' ', 2)[1];
                var caption = t.Split('[')[0].Trim();
                var para = new ParagraphNode(1, null, null, NextText(lines, i + 1), new(filePath, i + 2, 1), new List<ItemNode>());
                var article = new ArticleNode(articleNo, null, caption, $"第{articleNo}条", new(filePath, i + 1, 1), new List<ParagraphNode> { para });

                if (currentSection is not null) secArticles.Add(article);
                else if (currentChapter is not null) chArticles.Add(article);
                else direct.Add(article);
            }
        }

        FlushSection();
        FlushChapter();
        return new LawDocumentModel(meta, chapters, direct, Array.Empty<DiagnosticMessage>());

        string NextText(string[] arr, int start)
        {
            for (var x = start; x < arr.Length; x++)
            {
                var t = arr[x].Trim();
                if (string.IsNullOrEmpty(t) || t.StartsWith("#", StringComparison.Ordinal)) continue;
                return t;
            }

            return string.Empty;
        }

        void FlushSection()
        {
            if (currentSection is null) return;
            sections.Add(currentSection with { Articles = secArticles.ToList() });
            secArticles.Clear();
            currentSection = null;
        }

        void FlushChapter()
        {
            if (currentChapter is null) return;
            chapters.Add(currentChapter with { Sections = sections.ToList(), Articles = chArticles.ToList() });
            sections.Clear();
            chArticles.Clear();
            currentChapter = null;
        }
    }
}
