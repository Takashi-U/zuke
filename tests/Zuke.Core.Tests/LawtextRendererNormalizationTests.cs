using Zuke.Core.Rendering;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextRendererNormalizationTests
{
    [Fact]
    public void NormalizerNormalizesLineEndingsAndTrailingWhitespace()
    {
        var normalizer = new LawtextNormalizer();
        var text = "第一条　本文。  \r\n\r\n\r\n第二条　本文。\r";
        var normalized = normalizer.Normalize(text, new LawtextNormalizeOptions { CollapseExcessBlankLines = true });

        Assert.DoesNotContain('\r', normalized);
        Assert.Contains("\n\n", normalized);
        Assert.DoesNotContain("\n\n\n\n", normalized);
        Assert.EndsWith("\n", normalized);
    }
}
