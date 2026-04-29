using Xunit;

namespace Zuke.Core.Tests;

public class FrontMatterTests
{
    [Fact]
    public void MissingFrontMatter_IsLmd045()
    {
        var r = TestHelpers.Compile("# 総則\n## 目的\n本文\n");
        Assert.Contains(r.Diagnostics, d => d.Code == "LMD045");
    }

    [Fact]
    public void CrLfFrontMatter_IsParsed()
    {
        var md = "---\r\nlawTitle: T\r\nlawNum: N\r\nera: Reiwa\r\nyear: 6\r\nnum: 1\r\nlawType: Misc\r\nlang: ja\r\n---\r\n\r\n## 目的\r\n本文\r\n";
        var r = TestHelpers.Compile(md);
        Assert.DoesNotContain(r.Diagnostics, d => d.Code == "LMD045");
    }

    [Fact]
    public void FrontMatterWithUtf8Bom_IsParsed()
    {
        var md = "\uFEFF" + """
---
lawTitle: 育児・介護休業等に関する規則
---

## 目的
本文
""";
        var lawtext = TestHelpers.RenderLawtext(md);
        Assert.StartsWith("育児・介護休業等に関する規則", lawtext);
    }

    [Fact]
    public void MalformedFrontMatter_StillUsesLawTitleWhenPresent()
    {
        var md = """
---
lawTitle: 育児・介護休業等に関する規則
num: [
---

## 目的
本文
""";
        var lawtext = TestHelpers.RenderLawtext(md);
        Assert.StartsWith("育児・介護休業等に関する規則", lawtext);
    }
}
