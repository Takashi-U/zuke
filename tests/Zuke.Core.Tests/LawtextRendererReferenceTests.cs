using Xunit;

namespace Zuke.Core.Tests;

public class LawtextRendererReferenceTests
{
    [Fact]
    public void ResolvesFullAndRelativeReferences()
    {
        var fullMd = TestHelpers.ReadFixture("references.md");
        var relativeMd = TestHelpers.ReadFixture("relative-references.md");

        var fullLawtext = TestHelpers.RenderLawtext(fullMd);
        var relativeLawtext = TestHelpers.RenderLawtext(relativeMd);

        Assert.Contains("第1条第1項に基づく", fullLawtext);
        Assert.Contains("前項に基づく", relativeLawtext);
        Assert.DoesNotContain("{{参照:", fullLawtext + relativeLawtext);
        Assert.DoesNotContain("{{ref:", fullLawtext + relativeLawtext);
    }
}
