using Zuke.Core.Model;

namespace Zuke.Core.References;

public sealed class ReferenceTableBuilder
{
    public (IReadOnlyDictionary<string, ReferenceDefinition>, IReadOnlyList<DiagnosticMessage>) Build(LawDocumentModel model)
    {
        var table = new Dictionary<string, ReferenceDefinition>(StringComparer.OrdinalIgnoreCase);
        var diags = new List<DiagnosticMessage>();

        var articleIndex = 0;
        foreach (var article in EnumerateArticles(model))
        {
            articleIndex++;
            AddRef(article.ReferenceName, LawElementKind.Article, article.Location, article.Number, article.ArticleNumber, null, null, articleIndex);
            foreach (var paragraph in article.Paragraphs)
            {
                AddRef(paragraph.ReferenceName, LawElementKind.Paragraph, paragraph.Location, article.Number, article.ArticleNumber, paragraph.Number, null, articleIndex);
                foreach (var item in paragraph.Items)
                {
                    AddRef(item.ReferenceName, LawElementKind.Item, item.Location, article.Number, article.ArticleNumber, paragraph.Number, item.Number, articleIndex);
                }
            }
        }

        return (table, diags);

        void AddRef(string? rawName, LawElementKind kind, SourceLocation? location, int articleNumber, Numbering.ArticleNumber articleNumberValue, int? paragraphNumber, int? itemNumber, int documentArticleIndex)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return;
            var loc = location ?? new SourceLocation(null, 1, 1);
            if (!ReferenceNameNormalizer.TryNormalize(rawName, out var normalized))
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD023", $"参照名に禁止文字が含まれています: {rawName}", loc, []));
                return;
            }

            if (table.TryGetValue(normalized, out var existing))
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD022", $"参照名が重複しています: {rawName}", loc, [existing.Location, loc]));
                return;
            }

            table[normalized] = new(rawName, normalized, kind, loc, articleNumber, articleNumberValue, paragraphNumber, itemNumber, documentArticleIndex);
        }
    }

    private static IEnumerable<ArticleNode> EnumerateArticles(LawDocumentModel model)
    {
        foreach (var chapter in model.Chapters)
        {
            foreach (var section in chapter.Sections)
            {
                foreach (var article in section.Articles)
                {
                    yield return article;
                }
            }

            foreach (var article in chapter.Articles)
            {
                yield return article;
            }
        }

        foreach (var article in model.DirectArticles)
        {
            yield return article;
        }
    }
}
