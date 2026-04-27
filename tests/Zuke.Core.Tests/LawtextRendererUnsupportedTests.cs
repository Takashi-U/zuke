using Xunit;

namespace Zuke.Core.Tests;

public class LawtextRendererUnsupportedTests
{
    [Fact]
    public void UnsupportedMarkdownTableProducesDiagnostic()
    {
        const string md = """
---
lawTitle: 表規程
lawNum: 令和六年規程第九号
era: Reiwa
year: 6
num: 9
lawType: Misc
lang: ja
---
| a | b |
| - | - |
| 1 | 2 |
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code is "LMD046");
    }
}
