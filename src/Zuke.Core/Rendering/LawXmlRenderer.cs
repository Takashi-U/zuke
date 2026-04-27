using System.Xml.Linq;
using Zuke.Core.Model;

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
            yield return new XElement("Chapter", new XAttribute("Num", c.Number), new XElement("ChapterTitle", $"第{c.Number}章　{c.Title}"), c.Sections.Select(RenderSection), c.Articles.Select(RenderArticle));
        }
        foreach (var a in model.DirectArticles) yield return RenderArticle(a);
    }

    private static XElement RenderSection(SectionNode s) => new("Section", new XAttribute("Num", s.Number), new XElement("SectionTitle", $"第{s.Number}節　{s.Title}"), s.Articles.Select(RenderArticle));

    private static XElement RenderArticle(ArticleNode a) => new("Article", new XAttribute("Num", a.Number), new XElement("ArticleCaption", $"（{a.Caption}）"), new XElement("ArticleTitle", a.ArticleTitle), a.Paragraphs.Select(RenderParagraph));

    private static XElement RenderParagraph(ParagraphNode p) => new("Paragraph", new XAttribute("Num", p.Number), string.IsNullOrEmpty(p.ParagraphNumText)?new XElement("ParagraphNum"):new XElement("ParagraphNum",p.ParagraphNumText), new XElement("ParagraphSentence", new XElement("Sentence", new XAttribute("Num", 1), p.SentenceText)), p.Items.Select(RenderItem));
    private static XElement RenderItem(ItemNode i)=> new("Item", new XAttribute("Num", i.Number), new XElement("ItemTitle", i.ItemTitle), new XElement("ItemSentence", new XElement("Sentence", new XAttribute("Num", 1), i.SentenceText)));
}
