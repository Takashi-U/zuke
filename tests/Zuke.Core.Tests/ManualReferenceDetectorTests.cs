using Xunit;

namespace Zuke.Core.Tests;

public class ManualReferenceDetectorTests
{
    [Fact]
    public void DetectsReferencesAcrossTreeAndStrictMode()
    {
        var md = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
# 第一章 総則
## 第一条
第3条を参照する。
## 第二節 詳細
### 第二条
前項を準用する。
### 第三条
- 第1号に従う。
- イ 同号を読み替える。
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD020" && d.Severity == Zuke.Core.Model.DiagnosticSeverity.Warning);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD028" && d.Severity == Zuke.Core.Model.DiagnosticSeverity.Warning);

        var strict = TestHelpers.Compile(md, strict: true);
        Assert.Contains(strict.Diagnostics, d => d.Code == "LMD020" && d.Severity == Zuke.Core.Model.DiagnosticSeverity.Error);
        Assert.Contains(strict.Diagnostics, d => d.Code == "LMD028" && d.Severity == Zuke.Core.Model.DiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData("第三条第二項", "LMD020")]
    [InlineData("次条", "LMD028")]
    [InlineData("同項", "LMD028")]
    public void DetectsKanjiAndRelativePatterns(string token, string expectedCode)
    {
        var md = $$"""
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条
{{token}}に従う。
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == expectedCode);
    }

    [Theory]
    [InlineData("{{参照:第3条}}", "LMD020")]
    [InlineData("{{参照:第三条}}", "LMD020")]
    [InlineData("{{ref:article-3}}", "LMD020")]
    public void IgnoresNumericPatternsInsideReferenceMacros(string macroText, string code)
    {
        var md = $$"""
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条
{{macroText}}に従う。
""";
        var result = TestHelpers.Compile(md);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == code);
    }

    [Fact]
    public void DetectsNumericPatternInNormalBodyText()
    {
        var md = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条
第3条に従う。
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD020");
    }

    [Fact]
    public void IgnoresRelativePatternsInsideReferenceMacros()
    {
        var md = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条
{{参照:前項}}に従う。
""";
        var result = TestHelpers.Compile(md);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "LMD028");
    }

    [Fact]
    public void DetectsRelativePatternInNormalBodyText()
    {
        var md = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条
前項に従う。
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD028");
    }
}
