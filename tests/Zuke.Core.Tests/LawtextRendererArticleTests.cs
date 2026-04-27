using Xunit;

namespace Zuke.Core.Tests;

public class LawtextRendererArticleTests
{
    [Fact]
    public void RendersCaptionsAndContinuousArticleNumbering()
    {
        var md = TestHelpers.ReadFixture("chapter-section.md");
        var lawtext = TestHelpers.RenderLawtext(md);

        Assert.Contains("  （目的）", lawtext);
        Assert.Contains("第一条　", lawtext);
        Assert.Contains("第二条　", lawtext);
        Assert.DoesNotContain("[条:", lawtext);
    }
}
