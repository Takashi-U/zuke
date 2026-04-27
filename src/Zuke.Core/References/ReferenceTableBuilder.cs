using Zuke.Core.Model;

namespace Zuke.Core.References;

public sealed class ReferenceTableBuilder
{
    public (IReadOnlyDictionary<string, ReferenceDefinition>, IReadOnlyList<DiagnosticMessage>) Build(LawDocumentModel model)
    {
        var table = new Dictionary<string, ReferenceDefinition>(StringComparer.OrdinalIgnoreCase);
        var diags = new List<DiagnosticMessage>();

        foreach (var article in EnumerateArticles(model))
        {
            AddRef(article.ReferenceName, LawElementKind.Article, article.Location, article.Number, null, null);
            foreach (var paragraph in article.Paragraphs)
            {
                AddRef(paragraph.ReferenceName, LawElementKind.Paragraph, paragraph.Location, article.Number, paragraph.Number, null);
                foreach (var item in paragraph.Items)
                {
                    AddRef(item.ReferenceName, LawElementKind.Item, item.Location, article.Number, paragraph.Number, item.Number);
                }
            }
        }

        return (table, diags);

        void AddRef(string? rawName, LawElementKind kind, SourceLocation? location, int articleNumber, int? paragraphNumber, int? itemNumber)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return;
            var normalized = rawName.Trim();
            var loc = location ?? new SourceLocation(null, 1, 1);
            if (table.ContainsKey(normalized))
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD022", $"参照名が重複しています: {normalized}", loc, []));
                return;
            }

            table[normalized] = new(rawName, normalized, kind, loc, articleNumber, paragraphNumber, itemNumber);
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
