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
            diags.Add(new(DiagnosticSeverity.Error, "LMD040", "MainProvision直下で混在できない構造があります（章と条）。", loc, []));
        }

        foreach (var chapter in model.Chapters)
        {
            if (chapter.Sections.Count > 0 && chapter.Articles.Count > 0)
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD041", "Chapter内でSectionあり構成とArticle直下構成が混在しています。", chapter.Location, []));
            }

            foreach (var section in chapter.Sections)
            {
                if (section.Articles.Count == 0)
                {
                    diags.Add(new(DiagnosticSeverity.Error, "LMD042", "Section内に未対応構造があります。", section.Location, []));
                }
            }
        }

        foreach (var article in EnumerateArticles(model))
        {
            if (article.Paragraphs.Count == 0)
            {
                diags.Add(new(DiagnosticSeverity.Error, "LMD043", "ParagraphSentenceを生成できません（項がありません）。", article.Location, []));
                continue;
            }

            foreach (var paragraph in article.Paragraphs)
            {
                if (string.IsNullOrWhiteSpace(paragraph.SentenceText) && paragraph.Items.Count == 0)
                {
                    diags.Add(new(DiagnosticSeverity.Error, "LMD043", "ParagraphSentenceを生成できません。", paragraph.Location ?? article.Location, []));
                }
            }
        }

        return diags;
    }

    private static IEnumerable<ArticleNode> EnumerateArticles(LawDocumentModel model)
    {
        foreach (var ch in model.Chapters)
        {
            foreach (var sec in ch.Sections)
            {
                foreach (var a in sec.Articles) yield return a;
            }

            foreach (var a in ch.Articles) yield return a;
        }

        foreach (var a in model.DirectArticles) yield return a;
    }
}
