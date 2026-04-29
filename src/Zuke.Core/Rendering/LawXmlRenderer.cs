using System.Xml.Linq;
using Zuke.Core.Model;
using Zuke.Core.Numbering;

namespace Zuke.Core.Rendering;

public sealed class LawXmlRenderer
{
    public XDocument Render(LawDocumentModel model, LawXmlRenderOptions? options = null)
    {
        options ??= LawXmlRenderOptions.Default;
        var root = new XElement("Law",
            new XAttribute("Era", model.Metadata.Era),
            new XAttribute("Year", model.Metadata.Year),
            new XAttribute("Num", model.Metadata.Num),
            new XAttribute("LawType", model.Metadata.LawType),
            new XAttribute("Lang", model.Metadata.Lang),
            new XElement("LawNum", model.Metadata.LawNum),
            new XElement("LawBody", new XElement("LawTitle", model.Metadata.LawTitle), new XElement("MainProvision", RenderMain(model, options))));
        return new XDocument(root);
    }

    private static IEnumerable<XElement> RenderMain(LawDocumentModel model, LawXmlRenderOptions options)
    {
        foreach (var c in model.Chapters)
        {
            yield return new XElement("Chapter", new XAttribute("Num", c.Number), new XElement("ChapterTitle", $"{JapaneseNumberFormatter.ToChapter(c.Number, options.ArabicNumbers)}　{c.Title}"), c.Sections.Select(s => RenderSection(s, options)), c.Articles.Select(a => RenderArticle(a, options)));
        }

        foreach (var a in model.DirectArticles)
        {
            yield return RenderArticle(a, options);
        }
    }

    private static XElement RenderSection(SectionNode s, LawXmlRenderOptions options)
        => new("Section", new XAttribute("Num", s.Number), new XElement("SectionTitle", $"{JapaneseNumberFormatter.ToSection(s.Number, options.ArabicNumbers)}　{s.Title}"), s.Articles.Select(a => RenderArticle(a, options)));

    private static XElement RenderArticle(ArticleNode a, LawXmlRenderOptions options)
    {
        var elements = new List<object>
        {
            new XAttribute("Num", ArticleNumberFormatter.ToXmlNum(a.ArticleNumber)),
            new XElement("ArticleTitle", ArticleNumberFormatter.ToArticleTitle(a.ArticleNumber, options.ArabicNumbers))
        };

        if (!string.IsNullOrWhiteSpace(a.Caption))
        {
            var cap = a.Caption.Trim();
            if (!string.IsNullOrWhiteSpace(cap))
            {
                elements.Insert(1, new XElement("ArticleCaption", $"（{cap}）"));
            }
        }

        elements.AddRange(a.Paragraphs.Select(p => RenderParagraph(p, options)));
        return new XElement("Article", elements.ToArray());
    }

    private static XElement RenderParagraph(ParagraphNode p, LawXmlRenderOptions options)
    {
        var sentence = string.IsNullOrWhiteSpace(p.SentenceText) ? string.Empty : p.SentenceText;
        return new("Paragraph", new XAttribute("Num", p.Number), string.IsNullOrEmpty(p.ParagraphNumText) ? new XElement("ParagraphNum") : new XElement("ParagraphNum", p.ParagraphNumText), new XElement("ParagraphSentence", new XElement("Sentence", new XAttribute("Num", 1), sentence)), p.Items.Select(i => RenderItem(i, options)));
    }

    private static XElement RenderItem(ItemNode i, LawXmlRenderOptions options)
    {
        var e = new XElement("Item", new XAttribute("Num", i.Number), new XElement("ItemTitle", JapaneseNumberFormatter.ToItemTitle(i.Number, options.ArabicNumbers)), new XElement("ItemSentence", new XElement("Sentence", new XAttribute("Num", 1), i.SentenceText)));
        foreach (var child in i.Children)
        {
            e.Add(new XElement("Subitem1", new XAttribute("Num", child.Number), new XElement("Subitem1Title", child.ItemTitle), new XElement("Subitem1Sentence", new XElement("Sentence", new XAttribute("Num", 1), child.SentenceText))));
        }

        return e;
    }
}
