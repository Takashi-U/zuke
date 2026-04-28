using System.Xml.Linq;
using Zuke.Core.Model;
using Zuke.Core.Numbering;

namespace Zuke.Core.Rendering;

public sealed class LawXmlRenderer
{
    public XDocument Render(LawDocumentModel model)
    {
        var root = new XElement("Law",
            new XAttribute("Era", model.Metadata.Era),
            new XAttribute("Year", model.Metadata.Year),
            new XAttribute("Num", model.Metadata.Num),
            new XAttribute("LawType", model.Metadata.LawType),
            new XAttribute("Lang", model.Metadata.Lang),
            new XElement("LawNum", model.Metadata.LawNum),
            new XElement("LawBody", new XElement("LawTitle", model.Metadata.LawTitle), new XElement("MainProvision", RenderMain(model))));
        return new XDocument(root);
    }

    private static IEnumerable<XElement> RenderMain(LawDocumentModel model)
    {
        foreach (var c in model.Chapters)
        {
            yield return new XElement("Chapter", new XAttribute("Num", c.Number), new XElement("ChapterTitle", $"第{ToKanji(c.Number)}章　{c.Title}"), c.Sections.Select(RenderSection), c.Articles.Select(RenderArticle));
        }

        foreach (var a in model.DirectArticles)
        {
            yield return RenderArticle(a);
        }
    }

    private static XElement RenderSection(SectionNode s)
        => new("Section", new XAttribute("Num", s.Number), new XElement("SectionTitle", $"第{ToKanji(s.Number)}節　{s.Title}"), s.Articles.Select(RenderArticle));

    private static XElement RenderArticle(ArticleNode a)
    {
        var elements = new List<object>
        {
            new XAttribute("Num", a.Number),
            new XElement("ArticleTitle", a.ArticleTitle)
        };

        if (!string.IsNullOrWhiteSpace(a.Caption))
        {
            elements.Insert(1, new XElement("ArticleCaption", $"（{a.Caption}）"));
        }

        elements.AddRange(a.Paragraphs.Select(RenderParagraph));
        return new XElement("Article", elements.ToArray());
    }

    private static XElement RenderParagraph(ParagraphNode p)
        => new("Paragraph", new XAttribute("Num", p.Number), string.IsNullOrEmpty(p.ParagraphNumText) ? new XElement("ParagraphNum") : new XElement("ParagraphNum", p.ParagraphNumText), new XElement("ParagraphSentence", new XElement("Sentence", new XAttribute("Num", 1), p.SentenceText)), p.Items.Select(RenderItem));

    private static XElement RenderItem(ItemNode i)
    {
        var e = new XElement("Item", new XAttribute("Num", i.Number), new XElement("ItemTitle", string.IsNullOrWhiteSpace(i.ItemTitle) ? ToKanji(i.Number) : i.ItemTitle), new XElement("ItemSentence", new XElement("Sentence", new XAttribute("Num", 1), i.SentenceText)));
        foreach (var child in i.Children)
        {
            e.Add(new XElement("Subitem1", new XAttribute("Num", child.Number), new XElement("Subitem1Title", child.ItemTitle), new XElement("Subitem1Sentence", new XElement("Sentence", new XAttribute("Num", 1), child.SentenceText))));
        }

        return e;
    }

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
