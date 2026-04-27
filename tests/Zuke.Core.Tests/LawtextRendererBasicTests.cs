using Xunit;

namespace Zuke.Core.Tests;

public class LawtextRendererBasicTests
{
    [Fact]
    public void RendersBasicHeaderAndFormatting()
    {
        var md = TestHelpers.ReadFixture("minimal.md");
        var lawtext = TestHelpers.RenderLawtext(md);

        var lines = lawtext.Split('\n');
        Assert.Equal("施設利用規程", lines[0]);
        Assert.Equal("（令和六年規程第一号）", lines[1]);
        Assert.Equal(string.Empty, lines[2]);
        Assert.EndsWith("\n", lawtext);
        Assert.DoesNotContain("\r", lawtext);
        Assert.DoesNotContain("🍣", lawtext);
        Assert.DoesNotContain("#", lawtext);
        Assert.DoesNotContain(lawtext.Split('\n'), l => l.EndsWith(" "));
    }
}
