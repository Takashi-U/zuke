using Xunit;

namespace Zuke.Core.Tests;

public class LawtextPropertyLikeTests
{
    [Fact]
    public void GeneratedPatternsRenderWithoutResidualMacros()
    {
        foreach (var chapters in new[] { 1, 2 })
        foreach (var sections in new[] { false, true })
        foreach (var articles in new[] { 1, 2 })
        foreach (var paragraphs in new[] { 1, 2, 3 })
        {
            var md = Generate(chapters, sections, articles, paragraphs);
            var result = TestHelpers.Compile(md);
            Assert.False(result.HasErrors);
            var lawtext = new Zuke.Core.Rendering.LawtextRenderer().Render(result.Document!);
            Assert.DoesNotContain("{{", lawtext);
            Assert.DoesNotContain("[項:", lawtext);
            Assert.DoesNotContain("🍣", lawtext);
            Assert.DoesNotContain("\r", lawtext);
            Assert.EndsWith("\n", lawtext);
        }
    }

    private static string Generate(int chapterCount, bool withSection, int articleCount, int paragraphCount)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine("lawTitle: 生成規程");
        sb.AppendLine("lawNum: 令和六年規程第十号");
        sb.AppendLine("era: Reiwa");
        sb.AppendLine("year: 6");
        sb.AppendLine("num: 10");
        sb.AppendLine("lawType: Misc");
        sb.AppendLine("lang: ja");
        sb.AppendLine("---");
        for (var c = 1; c <= chapterCount; c++)
        {
            sb.AppendLine($"# 章{c}");
            if (withSection)
            {
                sb.AppendLine("## 節 通則");
            }

            for (var a = 1; a <= articleCount; a++)
            {
                sb.AppendLine(withSection ? $"### 条{c}-{a}" : $"## 条{c}-{a}");
                for (var p = 1; p <= paragraphCount; p++)
                {
                    sb.AppendLine($"[項:p{c}{a}{p}]");
                    sb.AppendLine($"本文{c}{a}{p}。");
                    sb.AppendLine();
                }
            }
        }

        return sb.ToString();
    }
}
