using Xunit;
using Zuke.Core.Diff;

namespace Zuke.Core.Tests;

public class HtmlDiffRendererTests
{
    [Fact]
    public void Render_GeneratesSideBySideHtml()
    {
        var result = new LawtextDiffService().Diff("a\nb\n", "a\nc\n", new DiffOptions(3));
        var html = new HtmlDiffRenderer().Render("old.md", "new.md", result);

        Assert.Contains("変更前", html);
        Assert.Contains("変更後", html);
        Assert.Contains("class='del'", html);
        Assert.Contains("class='add'", html);
        Assert.Contains("<style>", html);
        Assert.Contains("Diff:", html);
    }
}
