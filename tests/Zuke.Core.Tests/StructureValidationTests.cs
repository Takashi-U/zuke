using Xunit;

namespace Zuke.Core.Tests;

public class StructureValidationTests
{
    [Fact]
    public void ChapterSectionAndArticleMixed_IsLmd041()
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
# 総則
## 節 通則
### 目的
本文
## 章直下条文
本文
""";
        var r = TestHelpers.Compile(md);
        Assert.Contains(r.Diagnostics, d => d.Code == "LMD041");
    }
}
