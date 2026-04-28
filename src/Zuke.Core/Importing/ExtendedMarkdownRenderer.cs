using System.Text;
using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed class ExtendedMarkdownRenderer
{
    public (string Markdown, List<ImportMappingItem> MappingItems) Render(LawDocumentModel model, ExtendedMarkdownRenderOptions options)
    {
        var sb = new StringBuilder();
        var mapping = new List<ImportMappingItem>();
        if (options.MetadataMode.Equals("frontmatter", StringComparison.OrdinalIgnoreCase))
        {
            Append("---"); Append($"lawTitle: {model.Metadata.LawTitle}"); Append($"lawNum: {model.Metadata.LawNum}"); Append($"era: {model.Metadata.Era}"); Append($"year: {model.Metadata.Year}"); Append($"num: {model.Metadata.Num}"); Append($"lawType: {model.Metadata.LawType}"); Append($"lang: {model.Metadata.Lang}"); Append("---"); Append("");
        }
        foreach (var chapter in model.Chapters){ Append($"# {chapter.Title}"); Append(""); foreach (var section in chapter.Sections){ Append($"## 節 {section.Title}"); Append(""); foreach (var a in section.Articles) RenderArticle(a);} foreach (var a in chapter.Articles) RenderArticle(a);} foreach (var a in model.DirectArticles) RenderArticle(a);
        return (sb.ToString().Replace("\r\n","\n"), mapping);

        void RenderArticle(ArticleNode article)
        {
            var label = ShouldLabel(article.ReferenceName, true) ? $" [条:{article.ReferenceName}]" : "";
            Append($"### {(string.IsNullOrWhiteSpace(article.Caption) ? $"条 {article.ReferenceName}" : article.Caption)}{label}");
            mapping.Add(new("Article", article.Location?.Line ?? 1, GetLineNo(), $"第{article.Number}条", article.ReferenceName, article.Caption));
            Append("");
            foreach (var p in article.Paragraphs)
            {
                if (ShouldLabel(p.ReferenceName, false)) Append($"[項:{p.ReferenceName}]");
                mapping.Add(new("Paragraph", p.Location?.Line ?? 1, GetLineNo(), $"第{article.Number}条第{p.Number}項", p.ReferenceName, null));
                if (!string.IsNullOrWhiteSpace(p.SentenceText)) Append(p.SentenceText);
                foreach (var i in p.Items){ var l=ShouldLabel(i.ReferenceName,false)?$"[号:{i.ReferenceName}] ":""; Append($"- {l}{i.SentenceText}");}
                Append("");
            }
        }
        bool ShouldLabel(string? refName, bool isArticle) => options.ReferenceLabels.ToLowerInvariant() switch {"none"=>false,"used"=> isArticle || (!string.IsNullOrWhiteSpace(refName) && (options.UsedRefs?.Contains(refName)??false)), _=>true};
        int GetLineNo() => sb.ToString().Count(c=>c=="\n"[0])+1;
        void Append(string s)=>sb.AppendLine(s);
    }
}
