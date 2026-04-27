using Xunit;

namespace Zuke.Core.Tests;

public class XmlRenderingTests
{
    [Fact]
    public void Smoke()
    {
        var result = TestHelpers.Compile();
        Assert.NotNull(result.Document);
    }
}
