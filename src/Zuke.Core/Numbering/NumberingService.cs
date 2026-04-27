using Zuke.Core.Model;

namespace Zuke.Core.Numbering;

public sealed class NumberingService
{
    public LawDocumentModel Apply(LawDocumentModel model, bool arabic)
    {
        var articleNo = 0;
        var chapters = model.Chapters.Select((c,ci) =>
        {
            var sections = c.Sections.Select((s,si) => s with
            {
                Number = si+1,
                Articles = s.Articles.Select(a => RenumberArticle(a, ++articleNo, arabic)).ToList()
            }).ToList();
            var arts = c.Articles.Select(a => RenumberArticle(a, ++articleNo, arabic)).ToList();
            return c with { Number = ci+1, Sections = sections, Articles = arts};
        }).ToList();
        var direct = model.DirectArticles.Select(a => RenumberArticle(a, ++articleNo, arabic)).ToList();
        return model with { Chapters = chapters, DirectArticles = direct };
    }

    private static ArticleNode RenumberArticle(ArticleNode a, int no, bool arabic)
    {
        var ps = a.Paragraphs.Select((p,idx)=> p with { Number = idx+1, ParagraphNumText = JapaneseNumberFormatter.ToParagraphNum(idx+1)}).ToList();
        return a with { Number = no, ArticleTitle = JapaneseNumberFormatter.ToArticle(no, arabic), Paragraphs = ps };
    }
}
