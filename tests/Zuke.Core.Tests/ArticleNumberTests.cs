using Xunit;
using Zuke.Core.Numbering;

namespace Zuke.Core.Tests;

public class ArticleNumberTests
{
    [Theory]
    [InlineData("第19条",19)]
    [InlineData("第19条の2",19)]
    [InlineData("第十九条",19)]
    [InlineData("第十九条の二",19)]
    [InlineData("第38条の3の2",38)]
    [InlineData("第三十八条の三の二",38)]
    public void ParseArticleNumber_Works(string text, int expectedBase)
    {
        var ok = ArticleNumberFormatter.TryParseArticleNumber(text, out var n);
        Assert.True(ok);
        Assert.Equal(expectedBase, n.BaseNumber);
    }

    [Fact]
    public void Formatter_Works()
    {
        var n = new ArticleNumber(19, [2]);
        Assert.Equal("19_2", ArticleNumberFormatter.ToXmlNum(n));
        Assert.Equal("第十九条の二", ArticleNumberFormatter.ToArticleTitle(n, false));
        Assert.Equal("第19条の2", ArticleNumberFormatter.ToArticleTitle(n, true));
        Assert.Equal("article-19-2", ArticleNumberFormatter.ToReferenceName(n));
    }
}
