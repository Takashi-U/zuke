using Zuke.Core.Importing;
using Zuke.Core.Model;
using Zuke.Core.References;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportReferenceResolverTests
{
    [Fact]
    public void ResolvesRelativeAndAbsoluteReferences()
    {
        var article = new ArticleNode(2, "article-2", "", "第二条", new("x",1,1), [new ParagraphNode(1, "article-2-p1", null, "", new("x",1,1), []), new ParagraphNode(2, "article-2-p2", null, "前項", new("x",2,1), [])]);
        var model = new LawDocumentModel(new("t","", "Reiwa",0,0,"Misc","ja"), [], [article], []);
        var (table, _) = new ReferenceTableBuilder().Build(model);
        var diags = new List<DiagnosticMessage>();
        var used = new HashSet<string>();
        var resolved = new LawtextReferenceResolver().Resolve("前項", article, article.Paragraphs[1], null, table, diags, new("x",2,1), new(), used);
        Assert.Equal("{{参照:article-2-p1|相対}}", resolved);
    }
}
