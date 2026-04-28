using Xunit;
using Zuke.Core.Diff;

namespace Zuke.Core.Tests;

public class DiffSmokeTests
{
    [Fact]
    public void UnifiedDiff_IncludesHeaderAndHunk()
    {
        var result = new LawtextDiffService().Diff("a\nb\nc\n", "a\nb2\nc\n", new DiffOptions(1));
        var text = new UnifiedDiffRenderer().Render("old.txt", "new.txt", result);
        Assert.Contains("--- old.txt", text);
        Assert.Contains("+++ new.txt", text);
        Assert.Contains("@@ -", text);
        Assert.Contains("-b", text);
        Assert.Contains("+b2", text);
    }
}
