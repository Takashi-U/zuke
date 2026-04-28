using Zuke.Core.Numbering;
using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed class ReferenceNameGenerator
{
    public LawDocumentModel Apply(LawDocumentModel model, LawtextImportOptions options)
    {
        if (options.IdStyle.Equals("japanese", StringComparison.OrdinalIgnoreCase))
        {
            return ApplyJapanese(model);
        }

        return ApplyAscii(model);
    }

    private static LawDocumentModel ApplyAscii(LawDocumentModel model)
    {
        var chapters = model.Chapters.Select(ch => ch with
        {
            Articles = ch.Articles.Select(ApplyAsciiArticle).ToList(),
            Sections = ch.Sections.Select(s => s with { Articles = s.Articles.Select(ApplyAsciiArticle).ToList() }).ToList()
        }).ToList();

        var direct = model.DirectArticles.Select(ApplyAsciiArticle).ToList();
        return model with { Chapters = chapters, DirectArticles = direct };
    }

    private static ArticleNode ApplyAsciiArticle(ArticleNode article)
    {
        var articleRef = Numbering.ArticleNumberFormatter.ToReferenceName(article.ArticleNumber);
        var paragraphs = article.Paragraphs.Select(p => p with
        {
            ReferenceName = $"{articleRef}-p{p.Number}",
            Items = p.Items.Select(i => ApplyAsciiItem(i, articleRef, p.Number)).ToList()
        }).ToList();

        return article with { ReferenceName = articleRef, Paragraphs = paragraphs };
    }

    private static ItemNode ApplyAsciiItem(ItemNode item, string articleRef, int paragraphNumber)
    {
        var itemRef = $"{articleRef}-p{paragraphNumber}-i{item.Number}";
        return item with
        {
            ReferenceName = itemRef,
            Children = item.Children.Select(c => c with { ReferenceName = $"{itemRef}-sub1-{c.Number}" }).ToList()
        };
    }

    private static LawDocumentModel ApplyJapanese(LawDocumentModel model)
    {
        var used = new Dictionary<string, int>(StringComparer.Ordinal);
        string Unique(string key)
        {
            if (!used.TryGetValue(key, out var count))
            {
                used[key] = 1;
                return key;
            }

            count++;
            used[key] = count;
            return $"{key}-{count}";
        }

        ArticleNode Map(ArticleNode article)
        {
            var baseName = string.IsNullOrWhiteSpace(article.Caption) ? $"article-{article.Number}" : article.Caption.Replace(" ", "", StringComparison.Ordinal);
            var articleRef = Unique(baseName);
            return article with
            {
                ReferenceName = articleRef,
                Paragraphs = article.Paragraphs.Select(p => p with
                {
                    ReferenceName = $"{articleRef}-p{p.Number}",
                    Items = p.Items.Select(i => i with { ReferenceName = $"{articleRef}-p{p.Number}-i{i.Number}" }).ToList()
                }).ToList()
            };
        }

        return model with
        {
            Chapters = model.Chapters.Select(ch => ch with
            {
                Articles = ch.Articles.Select(Map).ToList(),
                Sections = ch.Sections.Select(s => s with { Articles = s.Articles.Select(Map).ToList() }).ToList()
            }).ToList(),
            DirectArticles = model.DirectArticles.Select(Map).ToList()
        };
    }
}
