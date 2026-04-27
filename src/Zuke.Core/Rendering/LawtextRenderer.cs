using System.Text;
using Zuke.Core.Model;

namespace Zuke.Core.Rendering;

public sealed class LawtextRenderer
{
    public string Render(LawDocumentModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine(model.Metadata.LawTitle);
        sb.AppendLine($"（{model.Metadata.LawNum}）");
        sb.AppendLine();
        foreach (var c in model.Chapters)
        {
            sb.AppendLine($"第{c.Number}章　{c.Title}");
            foreach (var s in c.Sections)
            {
                sb.AppendLine($"第{s.Number}節　{s.Title}");
                foreach (var a in s.Articles) RenderArticle(sb,a);
            }
            foreach (var a in c.Articles) RenderArticle(sb,a);
        }
        foreach (var a in model.DirectArticles) RenderArticle(sb,a);
        return sb.ToString();
    }

    private static void RenderArticle(StringBuilder sb, ArticleNode a)
    {
        sb.AppendLine($"（{a.Caption}）");
        for (int i=0;i<a.Paragraphs.Count;i++)
        {
            var p=a.Paragraphs[i];
            var head = i==0 ? a.ArticleTitle+"　" : (p.ParagraphNumText ?? "") + "　";
            sb.AppendLine(head + p.SentenceText);
            foreach (var item in p.Items) sb.AppendLine($"{item.ItemTitle}　{item.SentenceText}");
        }
        sb.AppendLine();
    }
}
