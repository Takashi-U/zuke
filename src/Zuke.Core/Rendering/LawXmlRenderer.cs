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
            new XElement("LawBody", new XElement("LawTitle", model.Metadata.LawTitle), new XElement("MainProvision", RenderMain(model, options)), RenderSupplementaryProvisions(model)));
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
        var paragraph = new XElement("Paragraph", new XAttribute("Num", p.Number), string.IsNullOrEmpty(p.ParagraphNumText) ? new XElement("ParagraphNum") : new XElement("ParagraphNum", p.ParagraphNumText), new XElement("ParagraphSentence", new XElement("Sentence", new XAttribute("Num", 1), sentence)));
        foreach (var item in p.Items)
        {
            if (item.IsRawBullet)
            {
                paragraph.Add(RenderList(item));
            }
            else
            {
                paragraph.Add(RenderItem(item, options));
            }
        }

        return paragraph;
    }

    private static XElement RenderItem(ItemNode i, LawXmlRenderOptions options)
    {
        var itemTitle = string.IsNullOrWhiteSpace(i.ItemTitle)
            ? JapaneseNumberFormatter.ToItemTitle(i.Number, options.ArabicNumbers)
            : i.ItemTitle;
        var itemSentence = NormalizeSentence(i.SentenceText, itemTitle);
        var e = new XElement("Item", new XAttribute("Num", i.Number), new XElement("ItemTitle", itemTitle), new XElement("ItemSentence", new XElement("Sentence", new XAttribute("Num", 1), itemSentence)));
        foreach (var child in i.Children)
        {
            if (child.IsRawBullet)
            {
                e.Add(RenderList(child));
                continue;
            }

                        var subitemSentence = NormalizeSentence(child.SentenceText, child.ItemTitle);
            e.Add(new XElement("Subitem1", new XAttribute("Num", child.Number), new XElement("Subitem1Title", child.ItemTitle), new XElement("Subitem1Sentence", new XElement("Sentence", new XAttribute("Num", 1), subitemSentence))));
        }

        return e;
    }

    private static XElement RenderList(ItemNode rawListItem)
    {
        return new XElement("List",
            new XElement("ListSentence",
                new XElement("Sentence", new XAttribute("Num", 1), rawListItem.SentenceText)));
    }

    private static IEnumerable<XElement> RenderSupplementaryProvisions(LawDocumentModel model)
    {
        foreach (var supplementaryProvision in model.SupplementaryProvisions)
        {
            var supplElements = new List<object>
            {
                new XElement("SupplProvisionLabel", supplementaryProvision.Title)
            };

            if (supplementaryProvision.Lines.Count > 0)
            {
                supplElements.Add(
                    new XElement("Paragraph",
                        new XAttribute("Num", 1),
                        new XElement("ParagraphNum"),
                        new XElement("ParagraphSentence",
                            supplementaryProvision.Lines.Select((line, idx) =>
                                new XElement("Sentence", new XAttribute("Num", idx + 1), line)))));
            }

            yield return new XElement("SupplProvision", supplElements.ToArray());
        }
    }

    private static string NormalizeSentence(string? sentenceText, string title)
    {
        var sentence = sentenceText ?? string.Empty;
        if (string.IsNullOrWhiteSpace(sentence) || string.IsNullOrWhiteSpace(title)) return sentence;
        if (!sentence.StartsWith(title, StringComparison.Ordinal)) return sentence;
        if (sentence.Length == title.Length) return string.Empty;

        var next = sentence[title.Length];
        if (next is ' ' or '　' or '\t')
        {
            return sentence[(title.Length + 1)..].TrimStart(' ', '　', '\t');
        }

        return sentence;
    }
}
