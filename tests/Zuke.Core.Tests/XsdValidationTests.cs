using Xunit;

namespace Zuke.Core.Tests;

public class XsdValidationTests
{
    [Fact]
    public void Smoke()
    {
        var result = TestHelpers.Compile();
        Assert.NotNull(result.Document);
    }
}
