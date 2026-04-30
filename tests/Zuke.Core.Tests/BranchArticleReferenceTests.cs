using Zuke.Core.Importing;
using Zuke.Core.Model;
using Zuke.Core.References;
using Zuke.Core.Rendering;
using Xunit;

namespace Zuke.Core.Tests;

public class BranchArticleReferenceTests
{
    [Fact]
    public void MarkdownReference_RendersBranchArticleFull()
    {
        var md = """
---
lawTitle: テスト規程
lawNum: 令和六年規則第一号
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第9条
本文。

## 第9条の2 [条:article-9-2]
[項:article-9-2-p1]
枝番号条文の本文。

## 第10条
{{参照:article-9-2-p1|完全}}に基づく申出。
""";
        var lawtext = TestHelpers.RenderLawtext(md);
        Assert.Contains("第九条の二第一項に基づく申出。", lawtext);
    }

    [Fact]
    public void Import_ResolvesBranchArticleReference()
    {
        var lawtext = """
テスト規程
（令和六年規則第一号）

（通常条文）
第9条　本文。

（枝番号条文）
第9条の2　枝番号条文の本文。

（準用）
第10条　第9条の2第1項に基づく申出。
""";
        var imported = new LawtextImportService().Import(lawtext, "x.law.txt", new());
        Assert.Contains("{{参照:article-9-2-p1|完全}}に基づく申出。", imported.Markdown);
    }

    [Fact]
    public void Xml_UsesBranchNum()
    {
        var article = new ArticleNode(1, "article-9-2", "", "第九条の二", new("x",1,1), [new ParagraphNode(1, "article-9-2-p1", null, "本文。", new("x",1,1), [])]) { ArticleNumber = new(9, [2]) };
        var model = new LawDocumentModel(new("T","N","Reiwa",6,1,"Misc","ja"), [], [article], []);
        var xml = new LawXmlRenderer().Render(model, LawXmlRenderOptions.Default).ToString();
        Assert.Contains("Num=\"9_2\"", xml);
        Assert.Contains("<ArticleTitle>第九条の二</ArticleTitle>", xml);
    }

    [Fact]
    public void PreviousArticle_IsDocumentOrder()
    {
        var a9 = new ArticleNode(9, "article-9", "", "第九条", new("x",1,1), [new ParagraphNode(1, "article-9-p1", null, "", new("x",1,1), [])]);
        var a92 = new ArticleNode(10, "article-9-2", "", "第九条の二", new("x",2,1), [new ParagraphNode(1, "article-9-2-p1", null, "", new("x",2,1), [])]) { ArticleNumber = new(9, [2]) };
        var a10 = new ArticleNode(11, "article-10", "", "第十条", new("x",3,1), [new ParagraphNode(1, "article-10-p1", null, "", new("x",3,1), [])]) { ArticleNumber = new(10, []) };
        var model = new LawDocumentModel(new("t","n","Reiwa",6,1,"Misc","ja"), [], [a9,a92,a10], []);
        var (table,_) = new ReferenceTableBuilder().Build(model);
        var diags = new List<DiagnosticMessage>();
        var r1 = new LawtextReferenceResolver().Resolve("前条", a92, a92.Paragraphs[0], null, table, diags, new("x",2,1), new(), []);
        var r2 = new LawtextReferenceResolver().Resolve("前条", a10, a10.Paragraphs[0], null, table, diags, new("x",3,1), new(), []);
        Assert.Equal("前条", r1);
        Assert.Equal("前条", r2);
    }
}
