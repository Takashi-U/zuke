using Xunit;
using Zuke.Core.Rendering;

namespace Zuke.Core.Tests;

public class NumberStyleTests
{
    private const string Markdown = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 参照先 [条:target]
- [号:item-12] 第十二号の対象

## 参照元
{{ref:target}}及び{{ref:item-12}}を参照する。
""";

    [Fact]
    public void KanjiStyle_IsAppliedToLawtextXmlAndReferences()
    {
        var result = TestHelpers.Compile(Markdown, arabicNumbers: false);
        Assert.NotNull(result.Document);

        var lawtext = new LawtextRenderer().Render(result.Document!, LawtextRenderOptions.Default with { ArabicNumbers = false });
        Assert.Contains("第一条", lawtext);
        Assert.Contains("第二条", lawtext);
        Assert.Contains("第一条及び第一条第一項第一号", lawtext);

        var xml = new LawXmlRenderer().Render(result.Document!.Document, LawXmlRenderOptions.Default with { ArabicNumbers = false });
        var xmlText = xml.ToString();
        Assert.Contains("<ArticleTitle>第一条</ArticleTitle>", xmlText);
        Assert.Contains("<ArticleTitle>第二条</ArticleTitle>", xmlText);
    }

    [Fact]
    public void ArabicStyle_IsAppliedToLawtextXmlAndReferences()
    {
        var result = TestHelpers.Compile(Markdown, arabicNumbers: true);
        Assert.NotNull(result.Document);

        var lawtext = new LawtextRenderer().Render(result.Document!, LawtextRenderOptions.Default with { ArabicNumbers = true });
        Assert.Contains("第1条", lawtext);
        Assert.Contains("第2条", lawtext);
        Assert.Contains("第1条及び第1条第1項第1号", lawtext);

        var xml = new LawXmlRenderer().Render(result.Document!.Document, LawXmlRenderOptions.Default with { ArabicNumbers = true });
        var xmlText = xml.ToString();
        Assert.Contains("<ArticleTitle>第1条</ArticleTitle>", xmlText);
        Assert.Contains("<ArticleTitle>第2条</ArticleTitle>", xmlText);
        Assert.Contains("<ItemTitle>一</ItemTitle>", xmlText);
    }
}
