using Zuke.Core.Compilation;
using Zuke.Core.Markdown;
using Zuke.Core.Rendering;
using Xunit;

namespace Zuke.Core.Tests;

public class ParagraphNumberStyleTests
{
    [Fact]
    public void FrontMatterParser_ParsesParagraphNumberStyleAscii()
    {
        const string markdown = """
---
lawTitle: 就業規則
lawNum:
era: Reiwa
year: 1
num: 1
lawType: Misc
lang: ja
numberStyle: arabic
paragraphNumberStyle: ascii
---

# 本則

### 第1条 目的

本文。
2　会社は...
""";

        var parsed = FrontMatterParser.ParseDetailed(markdown);

        Assert.Equal("ascii", parsed.Metadata.ParagraphNumberStyle);
    }

    [Fact]
    public void MarkdownRoundTrip_PreservesAsciiParagraphNumbers()
    {
        const string markdown = """
---
lawTitle: 就業規則
lawNum:
era: Reiwa
year: 1
num: 1
lawType: Misc
lang: ja
numberStyle: arabic
paragraphNumberStyle: ascii
---

# 本則

### 第2条 目的

本条第1項にかかわらず、本条第1項第1号に定める。
2　会社は、本条第6項又は第7項を準用する。
3　従業員は、本条第3項第1号による。
""";

        var compiled = new LawMarkdownCompiler().Compile(markdown, "sample.md", new CompileOptions(false, true));
        Assert.False(compiled.HasErrors);

        var rendered = new LawtextRenderer().Render(compiled.Document!);

        Assert.Contains("\n2　会社は", rendered, StringComparison.Ordinal);
        Assert.Contains("\n3　従業員は", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("\n２　会社は", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("\n３　従業員は", rendered, StringComparison.Ordinal);
    }
}
