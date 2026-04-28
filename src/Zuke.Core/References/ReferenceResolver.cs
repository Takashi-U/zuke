using Zuke.Core.Model;
using Zuke.Core.Parsing;

namespace Zuke.Core.References;

public sealed class ReferenceResolver
{
    public (LawDocumentModel, IReadOnlyList<DiagnosticMessage>) Resolve(LawDocumentModel model, IReadOnlyDictionary<string, ReferenceDefinition> table)
    {
        var diags = new List<DiagnosticMessage>();

        var chapters = model.Chapters
            .Select(ch => ch with
            {
                Sections = ch.Sections.Select(sec => sec with
                {
                    Articles = sec.Articles.Select(a => ResolveArticle(a, table, diags)).ToList()
                }).ToList(),
                Articles = ch.Articles.Select(a => ResolveArticle(a, table, diags)).ToList()
            })
            .ToList();

        var direct = model.DirectArticles.Select(a => ResolveArticle(a, table, diags)).ToList();

        return (model with { Chapters = chapters, DirectArticles = direct }, diags);
    }

    private static ArticleNode ResolveArticle(ArticleNode article, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags)
    {
        var paragraphs = article.Paragraphs.Select(p =>
        {
            var items = p.Items.Select(i => ResolveItem(article, p, i, table, diags)).ToList();
            var sentence = ResolveText(article, p, null, p.SentenceText, table, diags, p.Location);
            return p with { SentenceText = sentence, Items = items };
        }).ToList();

        return article with { Paragraphs = paragraphs };
    }

    private static ItemNode ResolveItem(ArticleNode article, ParagraphNode paragraph, ItemNode item, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags)
    {
        var text = ResolveText(article, paragraph, item, item.SentenceText, table, diags, item.Location);
        var children = item.Children.Select(c => c with { SentenceText = ResolveText(article, paragraph, c, c.SentenceText, table, diags, c.Location) }).ToList();
        return item with { SentenceText = text, Children = children };
    }

    private static string ResolveText(ArticleNode currentArticle, ParagraphNode currentParagraph, ItemNode? currentItem, string text, IReadOnlyDictionary<string, ReferenceDefinition> table, List<DiagnosticMessage> diags, SourceLocation? loc)
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

            if (option == ReferenceOption.Relative)
            {
                if (target.Kind == LawElementKind.Article && target.ArticleNumber == currentArticle.Number - 1)
                {
                    return "前条";
                }

                if (target.Kind == LawElementKind.Paragraph && target.ArticleNumber == currentArticle.Number && target.ParagraphNumber == currentParagraph.Number - 1)
                {
                    return "前項";
                }

                if (target.Kind == LawElementKind.Item && currentItem is not null && target.ArticleNumber == currentArticle.Number && target.ParagraphNumber == currentParagraph.Number && target.ItemNumber == currentItem.Number - 1)
                {
                    return "前号";
                }

                diags.Add(new(DiagnosticSeverity.Error, "LMD027", $"相対参照が不正です: {rawName}", loc, []));
                return rawName;
            }

            return option switch
            {
                ReferenceOption.ArticleOnly => $"第{ToKanji(target.ArticleNumber)}条",
                ReferenceOption.Full => RenderFull(target),
                ReferenceOption.Auto => RenderAuto(target),
                _ => RenderAuto(target)
            };
        });
    }

    private static string RenderAuto(ReferenceDefinition target)
    {
        return target.Kind switch
        {
            LawElementKind.Article => $"第{ToKanji(target.ArticleNumber)}条",
            LawElementKind.Paragraph => $"第{ToKanji(target.ArticleNumber)}条第{ToKanji(target.ParagraphNumber ?? 1)}項",
            LawElementKind.Item => $"第{ToKanji(target.ArticleNumber)}条第{ToKanji(target.ParagraphNumber ?? 1)}項第{ToKanji(target.ItemNumber ?? 1)}号",
            _ => target.RawName
        };
    }

    private static string RenderFull(ReferenceDefinition target) => RenderAuto(target);

    private static string ToKanji(int n) => n switch
    {
        1 => "一",
        2 => "二",
        3 => "三",
        4 => "四",
        5 => "五",
        6 => "六",
        7 => "七",
        8 => "八",
        9 => "九",
        10 => "十",
        _ => n.ToString()
    };
}
