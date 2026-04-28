using Xunit;
using Zuke.Core.References;

namespace Zuke.Core.Tests;

public class ReferenceNameNormalizerTests
{
    [Theory]
    [InlineData("届出義務", "届出義務")]
    [InlineData("fee-payment", "fee-payment")]
    [InlineData("ｆｅｅ－ｐａｙｍｅｎｔ", "fee-payment")]
    public void ValidNames_AreNormalized(string raw, string expected)
    {
        Assert.True(ReferenceNameNormalizer.TryNormalize(raw, out var normalized));
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("届出 義務")]
    [InlineData("届出:義務")]
    [InlineData("届出（義務）")]
    public void InvalidNames_AreRejectedAndEmitLmd023(string raw)
    {
        Assert.False(ReferenceNameNormalizer.TryNormalize(raw, out _));

        var md = $$"""
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条 [条:{{raw}}]
本文
""";
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD023");
    }
}
