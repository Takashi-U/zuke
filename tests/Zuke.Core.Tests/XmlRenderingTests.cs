using System.Xml.Linq;
using Xunit;
using Zuke.Core.Rendering;

namespace Zuke.Core.Tests;

public class XmlRenderingTests
{
    private const string FrontMatter = """
---
lawTitle: テスト規則
lawNum: テスト第1号
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---

""";

    [Fact]
    public void Smoke()
    {
        var result = TestHelpers.Compile();
        Assert.NotNull(result.Document);
    }

    [Fact]
    public void XmlItemTitleUsesJapaneseTitle()
    {
        const string markdown = """
# テスト規則

## 第一条
本文。

- 一 第一号
- 二 第二号
""";
        var xml = RenderXml(markdown);

        Assert.Contains("<Item Num=\"1\">", xml);
        Assert.Contains("<ItemTitle>一</ItemTitle>", xml);
        Assert.Contains("<Sentence Num=\"1\">第一号</Sentence>", xml);
        Assert.Contains("<ItemTitle>二</ItemTitle>", xml);
        Assert.DoesNotContain("一　第一号", xml);
        Assert.DoesNotContain("一 第一号", xml);
    }

    [Fact]
    public void XmlSubitemTitleUsesIroha()
    {
        const string markdown = """
# テスト規則

## 第一条
本文。

一　次のいずれか
  イ　イ号
  ロ　ロ号
""";
        var xml = RenderXml(markdown);

        Assert.Contains("<Subitem1Title>イ</Subitem1Title>", xml);
        Assert.Contains("<Sentence Num=\"1\">イ号</Sentence>", xml);
        Assert.DoesNotContain("イ　イ号", xml);
        Assert.DoesNotContain("イ イ号", xml);
    }

    [Fact]
    public void XmlDoesNotUseHyphenAsSubitemTitle()
    {
        const string markdown = """
# テスト規則

## 第一条
本文。

- 一 次のいずれか
  - - 通常勤務=...
""";
        var xml = RenderXml(markdown);

        Assert.DoesNotContain("<Subitem1Title>-</Subitem1Title>", xml);
        Assert.Contains("通常勤務=...", xml);
    }

    [Fact]
    public void XmlRendersSupplementaryProvisionAsParagraph()
    {
        const string markdown = """
# テスト規則

## 第一条
本文。

## 附則
本規則は、◯年◯月◯日から適用する。
""";
        var xml = RenderXml(markdown);

        Assert.Contains("<SupplProvision>", xml);
        Assert.Contains("<SupplProvisionLabel>附則</SupplProvisionLabel>", xml);
        Assert.DoesNotContain("<SupplProvisionSentence>", xml);
        Assert.Contains("<Paragraph Num=\"1\">", xml);
        Assert.Contains("<ParagraphNum />", xml);
        Assert.Contains("<ParagraphSentence><Sentence Num=\"1\">本規則は、◯年◯月◯日から適用する。</Sentence></ParagraphSentence>", xml);
    }

    private static string RenderXml(string markdown)
    {
        var result = TestHelpers.Compile(FrontMatter + markdown);
        Assert.False(result.HasErrors, string.Join("\n", result.Diagnostics.Select(d => $"{d.Code}:{d.Message}")));
        Assert.NotNull(result.Document);

        return new LawXmlRenderer().Render(result.Document!.Document).ToString(SaveOptions.DisableFormatting);
    }
}
