using Xunit;

namespace Zuke.Core.Tests;

public class ReferenceResolverTests
{
    [Fact]
    public void InvalidOption_IsLmd026()
    {
        var md = TestHelpers.ReadFixture("references.md").Replace("|完全", "|badopt", StringComparison.Ordinal);
        var r = TestHelpers.Compile(md);
        Assert.Contains(r.Diagnostics, d => d.Code == "LMD026");
    }

    [Fact]
    public void DuplicateReference_HasRelatedLocations()
    {
        var md = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## A [a:dup]
本文

## B [a:dup]
本文
""";
        var r = TestHelpers.Compile(md);
        var diag = Assert.Single(r.Diagnostics, d => d.Code == "LMD022");
        Assert.True(diag.RelatedLocations.Count >= 2);
    }
}
