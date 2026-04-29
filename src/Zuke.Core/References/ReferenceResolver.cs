using Zuke.Core.Model;
using Zuke.Core.Numbering;
using Zuke.Core.Parsing;

namespace Zuke.Core.References;

public sealed class ReferenceResolver
{
    public (LawDocumentModel, IReadOnlyList<DiagnosticMessage>) Resolve(LawDocumentModel model, IReadOnlyDictionary<string, ReferenceDefinition> table, bool arabicNumbers = false)
    {
        var diags = new List<DiagnosticMessage>();

        var chapters = model.Chapters
            .Select(ch => ch with
            {
                Sections = ch.Sections.Select(sec => sec with
                {
                    Articles = sec.Articles.Select(a => ResolveArticle(a, table, diags, arabicNumbers)).ToList()
                }).ToList(),
                Articles = ch.Articles.Select(a => ResolveArticle(a, table, diags, arabicNumbers)).ToList()
            })
            .ToList();

        var direct = model.DirectArticles.Select(a => ResolveArticle(a, table, diags, arabicNumbers)).ToList();

        return (model with { Chapters = chapters, DirectArticles = direct }, diags);
    }

    private static ArticleNode ResolveArticle(ArticleNode article, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags, bool arabicNumbers)
    {
        var paragraphs = article.Paragraphs.Select(p =>
        {
            var items = p.Items.Select(i => ResolveItem(article, p, i, table, diags, arabicNumbers)).ToList();
            var sentence = ResolveText(article, p, null, p.SentenceText, table, diags, p.Location, arabicNumbers);
            return p with { SentenceText = sentence, Items = items };
        }).ToList();

        return article with { Paragraphs = paragraphs };
    }

    private static ItemNode ResolveItem(ArticleNode article, ParagraphNode paragraph, ItemNode item, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags, bool arabicNumbers)
    {
        var text = ResolveText(article, paragraph, item, item.SentenceText, table, diags, item.Location, arabicNumbers);
        var children = item.Children.Select(c => c with { SentenceText = ResolveText(article, paragraph, c, c.SentenceText, table, diags, c.Location, arabicNumbers) }).ToList();
        return item with { SentenceText = text, Children = children };
    }

    private static string ResolveText(ArticleNode currentArticle, ParagraphNode currentParagraph, ItemNode? currentItem, string text, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags, SourceLocation? loc, bool arabicNumbers)
    {
        return ReferenceParser.RefRegex.Replace(text, m =>
        {
            var rawName = m.Groups["name"].Value.Trim();
            if (!ReferenceNameNormalizer.TryNormalize(rawName, out var normalized))
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD023", $"参照名に禁止文字が含まれています: {rawName}", loc, []));
                return rawName;
            }
            var rawOpt = m.Groups["opt"].Value.Trim();
            if (!ReferenceParser.TryParseOption(rawOpt, out var option))
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD026", $"未対応の参照オプションです: {rawOpt}", loc, []));
                return rawName;
            }
            if (!table.TryGetValue(normalized, out var target))
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD021", $"参照を解決できません: {rawName}", loc, []));
                return rawName;
            }

            var currentIndex = FindCurrentArticleIndex(currentArticle, table);
            if (option == ReferenceOption.Relative)
            {
                if (target.Kind == LawElementKind.Article && currentIndex > 1 && target.DocumentArticleIndex == currentIndex - 1) return "前条";
                if (target.Kind == LawElementKind.Paragraph && target.DocumentArticleIndex == currentIndex && target.ParagraphNumber == currentParagraph.Number - 1) return "前項";
                if (target.Kind == LawElementKind.Item && currentItem is not null && target.DocumentArticleIndex == currentIndex && target.ParagraphNumber == currentParagraph.Number && target.ItemNumber == currentItem.Number - 1) return "前号";
                diags.Add(new(DiagnosticSeverity.Error, "LMD027", $"相対参照が不正です: {rawName}", loc, []));
                return rawName;
            }

            return option switch
            {
                ReferenceOption.ArticleOnly => ArticleNumberFormatter.ToArticleTitle(target.ArticleNumberValue, arabicNumbers),
                ReferenceOption.Full => RenderAuto(target, arabicNumbers),
                _ => RenderAuto(target, arabicNumbers)
            };
        });
    }

    private static int FindCurrentArticleIndex(ArticleNode article, IReadOnlyDictionary<string, ReferenceDefinition> table)
    {
        if (!string.IsNullOrWhiteSpace(article.ReferenceName) && ReferenceNameNormalizer.TryNormalize(article.ReferenceName, out var key) && table.TryGetValue(key, out var rd)) return rd.DocumentArticleIndex;
        return -1;
    }

    private static string RenderAuto(ReferenceDefinition target, bool arabicNumbers)
        => target.Kind switch
        {
            LawElementKind.Article => ArticleNumberFormatter.ToArticleTitle(target.ArticleNumberValue, arabicNumbers),
            LawElementKind.Paragraph => $"{ArticleNumberFormatter.ToArticleTitle(target.ArticleNumberValue, arabicNumbers)}{JapaneseNumberFormatter.ToParagraphReference(target.ParagraphNumber ?? 1, arabicNumbers)}",
            LawElementKind.Item => $"{ArticleNumberFormatter.ToArticleTitle(target.ArticleNumberValue, arabicNumbers)}{JapaneseNumberFormatter.ToParagraphReference(target.ParagraphNumber ?? 1, arabicNumbers)}{JapaneseNumberFormatter.ToItemReference(target.ItemNumber ?? 1, arabicNumbers)}",
            _ => target.RawName
        };
}
