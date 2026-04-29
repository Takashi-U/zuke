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
        if (text.Contains("38")) Assert.Equal(new[] { 3, 2 }, n.BranchNumbers);
        if (text.Contains("19条の2") || text.Contains("十九条の二")) Assert.Equal(new[] { 2 }, n.BranchNumbers);
    }


    [Theory]
    [InlineData("第9条の")]
    [InlineData("第9条のA")]
    [InlineData("第9条の0")]
    [InlineData("第9条の二の")]
    public void ParseArticleNumber_Invalid_ReturnsFalse(string text)
    {
        var ok = ArticleNumberParser.TryParseArticleNumber(text, out _);
        Assert.False(ok);
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
