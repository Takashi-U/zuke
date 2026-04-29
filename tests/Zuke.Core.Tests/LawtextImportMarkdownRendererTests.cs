using Zuke.Core.Importing;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportMarkdownRendererTests
{
    [Fact]
    public void RendersChapterSectionAndLabels()
    {
        var text = File.ReadAllText(Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt"));
        var result = new LawtextImportService().Import(text, "sample", new());
        Assert.Contains("# 総則", result.Markdown);
        Assert.Contains("## 節 通則", result.Markdown);
        Assert.Contains("[条:article-1]", result.Markdown);
        Assert.Contains("{{参照:article-2-p1|相対}}", result.Markdown);
        Assert.Contains("{{参照:article-2-p1|完全}}", result.Markdown);
    }

    [Fact]
    public void RoundTrip_KeepsItemNumbersAndSubitems()
    {
        var lawtext = """
題名
第1条　本文。
  一　甲
  二　次のいずれかの事情があること
    イ　保育所等
    ロ　配偶者事情
  三　第三号
""";
        var imported = new LawtextImportService().Import(lawtext, "sample", new());
        var roundtrip = TestHelpers.RenderLawtext(imported.Markdown);
        Assert.Contains("一　甲", roundtrip);
        Assert.Contains("二　次のいずれかの事情があること", roundtrip);
        Assert.Contains("  イ　保育所等", roundtrip);
        Assert.Contains("  ロ　配偶者事情", roundtrip);
        Assert.Contains("\n三　", roundtrip);
        Assert.DoesNotContain("\n1　", roundtrip);
        Assert.DoesNotContain("- イ", roundtrip);
    }

    [Fact]
    public void RoundTrip_KeepsMultiAndRangeReferences()
    {
        var lawtext = """
題名
第1条　本文。
第2条　本条第1項、第3項から第7項にかかわらず
  一　本条第4項又は第5項に基づく
""";
        var imported = new LawtextImportService().Import(lawtext, "sample", new());
        var roundtrip = TestHelpers.RenderLawtext(imported.Markdown);
        Assert.Contains("本条第1項、第3項から第7項にかかわらず", roundtrip);
        Assert.Contains("本条第4項又は第5項に基づく", roundtrip);
        Assert.DoesNotContain("第2条第3項から第2条第7項", roundtrip);
        Assert.DoesNotContain("第2条第4項又は第3条第5項", roundtrip);
    }
}
