using Xunit;

namespace Zuke.Core.Tests;

public class RelativeReferenceTests
{
    [Fact]
    public void RelativeReferenceFailure_IsLmd027()
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
## A [a:a]
[p:p1]
本文

## B [a:b]
[p:p2]
{{ref:p1|relative}}
""";
        var r = TestHelpers.Compile(md);
        Assert.Contains(r.Diagnostics, d => d.Code == "LMD027");
    }
}
