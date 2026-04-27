using Xunit;

namespace Zuke.Core.Tests;

public class LawtextRendererParagraphTests
{
    [Fact]
    public void RendersSecondAndThirdParagraphNumbers()
    {
        const string md = """
---
lawTitle: 段落規程
lawNum: 令和六年規程第七号
era: Reiwa
year: 6
num: 7
lawType: Misc
lang: ja
---
# 総則
## 届出
[項:a]
第一項。

[項:b]
第二項。

[項:c]
第三項。
""";
        var lawtext = TestHelpers.RenderLawtext(md);

        Assert.Contains("第一条　第一項。", lawtext);
        Assert.Contains("２　第二項。", lawtext);
        Assert.Contains("３　第三項。", lawtext);
        Assert.DoesNotContain("[項:", lawtext);
    }
}
