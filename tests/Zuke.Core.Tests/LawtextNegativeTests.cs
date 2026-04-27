using Xunit;

namespace Zuke.Core.Tests;

public class LawtextNegativeTests
{
    [Fact]
    public void UnresolvedReferenceProducesLmd021()
    {
        const string md = """
---
lawTitle: 失敗規程
lawNum: 令和六年規程第十一号
era: Reiwa
year: 6
num: 11
lawType: Misc
lang: ja
---
## 条
{{参照:存在しない参照名}}
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD021");
    }

    [Fact]
    public void RelativeReferenceFailureProducesLmd027()
    {
        const string md = """
---
lawTitle: 相対失敗
lawNum: 令和六年規程第十二号
era: Reiwa
year: 6
num: 12
lawType: Misc
lang: ja
---
## 条
[項:先頭]
本文。

[項:二]
本文。

[項:三]
{{参照:先頭|相対}}
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD027");
    }

    [Fact]
    public void DuplicateReferenceNameProducesLmd022()
    {
        const string md = """
---
lawTitle: 重複規程
lawNum: 令和六年規程第十三号
era: Reiwa
year: 6
num: 13
lawType: Misc
lang: ja
---
## 条A [条:届出]
本文。
## 条B [条:届出]
本文。
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD022");
    }
}
