using System.Text;
using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed class ExtendedMarkdownRenderer
{
    public string Render(LawDocumentModel model, ExtendedMarkdownRenderOptions options)
    {
        var sb = new StringBuilder();
        if (options.MetadataMode.Equals("frontmatter", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("---");
            sb.AppendLine($"lawTitle: {model.Metadata.LawTitle}");
            sb.AppendLine($"lawNum: {model.Metadata.LawNum}");
            sb.AppendLine($"era: {model.Metadata.Era}");
            sb.AppendLine($"year: {model.Metadata.Year}");
            sb.AppendLine($"num: {model.Metadata.Num}");
            sb.AppendLine($"lawType: {model.Metadata.LawType}");
            sb.AppendLine($"lang: {model.Metadata.Lang}");
            sb.AppendLine("---");
            sb.AppendLine();
        }

        foreach (var chapter in model.Chapters)
        {
            sb.AppendLine($"# {chapter.Title}");
            sb.AppendLine();
            foreach (var section in chapter.Sections)
            {
                sb.AppendLine($"## 節 {section.Title}");
                sb.AppendLine();
                foreach (var article in section.Articles)
                {
                    RenderArticle(sb, article, options);
                }
            }

            foreach (var article in chapter.Articles) RenderArticle(sb, article, options);
        }

        foreach (var article in model.DirectArticles) RenderArticle(sb, article, options);

        return sb.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void RenderArticle(StringBuilder sb, ArticleNode article, ExtendedMarkdownRenderOptions options)
    {
        var title = string.IsNullOrWhiteSpace(article.Caption) ? $"条 {article.ReferenceName}" : article.Caption;
        var label = options.ReferenceLabels.Equals("none", StringComparison.OrdinalIgnoreCase) ? "" : $" [条:{article.ReferenceName}]";
        sb.AppendLine($"### {title}{label}");
        sb.AppendLine();
        foreach (var paragraph in article.Paragraphs)
        {
            if (!options.ReferenceLabels.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"[項:{paragraph.ReferenceName}]");
            }

            if (!string.IsNullOrWhiteSpace(paragraph.SentenceText))
            {
                sb.AppendLine(paragraph.SentenceText);
            }

            foreach (var item in paragraph.Items)
            {
                var itemLabel = options.ReferenceLabels.Equals("none", StringComparison.OrdinalIgnoreCase) ? "" : $"[号:{item.ReferenceName}] ";
                sb.AppendLine($"- {itemLabel}{item.SentenceText}");
                foreach (var child in item.Children)
                {
                    var childLabel = options.ReferenceLabels.Equals("none", StringComparison.OrdinalIgnoreCase) ? "" : $"[号:{child.ReferenceName}] ";
                    sb.AppendLine($"  - {childLabel}{child.SentenceText}");
                }
            }

            sb.AppendLine();
        }
    }
}
