using Xunit;

namespace Zuke.Core.Tests;

public class LawtextRendererItemTests
{
    [Fact]
    public void RendersItemsAndSubitem1()
    {
        var md = TestHelpers.ReadFixture("items.md");
        var lawtext = TestHelpers.RenderLawtext(md);

        Assert.Contains("一　無断欠勤をしないこと。", lawtext);
        Assert.Contains("二　秘密情報を漏らさないこと。", lawtext);
        Assert.Contains("  イ　社外への送信をしないこと。", lawtext);
        Assert.Contains("  ロ　私的利用をしないこと。", lawtext);
        Assert.DoesNotContain("[号:", lawtext);
    }
}
