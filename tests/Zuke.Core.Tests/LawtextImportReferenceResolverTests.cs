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
        Assert.Equal("前項", resolved);
    }

    [Theory]
    [InlineData("第1号及び第2号の措置")]
    [InlineData("第4項")]
    [InlineData("第4項及び第7項")]
    [InlineData("第4項から第7項")]
    [InlineData("本条第1項")]
    [InlineData("前条第3項")]
    [InlineData("次条第3項")]
    public void DoesNotExpandShortOrRelativeReferences(string text)
    {
        var article = new ArticleNode(1, "article-1", "", "第一条", new("x", 1, 1), [new ParagraphNode(1, "article-1-p1", null, "", new("x", 1, 1), [])]);
        var model = new LawDocumentModel(new("t", "", "Reiwa", 0, 0, "Misc", "ja"), [], [article], []);
        var (table, _) = new ReferenceTableBuilder().Build(model);
        var diags = new List<DiagnosticMessage>();
        var used = new HashSet<string>();

        var resolved = new LawtextReferenceResolver().Resolve(text, article, article.Paragraphs[0], null, table, diags, new("x", 1, 1), new(), used);

        Assert.Equal(text, resolved);
        Assert.DoesNotContain("{{参照:", resolved, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("第1条", "{{参照:article-1|完全}}")]
    [InlineData("第1条第1項", "{{参照:article-1-p1|完全}}")]
    [InlineData("第1条第1項第1号", "{{参照:article-1-p1-i1|完全}}")]
    public void ExpandsOnlyExplicitAbsoluteReferences(string text, string expected)
    {
        var article = new ArticleNode(1, "article-1", "", "第一条", new("x", 1, 1), [new ParagraphNode(1, "article-1-p1", null, "", new("x", 1, 1), [new ItemNode(1, "article-1-p1-i1", "一", "第一号", new("x", 1, 1), [])])]);
        var model = new LawDocumentModel(new("t", "", "Reiwa", 0, 0, "Misc", "ja"), [], [article], []);
        var (table, _) = new ReferenceTableBuilder().Build(model);
        var diags = new List<DiagnosticMessage>();
        var used = new HashSet<string>();

        var resolved = new LawtextReferenceResolver().Resolve(text, article, article.Paragraphs[0], null, table, diags, new("x", 1, 1), new(), used);

        Assert.Equal(expected, resolved);
    }
}
