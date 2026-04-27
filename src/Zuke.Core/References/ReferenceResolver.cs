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
            var sentence = ReferenceParser.ResolveInline(p.SentenceText, (name, option) =>
            {
                if (!table.TryGetValue(name, out var target))
                {
                    diags.Add(new(DiagnosticSeverity.Error, "LMD021", $"参照を解決できません: {name}", p.Location, []));
                    return name;
                }

                if (option == ReferenceOption.Relative)
                {
                    if (target.ArticleNumber != article.Number || target.ParagraphNumber != p.Number - 1)
                    {
                        diags.Add(new(DiagnosticSeverity.Error, "LMD027", $"相対参照が不正です: {name}", p.Location, []));
                        return "前項";
                    }

                    return "前項";
                }

                return option switch
                {
                    ReferenceOption.ArticleOnly => $"第{target.ArticleNumber}条",
                    _ => target.ParagraphNumber.HasValue
                        ? $"第{target.ArticleNumber}条第{target.ParagraphNumber.Value}項"
                        : $"第{target.ArticleNumber}条"
                };
            });
            return p with { SentenceText = sentence };
        }).ToList();

        return article with { Paragraphs = paragraphs };
    }
}
