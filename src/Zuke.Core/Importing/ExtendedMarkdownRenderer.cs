using System.Text;
using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed class ExtendedMarkdownRenderer
{
    private const string DefaultLineNumberText = "1";

    public (string Markdown, List<ImportMappingItem> MappingItems) Render(LawDocumentModel model, ExtendedMarkdownRenderOptions options)
    {
        var sb = new StringBuilder();
        var mapping = new List<ImportMappingItem>();

        if (options.MetadataMode.Equals("frontmatter", StringComparison.OrdinalIgnoreCase))
        {
            Append("---");
            Append($"lawTitle: {model.Metadata.LawTitle}");
            Append($"lawNum: {model.Metadata.LawNum}");
            Append($"era: {model.Metadata.Era}");
            Append($"year: {model.Metadata.Year}");
            Append($"num: {model.Metadata.Num}");
            Append($"lawType: {model.Metadata.LawType}");
            Append($"lang: {model.Metadata.Lang}");
            Append($"numberStyle: {model.Metadata.NumberStyle}");
            Append("---");
            Append("");
        }

        foreach (var chapter in model.Chapters)
        {
            Append($"# {chapter.Title}");
            Append("");
            foreach (var section in chapter.Sections)
            {
                Append($"## 節 {section.Title}");
                Append("");
                foreach (var article in section.Articles) RenderArticle(article);
            }
            foreach (var article in chapter.Articles) RenderArticle(article);
        }

        foreach (var article in model.DirectArticles) RenderArticle(article);

        return (sb.ToString().Replace("\r\n","\n"), mapping);

        void RenderArticle(ArticleNode article)
        {
            var label = ShouldEmitLabel(article.ReferenceName, "Article") ? $" [条:{article.ReferenceName}]" : "";
            var articleTitle = article.ArticleNumber is null
                ? BuildArticleNumber(article)
                : $"第{article.ArticleNumber.BaseNumber}条{string.Concat(article.ArticleNumber.BranchNumbers.Select(b => $"の{b}"))}";
            Append($"### {articleTitle}{(string.IsNullOrWhiteSpace(article.Caption) ? "" : $" {article.Caption}")}{label}");
            mapping.Add(CreateMapping("Article", article.Location?.Line, BuildArticleNumber(article), article.ReferenceName, article.Caption));
            Append("");
            foreach (var paragraph in article.Paragraphs) RenderParagraph(article, paragraph);
        }

        void RenderParagraph(ArticleNode article, ParagraphNode paragraph)
        {
            if (ShouldEmitLabel(paragraph.ReferenceName, "Paragraph")) Append($"[項:{paragraph.ReferenceName}]");
            mapping.Add(CreateMapping("Paragraph", paragraph.Location?.Line, BuildParagraphNumber(article, paragraph), paragraph.ReferenceName, null));

            if (!string.IsNullOrWhiteSpace(paragraph.SentenceText)) Append(paragraph.SentenceText);
            foreach (var item in paragraph.Items) RenderItem(article, paragraph, item);
            Append("");
        }

        void RenderItem(ArticleNode article, ParagraphNode paragraph, ItemNode item)
        {
            var label = ShouldEmitLabel(item.ReferenceName, "Item") ? $"[号:{item.ReferenceName}] " : string.Empty;
            Append($"- {label}{item.SentenceText}");
            mapping.Add(CreateMapping("Item", item.Location?.Line, $"{BuildParagraphNumber(article, paragraph)}第{item.Number}号", item.ReferenceName, null));
            foreach (var subitem in item.Children)
            {
                Append($"  - {subitem.ItemTitle} {subitem.SentenceText}");
                mapping.Add(CreateMapping("Subitem1", subitem.Location?.Line, $"{BuildParagraphNumber(article, paragraph)}第{item.Number}号{subitem.ItemTitle}", subitem.ReferenceName, null));
            }
        }

        ImportMappingItem CreateMapping(string kind, int? lawtextLine, string number, string? referenceName, string? caption)
            => new(kind, lawtextLine ?? 1, GetCurrentMarkdownLine(sb), number, referenceName, caption);

        bool ShouldEmitLabel(string? refName, string kind)
        {
            var mode = options.ReferenceLabels.ToLowerInvariant();
            if (mode == "none") return false;
            if (mode == "all") return true;
            if (kind == "Article") return true;
            return !string.IsNullOrWhiteSpace(refName) && (options.UsedRefs?.Contains(refName) ?? false);
        }

        static int GetCurrentMarkdownLine(StringBuilder markdown) => markdown.ToString().Count(c=>c=='\n') + 1;
        static string BuildArticleNumber(ArticleNode article) => $"第{article.Number}条";
        static string BuildParagraphNumber(ArticleNode article, ParagraphNode paragraph) => $"第{article.Number}条第{paragraph.Number}項";
        void Append(string s) => sb.AppendLine(s);
    }
}
