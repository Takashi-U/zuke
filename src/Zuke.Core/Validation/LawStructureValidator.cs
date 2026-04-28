using Zuke.Core.Model;

namespace Zuke.Core.Validation;

public sealed class LawStructureValidator
{
    public IReadOnlyList<DiagnosticMessage> Validate(LawDocumentModel model)
    {
        var diags = new List<DiagnosticMessage>();

        if (model.Chapters.Count > 0 && model.DirectArticles.Count > 0)
        {
            var loc = model.DirectArticles.First().Location ?? model.Chapters.First().Location;
            diags.Add(Error("LMD040", "MainProvision直下で混在できない構造があります（章構成と条構成）。", loc));
        }

        foreach (var chapter in model.Chapters)
        {
            if (chapter.Sections.Count > 0 && chapter.Articles.Count > 0)
            {
                diags.Add(Error("LMD041", "Chapter内でSectionあり構成とArticle直下構成が混在しています。", chapter.Location));
            }

            foreach (var section in chapter.Sections)
            {
                if (section.Articles.Count == 0)
                {
                    diags.Add(Error("LMD042", "Section内に未対応構造があります（条がありません）。", section.Location));
                }

                foreach (var article in section.Articles)
                {
                    ValidateArticle(article, diags);
                }
            }

            foreach (var article in chapter.Articles)
            {
                ValidateArticle(article, diags);
            }
        }

        foreach (var article in model.DirectArticles)
        {
            ValidateArticle(article, diags);
        }

        return diags;
    }

    private static void ValidateArticle(ArticleNode article, List<DiagnosticMessage> diags)
    {
        if (article.Paragraphs.Count == 0)
        {
            diags.Add(Error("LMD043", "ParagraphSentenceを生成できません（条に項がありません）。", article.Location));
            return;
        }

        foreach (var paragraph in article.Paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph.SentenceText) && paragraph.Items.Count == 0)
            {
                diags.Add(Error("LMD043", "ParagraphSentenceを生成できません（本文または号が必要です）。", paragraph.Location ?? article.Location));
            }

            foreach (var item in paragraph.Items)
            {
                ValidateItem(item, article, paragraph, diags);
            }
        }
    }

    private static void ValidateItem(ItemNode item, ArticleNode article, ParagraphNode paragraph, List<DiagnosticMessage> diags)
    {
        if (string.IsNullOrWhiteSpace(item.SentenceText) && item.Children.Count == 0)
        {
            diags.Add(Error("LMD043", "ParagraphSentenceを生成できません（号本文が空です）。", item.Location ?? paragraph.Location ?? article.Location));
        }

        foreach (var child in item.Children)
        {
            if (string.IsNullOrWhiteSpace(child.SentenceText))
            {
                diags.Add(Error("LMD043", "ParagraphSentenceを生成できません（Subitem1本文が空です）。", child.Location ?? item.Location ?? paragraph.Location ?? article.Location));
            }
        }
    }

    private static DiagnosticMessage Error(string code, string message, SourceLocation? location)
        => new(DiagnosticSeverity.Error, code, message, location, []);
}
